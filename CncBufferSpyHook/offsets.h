#pragma once
#include <cstdint>
#include <libloaderapi.h>
#include "ddraw.h"

typedef int(__cdecl* GameLoop)();

struct Rect
{
    uint32_t x, y, width, height;
};

struct __declspec(align(4)) Surface
{
    void* vftble;
    int Width;
    int Height;
};
struct __declspec(align(4)) Buffer
{
    void* BufferPtr;
    uint32_t Size;
    uint8_t IsAllocated;
};

struct __declspec(align(4)) XSurface
{
    Surface s;
    int LockLevel;
    int BytesPerPixel;
};

struct __declspec(align(4)) BSurface
{
    XSurface xs;
    Buffer Buffer;
};

struct __declspec(align(4)) DSurface
{
    XSurface xs;
    uint16_t* Buffer;
    bool IsAllocated;
    bool InVram;
    uint16_t unknown;
    IDirectDrawSurface* DDrawSurface;
    LPDDSURFACEDESC SurfaceDesc;
};

struct ZBuffer
{
    uint32_t x;
    uint32_t y;
    uint32_t width;
    uint32_t height;
    uint16_t* buffer_anchor; // points to pixel 0,0 in buffer area. loops around.
    void* surface; // points to surface class
    uint16_t* data;
    uint32_t data_end;
    uint32_t size;
    uint32_t buffer_maxValue;
    uint32_t buffer_width;
    uint32_t buffer_height;
    uint32_t unknown[20];
};

typedef ZBuffer ABuffer;

struct OffsetCollection
{
    const unsigned int* sku;
    const unsigned int* version;
    const unsigned int* game_frame;
    GameLoop game_loop;
    ZBuffer** depth_buffer = nullptr;
    ZBuffer** shroud_buffer = nullptr;
    ABuffer** alpha_buffer = nullptr;
    DSurface** tiles = nullptr;
    DSurface** primary = nullptr;
    DSurface** sidebar = nullptr;
    DSurface** hidden = nullptr;
    DSurface** alt = nullptr;
    DSurface** temp = nullptr;
    DSurface** composite = nullptr;
    DSurface** cloak = nullptr;
    BSurface** voxel = nullptr;
    BSurface** voxel2 = nullptr;
};


OffsetCollection OffsetsYR;
OffsetCollection OffsetsRA2;
OffsetCollection OffsetsTS;

void InitOffsets()
{
    OffsetsRA2.sku = (const unsigned int*)0x00743070;
    // OffsetsRA2.version = (const unsigned int*)0x00A6f3f4, // not always readable soon enough
    OffsetsRA2.game_frame = (const unsigned int*)0x00A40D2C;
    OffsetsRA2.game_loop = (GameLoop)0x0053fbd0;
    OffsetsRA2.depth_buffer = (ZBuffer**)0x00839C6C;
    OffsetsRA2.shroud_buffer = (ZBuffer**)0x008315B4;

    OffsetsYR.sku = (const unsigned int*)0x0084A234;
    // OffsetsYR.version= (unsigned int* const)(0x00843118), // unreliable offset
    OffsetsYR.game_frame = (const unsigned int*)0x00A8b564;
    OffsetsYR.game_loop = (GameLoop)0x0055d360;
    OffsetsYR.depth_buffer = (ZBuffer**)0x00887644;
    OffsetsYR.shroud_buffer = (ZBuffer**)0x0087E8A4;
    OffsetsYR.tiles = (DSurface**)0x008872FC;
    OffsetsYR.primary = (DSurface**)0x00887308;
    OffsetsYR.sidebar = (DSurface**)0x00887300;
    OffsetsYR.hidden = (DSurface**)0x0088730C;
    OffsetsYR.alt = (DSurface**)0x00887310;
    OffsetsYR.temp = (DSurface**)0x00887314;
    OffsetsYR.composite = (DSurface**)0x0088731C;
    OffsetsYR.cloak = (DSurface**)0x0089DDC0;

    OffsetsTS.sku = (const unsigned int*)0x006931A8;
    OffsetsTS.version = (const unsigned int*)0x00697604;
    OffsetsTS.game_frame = (const unsigned int*)0x007E4924;
    OffsetsTS.game_loop = (GameLoop)0x00508A40;
    OffsetsTS.depth_buffer = (ZBuffer**)0x0074C8F4;
    OffsetsTS.shroud_buffer = (ZBuffer**)0x007474A8;
    OffsetsTS.alpha_buffer = (ABuffer**)0x007474A8;
    OffsetsTS.tiles = (DSurface**)0x0074C5CC;
    OffsetsTS.primary = (DSurface**)0x0074C5D8;
    OffsetsTS.sidebar = (DSurface**)0x0074C5D0;
    OffsetsTS.hidden = (DSurface**)0x0074C5DC;
    OffsetsTS.alt = (DSurface**)0x0074C5E0;
    OffsetsTS.temp = (DSurface**)0x0074C5E4;
    OffsetsTS.composite = (DSurface**)0x0074C5EC;
    OffsetsTS.cloak = (DSurface**)0x00760540;
    OffsetsTS.voxel = (BSurface**)0x0081FFB8;
    OffsetsTS.voxel2 = (BSurface**)0x008200F0;
}