#pragma once
#include <cstdint>
#include <libloaderapi.h>

const unsigned long ImageBase = (unsigned long)GetModuleHandle(NULL);
typedef int(__cdecl* GameLoop)();

struct buffers_base_t
{
    uint32_t x;
    uint32_t y;
    uint32_t width;
    uint32_t height;
    uint32_t buffer_start; // anchor to memory offset that contains info for pixel 0,0. loops around.
    uint32_t buffer_end;
    uint16_t* buffer;
};

struct offsets_t
{
    unsigned int* sku;
    unsigned int* version;
    unsigned int* const game_frame;
    GameLoop game_loop;
    buffers_base_t** depth_buffer;
    // buffers_base_t** shroud_buffer;
};

offsets_t ra2_offsets = {
    .sku = (unsigned int*)(ImageBase + 0x00343070),
    //.version = (unsigned int*)(ImageBase + 0x0066f3f4), // not always readable soon enough
    .game_frame = (unsigned int*)(ImageBase + 0x00640D2C),
    .game_loop = (GameLoop)(ImageBase + 0x0013fbd0),
    .depth_buffer = (buffers_base_t**)(ImageBase + 0x00439C6C),
};

offsets_t yr_offsets = {
    .sku = (unsigned int*)(ImageBase + 0x0044A234),
    //.version= (unsigned int*)(ImageBase + 0x00443118), // unreliable offset
    .game_frame = (unsigned int*)(ImageBase + 0x0068b564), 
    .game_loop = (GameLoop)(ImageBase + 0x0015d360),
    .depth_buffer = (buffers_base_t**)(ImageBase + 0x00487644),
};

offsets_t ts_offsets = {
    .sku = (unsigned int*)(ImageBase + 0x002931A8),
    .version = (unsigned int*)(ImageBase + 0x00297604),
    .game_frame = (unsigned int*)(ImageBase + 0x003E4924),
    .game_loop = (GameLoop)(ImageBase + 0x00108A40),
    .depth_buffer = (buffers_base_t**)(ImageBase + 0x0034C8F4),
    // .shroud_buffer = (buffers_base_t**)(ImageBase + 0x003474A8),
};
