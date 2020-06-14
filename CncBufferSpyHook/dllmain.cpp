#include <atomic>
#include "main.h"
#include "offsets.h"
#include "pipe_proto.h"
#include "pipeserver.h"
#include <windows.h>
#include "detours.h"


HINSTANCE sInstanceHandle;
bool sIsHookedInstance = false;
PipeServer pipeServer;
HANDLE hMap1 = NULL, hMap2 = NULL;
void* pMapBuf1 = NULL, * pMapBuf2 = NULL;
bool tsHooked = false, ra2Hooked = false, yrHooked = false;
std::atomic<pipe_request*> open_request;
std::atomic<pipe_frame*> open_frame;


int __cdecl GameLoopSpy(offsets_t* offsets) {
    // Create file mappings for 2 buffers
    DWORD bufferSize = (*offsets->depth_buffer)->reso_h * (*offsets->depth_buffer)->reso_v * 2;

    if (!hMap1)
    {
        hMap1 = CreateFileMapping(INVALID_HANDLE_VALUE, NULL, PAGE_READWRITE, 0, bufferSize, "cnc_buffer_spy1");
        hMap2 = CreateFileMapping(INVALID_HANDLE_VALUE, NULL, PAGE_READWRITE, 0, bufferSize, "cnc_buffer_spy2");

        if (!hMap1 || !hMap2)
        {
            MessageBox(NULL, "Could not allocate map space", "Error", 0);
        }

        pMapBuf1 = MapViewOfFile(hMap1, FILE_MAP_ALL_ACCESS, 0, 0, bufferSize);
        pMapBuf2 = MapViewOfFile(hMap2, FILE_MAP_ALL_ACCESS, 0, 0, bufferSize);

        if (!pMapBuf1 || !pMapBuf2)
        {
            MessageBox(NULL, "Could not allocate map space", "Error", 0);
        }
    }

    int ret = offsets->game_loop();

    // copy depth buffer into mapped area
    pipe_request* req = open_request.exchange(nullptr);
    if (req)
    {
        // copy current depth buffer into open_request's frame
        if (req->buffer == pipe_buffer::buffer1)
            memcpy(pMapBuf1, (*offsets->depth_buffer)->depth_buffer, bufferSize);
        else if (req->buffer == pipe_buffer::buffer2)
            memcpy(pMapBuf2, (*offsets->depth_buffer)->depth_buffer, bufferSize);

        // nudge pipe server that request is filled
        pipe_msg msg;
        msg.msg_type = pipe_msg_type::frame_available;
        msg.frame.framenr = *offsets->game_frame;
        msg.frame.reso_h = (*offsets->depth_buffer)->reso_h;
        msg.frame.reso_v = (*offsets->depth_buffer)->reso_v;
        msg.frame.type = frame_type::depth;
        msg.frame.buffer = req->buffer;
        msg.frame.frame_memory_start = (uint32_t)(*offsets->depth_buffer)->depth_buffer;
        msg.frame.anchor_offset = (uint32_t)(*offsets->depth_buffer)->anchor_offset;
        std::vector<unsigned char> v((unsigned char*)&msg, (unsigned char*)&msg + sizeof(msg));
        pipeServer.writeToPipe(v);

        delete req;
    }

    return ret;
}

int __cdecl GameLoopRA2()
{
    return GameLoopSpy(&ra2_offsets);
}
int __cdecl GameLoopYR()
{
    return GameLoopSpy(&yr_offsets);
}
int __cdecl GameLoopTS()
{
    return GameLoopSpy(&ts_offsets);
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
    if (data.size() == sizeof(pipe_msg))
    {
        const pipe_msg* msg = (pipe_msg*)&data[0];
        if (msg->msg_type == pipe_msg_type::frame_request)
        {
            auto req = new pipe_request;
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
    sInstanceHandle = hInstance;

    char fn[MAX_PATH];
    GetModuleFileName(NULL, fn, MAX_PATH);
    if (strstr(fn, "SpyClient") || DetourIsHelperProcess()) {
        sIsHookedInstance = false;
        return true;
    }
    sIsHookedInstance = true;

    switch (dwReason) {
        case DLL_PROCESS_ATTACH: {
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

            if (*ra2_offsets.sku == 0x2100 /*&& *ra2_offsets.version == 0x10006*/) {
                DetourTransactionBegin();
                DetourUpdateThread(GetCurrentThread());
                DetourAttach(&(PVOID&)ra2_offsets.game_loop, GameLoopRA2);
                hookSuccess &= DetourTransactionCommit() == NO_ERROR;
                ra2Hooked = true;
            }
            else if (*yr_offsets.sku == 0x2900 /*&& *yr_offsets.version == 0x10001*/) {
                DetourTransactionBegin();
                DetourUpdateThread(GetCurrentThread());
                DetourAttach(&(PVOID&)yr_offsets.game_loop, GameLoopYR);
                hookSuccess &= DetourTransactionCommit() == NO_ERROR;
                yrHooked = true;
            }
            else if (*ts_offsets.sku == 0x1200 && *ts_offsets.version == 0x20003) {
                DetourTransactionBegin();
                DetourUpdateThread(GetCurrentThread());
                DetourAttach(&(PVOID&)ts_offsets.game_loop, GameLoopTS);
                hookSuccess &= DetourTransactionCommit() == NO_ERROR;
                tsHooked = true;
            }
            else {
                // unknown game
                hookSuccess = false;
            }

            if (hookSuccess)
                pipeServer.start(OnDataReceived);
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
                DetourDetach(&(PVOID&)ra2_offsets.game_loop, GameLoopRA2);
            if (yrHooked)
                DetourDetach(&(PVOID&)yr_offsets.game_loop, GameLoopYR);
            if (tsHooked)
                DetourDetach(&(PVOID&)ts_offsets.game_loop, GameLoopTS);

            DetourTransactionCommit();

            if (pMapBuf1) UnmapViewOfFile(pMapBuf1);
            if (pMapBuf2) UnmapViewOfFile(pMapBuf2);
            if (hMap1) CloseHandle(hMap1);
            if (hMap2) CloseHandle(hMap2);
            pMapBuf1 = pMapBuf2 = NULL;
            hMap1 = hMap2 = NULL;

            return true;

        case DLL_THREAD_ATTACH:
            return true;

        case DLL_THREAD_DETACH:
            return true;
    }
    return true;
}