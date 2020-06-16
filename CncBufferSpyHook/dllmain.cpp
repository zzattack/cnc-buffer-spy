#include <atomic>
#include "main.h"
#include "offsets.h"
#include "pipe_proto.h"
#include "pipeserver.h"
#include <windows.h>
#include "detours.h"
#include "ddraw.h"

HINSTANCE sInstanceHandle;
bool sIsHookedInstance = false;
std::shared_ptr<PipeServer> pipeServer;
HANDLE hMap1 = NULL, hMap2 = NULL;
unsigned int mapFileSize = 0;
void* pMapBuf1 = NULL, * pMapBuf2 = NULL;
bool tsHooked = false, ra2Hooked = false, yrHooked = false;
std::atomic<PipeRequest*> open_request;
std::atomic<PipeFrame*> open_frame;

bool EnsureBufferFits(uint32_t size);
bool EnsureBuffersCompatible(DSurface* surface);
bool EnsureBuffersCompatible(ZBuffer* buffer);
bool EnsureBuffersCompatible(IDirectDrawSurface* ddrawSurface, LPDDSURFACEDESC surfaceDesc);
DSurface** getSurfacePointer(OffsetCollection* offsets, BufferType type);
ZBuffer** getZBufferPointer(OffsetCollection* offsets, BufferType type);


int __cdecl GameLoopSpy(OffsetCollection* offsets) {

    int ret = offsets->game_loop();

    // copy depth buffer into mapped area
    PipeRequest* req = open_request.exchange(nullptr);
    if (req)
    {
        bool requestHandled = false;

        if (auto surface = getSurfacePointer(offsets, req->bufferType); surface != nullptr && *surface != nullptr) {
            if (EnsureBuffersCompatible(*surface)) {
                // copy current depth buffer into open_request's frame
                if (req->buffer == DestinationBuffer::Buffer1)
                    memcpy(pMapBuf1, (*surface)->Buffer, mapFileSize);
                else if (req->buffer == DestinationBuffer::Buffer2)
                    memcpy(pMapBuf2, (*surface)->Buffer, mapFileSize);

                // nudge pipe server that request is filled
                PipeMessage msg;
                msg.messageType = PipeMessageType::FrameAvailable;
                msg.frame.frameNumber = *offsets->game_frame;
                msg.frame.width = (*surface)->xs.s.Width;
                msg.frame.height = (*surface)->xs.s.Height;
                msg.frame.bufferType = req->bufferType;
                msg.frame.destinationBuffer = req->buffer;
                msg.frame.sourceBuffer = (*surface)->Buffer;
                msg.frame.sourceAnchor = 0;
                msg.frame.pixelFormat = (*surface)->xs.BytesPerPixel == 2 ? BufferPixelFormat::Format16bpRGB : BufferPixelFormat::Format8bpp;
                msg.frame.bytesPerPixel = (*surface)->xs.BytesPerPixel;
                std::vector<unsigned char> v((unsigned char*)&msg, (unsigned char*)&msg + sizeof(msg));
                pipeServer->writeToPipe(v);
                requestHandled = true;
            }

            else if (EnsureBuffersCompatible((*surface)->DDrawSurface, (*surface)->SurfaceDesc))
            {
                DDSURFACEDESC desc;
                memset(&desc, 0, sizeof(DDSURFACEDESC));
                desc.dwSize = sizeof(DDSURFACEDESC);
                if ((*surface)->DDrawSurface->Lock(nullptr, &desc, DDLOCK_SURFACEMEMORYPTR, nullptr) == DD_OK)
                {
                    if (req->buffer == DestinationBuffer::Buffer1)
                        memcpy(pMapBuf1, desc.lpSurface, mapFileSize);
                    else if (req->buffer == DestinationBuffer::Buffer2)
                        memcpy(pMapBuf2, desc.lpSurface, mapFileSize);
                    (*surface)->DDrawSurface->Unlock(nullptr);

                    PipeMessage msg;
                    msg.messageType = PipeMessageType::FrameAvailable;
                    msg.frame.frameNumber = *offsets->game_frame;
                    msg.frame.width = desc.dwWidth;
                    msg.frame.height = desc.dwHeight;
                    msg.frame.bufferType = req->bufferType;
                    msg.frame.destinationBuffer = req->buffer;
                    msg.frame.sourceBuffer = (uint16_t*)desc.lpSurface;
                    msg.frame.sourceAnchor = 0;
                    msg.frame.pixelFormat = BufferPixelFormat::Format16bpRGB;
                    msg.frame.bytesPerPixel = desc.lPitch / desc.dwWidth;
                    std::vector<unsigned char> v((unsigned char*)&msg, (unsigned char*)&msg + sizeof(msg));
                    pipeServer->writeToPipe(v);
                    requestHandled = true;
                }
            }

        }

        else if (auto zbuf = getZBufferPointer(offsets, req->bufferType); zbuf && *zbuf && EnsureBuffersCompatible(*zbuf)) {
            // copy current depth buffer into open_request's frame
            if (req->buffer == DestinationBuffer::Buffer1)
                memcpy(pMapBuf1, (*zbuf)->data, mapFileSize);
            else if (req->buffer == DestinationBuffer::Buffer2)
                memcpy(pMapBuf2, (*zbuf)->data, mapFileSize);

            // nudge pipe server that request is filled
            PipeMessage msg;
            msg.messageType = PipeMessageType::FrameAvailable;
            msg.frame.frameNumber = *offsets->game_frame;
            msg.frame.width = (*zbuf)->width;
            msg.frame.height = (*zbuf)->height;
            msg.frame.bufferType = req->bufferType;
            msg.frame.destinationBuffer = req->buffer;
            msg.frame.sourceBuffer = (*zbuf)->data;
            msg.frame.sourceAnchor = (*zbuf)->buffer_anchor;
            msg.frame.pixelFormat = BufferPixelFormat::Format16bppGrayscale;
            msg.frame.bytesPerPixel = 2;
            std::vector<unsigned char> v((unsigned char*)&msg, (unsigned char*)&msg + sizeof(msg));
            pipeServer->writeToPipe(v);
            requestHandled = true;
        }

        if (!requestHandled)
        {
            // negative acknowledge
            PipeMessage msg;
            msg.messageType = PipeMessageType::FrameRequestFailed;
            std::vector<unsigned char> v((unsigned char*)&msg, (unsigned char*)&msg + sizeof(msg));
            pipeServer->writeToPipe(v);
        }

        delete req;
    }

    return ret;
}

int __cdecl GameLoopRA2()
{
    return GameLoopSpy(&OffsetsRA2);
}
int __cdecl GameLoopYR()
{
    return GameLoopSpy(&OffsetsYR);
}
int __cdecl GameLoopTS()
{
    return GameLoopSpy(&OffsetsTS);
}

bool EnsureBuffersCompatible(DSurface* surface)
{
    if (surface == nullptr || surface->Buffer == nullptr)
        return false;

    // Create file mappings for 2 buffers
    uint32_t size = surface->xs.s.Width * surface->xs.s.Height * surface->xs.BytesPerPixel;
    return EnsureBufferFits(size);
}

bool EnsureBuffersCompatible(IDirectDrawSurface* ddrawSurface, LPDDSURFACEDESC surfaceDesc)
{
    if (surfaceDesc == nullptr)
        return false;
    return EnsureBufferFits(surfaceDesc->dwHeight * surfaceDesc->lPitch);
}

bool EnsureBuffersCompatible(ZBuffer* buffer)
{
    if (buffer == nullptr || buffer->data == nullptr)
        return false;

    // Create file mappings for 2 buffers
    return EnsureBufferFits(buffer->size);
}

bool EnsureBufferFits(uint32_t size) {
    if (size > 8192 * 8192 * 4)
        return false; // unrealistic size requested

    if (!hMap1 || !hMap2 || !pMapBuf1 || !pMapBuf2 || mapFileSize != size)
    {
        if (pMapBuf1 != nullptr) UnmapViewOfFile(pMapBuf1);
        if (pMapBuf2 != nullptr) UnmapViewOfFile(pMapBuf2);
        if (hMap1 != nullptr) CloseHandle(hMap1);
        if (hMap2 != nullptr) CloseHandle(hMap2);

        hMap1 = CreateFileMapping(INVALID_HANDLE_VALUE, NULL, PAGE_READWRITE, 0, size, "cnc_buffer_spy1");
        hMap2 = CreateFileMapping(INVALID_HANDLE_VALUE, NULL, PAGE_READWRITE, 0, size, "cnc_buffer_spy2");

        if (hMap1 && hMap2) {
            mapFileSize = size;

            pMapBuf1 = MapViewOfFile(hMap1, FILE_MAP_WRITE, 0, 0, mapFileSize);
            pMapBuf2 = MapViewOfFile(hMap2, FILE_MAP_WRITE, 0, 0, mapFileSize);

            if (!pMapBuf1 || !pMapBuf2) {
                Log("Could not obtain map view");
                //MessageBox(NULL, "Could not obtain map view", "Error", 0);
            }
        }
        else
        {
            Log("Could not allocate map space");
            // MessageBox(NULL, "Could not allocate map space", "Error", 0);
        }
    }

    return hMap1 && hMap2 && pMapBuf1 && pMapBuf2 && mapFileSize == size;
}


DSurface** getSurfacePointer(OffsetCollection* offsets, BufferType type)
{
    switch (type)
    {
        case BufferType::SurfaceTile: return offsets->tiles;
        case BufferType::SurfacePrimary: return offsets->primary;
        case BufferType::SurfaceSidebar: return offsets->sidebar;
        case BufferType::SurfaceHidden: return offsets->hidden;
        case BufferType::SurfaceAlternative: return offsets->alt;
        case BufferType::SurfaceTemp: return offsets->temp;
        case BufferType::SurfaceComposite: return offsets->composite;
        case BufferType::SurfaceCloak: return offsets->cloak;
        default: return nullptr;
    }
}

ZBuffer** getZBufferPointer(OffsetCollection* offsets, BufferType type)
{
    switch (type)
    {
        case BufferType::DepthBuffer: return offsets->depth_buffer;
        case BufferType::ShroudBuffer: return offsets->shroud_buffer;
        default: return nullptr;
    }
}


void Log(const char* format, ...)
{
    // Route printf style to OutputDebugString
    char buffer[256];
    va_list args;
    va_start(args, format);
    vsnprintf(buffer, 255, format, args);

    //do something with the error
    OutputDebugString(buffer);

    va_end(args);
}

void OnDataReceived(std::vector<unsigned char> data)
{
    Log("Data received (%d)", data.size());

    // let's just deal with this data as if it always contains a full frame.
    // ideally we'd wrap it around a protocol interpreter that allows buffering.
    if (data.size() == sizeof(PipeMessage))
    {
        const PipeMessage* msg = (PipeMessage*)&data[0];
        if (msg->messageType == PipeMessageType::FrameRequest)
        {
            auto req = new PipeRequest;
            *req = msg->request;
            auto pending = open_request.exchange(req);
            if (pending) delete pending; // we've overwritten an outstanding request, so delete it
        }
    }
}


/*  Uncomment if you want to track memory allocations.
 *  Useful for finding buffers of which the (estimated) size is known.
 
#include <vector>
#include <map>
std::vector<void*> allocs;
std::map<void*, SIZE_T> allocsMap;
LPVOID(WINAPI* Real_HeapAlloc)(HANDLE hHeap, DWORD dwFlags, DWORD_PTR dwBytes) = HeapAlloc;
LPVOID WINAPI Mine_HeapAlloc(HANDLE hHeap, DWORD dwFlags, SIZE_T dwBytes)
{
    auto rv = Real_HeapAlloc(hHeap, dwFlags, dwBytes);
    if (dwBytes >= 1800 * 800 * 2 && dwBytes < 1920 * 1080 * 4) {
        allocs.push_back(rv);
        allocsMap[rv] = dwBytes;
    }
    return rv;
}

BOOL(WINAPI* Real_HeapFree)(HANDLE hHeap, DWORD dwFlags, LPVOID lpMem) = HeapFree;
BOOL WINAPI Mine_HeapFree(HANDLE hHeap, DWORD dwFlags, LPVOID lpMem)
{
    auto rv = Real_HeapFree(hHeap, dwFlags, lpMem);
    allocsMap.erase(lpMem);
    return rv;
}
*/

bool APIENTRY DllMain(HINSTANCE hInstance, DWORD dwReason, void* lpReserved)
{
    if (DetourIsHelperProcess()) 
        return true;

    sInstanceHandle = hInstance;
    char fn[MAX_PATH];
    GetModuleFileName(NULL, fn, MAX_PATH);
    if (strstr(fn, "SpyClient")) {
        sIsHookedInstance = false;
        return true;
    }
    sIsHookedInstance = true;

    switch (dwReason) {
        case DLL_PROCESS_ATTACH: {
            InitOffsets();

            DisableThreadLibraryCalls(sInstanceHandle);

            bool hookSuccess = true;
            /*DetourTransactionBegin();
            DetourUpdateThread(GetCurrentThread());
            DetourAttach(&(PVOID&)Real_HeapAlloc, Mine_HeapAlloc);
            hookSuccess &= DetourTransactionCommit() == NO_ERROR;
            DetourTransactionBegin();
            DetourUpdateThread(GetCurrentThread());
            DetourAttach(&(PVOID&)Real_HeapFree, Mine_HeapFree);
            hookSuccess &= DetourTransactionCommit() == NO_ERROR;*/

            if (*OffsetsRA2.sku == 0x2100 /*&& *ra2_offsets.version == 0x10006*/) {
                DetourTransactionBegin();
                DetourUpdateThread(GetCurrentThread());
                DetourAttach(&(PVOID&)OffsetsRA2.game_loop, GameLoopRA2);
                hookSuccess &= DetourTransactionCommit() == NO_ERROR;
                ra2Hooked = true;
            }
            else if (*OffsetsYR.sku == 0x2900 /*&& *yr_offsets.version == 0x10001*/) {
                DetourTransactionBegin();
                DetourUpdateThread(GetCurrentThread());
                DetourAttach(&(PVOID&)OffsetsYR.game_loop, GameLoopYR);
                hookSuccess &= DetourTransactionCommit() == NO_ERROR;
                yrHooked = true;
            }
            else if (*OffsetsTS.sku == 0x1200 && *OffsetsTS.version == 0x20003) {
                DetourTransactionBegin();
                DetourUpdateThread(GetCurrentThread());
                DetourAttach(&(PVOID&)OffsetsTS.game_loop, GameLoopTS);
                hookSuccess &= DetourTransactionCommit() == NO_ERROR;
                tsHooked = true;
            }
            else {
                // unknown game
                hookSuccess = false;
            }

            if (hookSuccess)
                pipeServer.reset(new PipeServer(OnDataReceived));
            else
                MessageBox(NULL, "Hooking failed, not starting pipe server", "Error", 0);

            return true;
        }

        case DLL_PROCESS_DETACH:
            DetourTransactionBegin();
            DetourUpdateThread(GetCurrentThread());

            // DetourDetach(&(PVOID&)Real_HeapAlloc, Mine_HeapAlloc);
            // DetourDetach(&(PVOID&)Real_HeapFree, Mine_HeapFree);

            if (ra2Hooked)
                DetourDetach(&(PVOID&)OffsetsRA2.game_loop, GameLoopRA2);
            if (yrHooked)
                DetourDetach(&(PVOID&)OffsetsYR.game_loop, GameLoopYR);
            if (tsHooked)
                DetourDetach(&(PVOID&)OffsetsTS.game_loop, GameLoopTS);

            DetourTransactionCommit();

            if (pMapBuf1) UnmapViewOfFile(pMapBuf1);
            if (pMapBuf2) UnmapViewOfFile(pMapBuf2);
            if (hMap1) CloseHandle(hMap1);
            if (hMap2) CloseHandle(hMap2);
            pMapBuf1 = pMapBuf2 = NULL;
            hMap1 = hMap2 = NULL;
            pipeServer.reset();

            return true;

        case DLL_THREAD_ATTACH:
            return true;

        case DLL_THREAD_DETACH:
            return true;
    }
    return true;
}