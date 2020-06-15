#pragma once
#include <cstdint>

enum class PipeMessageType : uint32_t
{
    FrameAvailable,
    FrameRequest,
    FrameRequestFailed,
};

enum class DestinationBuffer : uint32_t
{
    Buffer1,
    Buffer2
};

enum class BufferType : uint32_t
{
    DepthBuffer,
    ShroudBuffer,
    SurfaceTile,
    SurfacePrimary,
    SurfaceSidebar,
    SurfaceHidden,
    SurfaceAlternative,
    SurfaceTemp,
    SurfaceComposite,
    SurfaceCloak
};

struct PipeRequest
{
    // named event to signal when data available
    DestinationBuffer buffer;
    BufferType bufferType;
};

struct PipeFrame
{
    uint32_t width;
    uint32_t height;
    uint32_t bytesPerPixel;
    BufferType bufferType;
    uint32_t frameNumber;
    uint16_t* sourceBuffer;
    uint16_t* sourceAnchor;
    DestinationBuffer destinationBuffer;
};

struct PipeMessage
{
    PipeMessageType messageType;

    union
    {
        PipeRequest request;
        PipeFrame frame;
    };
};