using System.Runtime.InteropServices;

namespace CncBufferSpyClient {

	public enum PipeMessageType : uint {
		FrameAvailable,
		FrameRequest
	}

	public enum PipeBuffer : uint {
		Buffer1,
		Buffer2
	}

	public struct PipeRequest {
		public PipeBuffer buffer;
		public BufferType type;
	}

	public enum BufferType : uint {
		Depth,
		Shadow,
		Shroud
	}

	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	public struct PipeFrame {
		public uint Width;
		public uint Height;
		public BufferType type;
		public uint framenr;
		public PipeBuffer buffer;
		public uint frame_memory_start;
		public uint anchor_offset;
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
