#pragma once
#include <cstdint>
#include <libloaderapi.h>
#include "ddraw.h"

const unsigned long ImageBase = (unsigned long)GetModuleHandle(NULL);
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
    uint16_t BufferPtr;
    uint32_t Size;
    BOOL IsAllocated;
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

struct OffsetCollection
{
    unsigned int* sku;
    unsigned int* version;
    unsigned int* const game_frame;
    GameLoop game_loop;
    ZBuffer** depth_buffer = nullptr;
    ZBuffer** shroud_buffer = nullptr;
    DSurface** tiles = nullptr;
    DSurface** primary = nullptr;
    DSurface** sidebar = nullptr;
    DSurface** hidden = nullptr;
    DSurface** alt = nullptr;
    DSurface** temp = nullptr;
    DSurface** composite = nullptr;
    DSurface** cloak = nullptr;
};

OffsetCollection OffsetsRA2 = {
    .sku = (unsigned int*)0x00743070,
    //.version = (unsigned int*)0x00A6f3f4, // not always readable soon enough
    .game_frame = (unsigned int*)0x00A40D2C,
    .game_loop = (GameLoop)0x0053fbd0,
    .depth_buffer = (ZBuffer**)0x00839C6C,
    .shroud_buffer = (ZBuffer**)0x008315B4,
};

OffsetCollection OffsetsYR = {
    .sku = (unsigned int*)0x0084A234,
    //.version= (unsigned int*)(0x00843118), // unreliable offset
    .game_frame = (unsigned int*)0x00A8b564, 
    .game_loop = (GameLoop)0x0055d360,
    .depth_buffer = (ZBuffer**)0x00887644,
    .shroud_buffer = (ZBuffer**)0x0087E8A4,
    .tiles = (DSurface**)0x008872FC,
    .primary = (DSurface**)0x00887308,
    .sidebar = (DSurface**)0x00887300,
    .hidden = (DSurface**)0x0088730C,
    .alt = (DSurface**)0x00887310,
    .temp = (DSurface**)0x00887314,
    .composite = (DSurface**)0x0088731C,
    .cloak = (DSurface**)0x0089DDC0,
};

OffsetCollection OffsetsTS = {
    .sku = (unsigned int*)0x006931A8,
    .version = (unsigned int*)0x00697604,
    .game_frame = (unsigned int*)0x007E4924,
    .game_loop = (GameLoop)0x00508A40,
    .depth_buffer = (ZBuffer**)0x0074C8F4,
    .shroud_buffer = (ZBuffer**)0x007474A8,
    .cloak = (DSurface**)0x00760540,
};
