#pragma once
#include <cstdint>

enum class pipe_msg_type : uint32_t
{
    frame_available,
    frame_request
};

enum class pipe_buffer : uint32_t
{
    buffer1,
    buffer2
};

struct pipe_request
{
    // named event to signal when data available
    pipe_buffer buffer;
};

enum class frame_type : uint32_t
{
    depth,
    shadow,
    shroud
};

struct pipe_frame
{
    uint32_t reso_h;
    uint32_t reso_v;
    frame_type type;
    uint32_t framenr;
    pipe_buffer buffer;
    uint32_t frame_memory_start;
    uint32_t anchor_offset;
};

struct pipe_msg
{
    pipe_msg_type msg_type;

    union
    {
        pipe_request request;
        pipe_frame frame;
    };
};