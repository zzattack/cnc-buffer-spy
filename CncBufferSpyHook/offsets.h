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
    BOOL InVideoMemory;
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
    .sku = (unsigned int*)(ImageBase + 0x00343070),
    //.version = (unsigned int*)(ImageBase + 0x0066f3f4), // not always readable soon enough
    .game_frame = (unsigned int*)(ImageBase + 0x00640D2C),
    .game_loop = (GameLoop)(ImageBase + 0x0013fbd0),
    .tiles = (DSurface**)(ImageBase + 0x00439C6C),
};

OffsetCollection OffsetsYR = {
    .sku = (unsigned int*)(ImageBase + 0x0044A234),
    //.version= (unsigned int*)(ImageBase + 0x00443118), // unreliable offset
    .game_frame = (unsigned int*)(ImageBase + 0x0068b564), 
    .game_loop = (GameLoop)(ImageBase + 0x0015d360),
    .depth_buffer = (ZBuffer**)(ImageBase + 0x00487644),
    .tiles = (DSurface**)(ImageBase + 0x004872FC),
    .primary = (DSurface**)(ImageBase + 0x00487308),
    .sidebar = (DSurface**)(ImageBase + 0x00487300),
    .hidden = (DSurface**)(ImageBase + 0x0048730C),
    .alt = (DSurface**)(ImageBase + 0x00487310),
    .temp = (DSurface**)(ImageBase + 0x00487314),
    .composite = (DSurface**)(ImageBase + 0x0048731C),
    .cloak = (DSurface**)(ImageBase + 0x0049DDC0),
};

OffsetCollection OffsetsTS = {
    .sku = (unsigned int*)(ImageBase + 0x002931A8),
    .version = (unsigned int*)(ImageBase + 0x00297604),
    .game_frame = (unsigned int*)(ImageBase + 0x003E4924),
    .game_loop = (GameLoop)(ImageBase + 0x00108A40),
    .depth_buffer = (ZBuffer**)(ImageBase + 0x0034C8F4),
    // .shroud_buffer = (buffers_base_t**)(ImageBase + 0x003474A8),
    .cloak = (DSurface**)(ImageBase + 0x00360540),
};
