using System.Runtime.InteropServices;

namespace CncBufferSpyClient {

	public enum pipe_msg_type : uint {
		frame_available,
		frame_request
	}

	public enum pipe_buffer : uint {
		buffer1,
		buffer2
	}

	public struct pipe_request {
		public pipe_buffer buffer;
	}

	public enum buffer_type : uint {
		depth,
		shadow,
		shroud
	}

	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	public struct pipe_frame {
		public uint reso_h;
		public uint reso_v;
		public buffer_type type;
		public uint framenr;
		public pipe_buffer buffer;
		public uint frame_memory_start;
		public uint anchor_offset;
	};

	[StructLayout(LayoutKind.Explicit, Pack = 4)]
	public struct pipe_msg {
		[FieldOffset(0)]
		public pipe_msg_type msg_type;

		[FieldOffset(4)]
		public pipe_request request;

		[FieldOffset(4)]
		public pipe_frame frame;
	};

}
