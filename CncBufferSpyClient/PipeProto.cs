using System.Runtime.InteropServices;

namespace CncBufferSpyClient {

	public enum PipeMessageType : uint {
		FrameAvailable,
		FrameRequest,
		FrameRequestFailed,
	}

	public enum DestinationBuffer : uint {
		Buffer1,
		Buffer2
	}

	public struct PipeRequest {
		public DestinationBuffer DestinationBuffer;
		public SurfaceType SurfaceType;
		public uint CustomOffset;
	}

	public enum SurfaceType : uint {
		DepthBuffer,
		ShroudBuffer,
		AlphaBuffer,
		SurfaceTile,
		SurfacePrimary,
		SurfaceSidebar,
		SurfaceHidden,
		SurfaceAlternative,
		SurfaceTemp,
		SurfaceComposite,
		SurfaceCloak,
		SurfaceVoxel,
		SurfaceVoxel2,
		Custom = 0xFFFFFFFE,
		Invalid = 0xFFFFFFFF
	}

	public enum BufferPixelFormat : uint {
		Format8bpp,
		Format16bppGrayscale,
		Format16bppRGB,
	}

	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	public struct PipeFrame {
		public uint Width;
		public uint Height;
		public BufferPixelFormat PixelFormat;
		public uint BytesPerPixel;
		public SurfaceType SurfaceType;
		public uint FrameNumber;
		public uint SourceBufferAddress;
		public uint SourceBufferAnchor;
		public DestinationBuffer DestBuffer;
	};

	[StructLayout(LayoutKind.Explicit, Pack = 4)]
	public struct PipeMessage {
		[FieldOffset(0)]
		public PipeMessageType MessageType;

		[FieldOffset(4)]
		public PipeRequest request;

		[FieldOffset(4)]
		public PipeFrame frame;
	};

}
