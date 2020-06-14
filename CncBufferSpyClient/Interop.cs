using System.Runtime.InteropServices;

namespace CncBufferSpyClient {
	static class Interop
	{

		[DllImport("CncBufferSpyHook.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
		public static extern bool StartProcess([MarshalAs(UnmanagedType.LPStr)] string lpString);
		
		[DllImport("CncBufferSpyHook.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
		public static extern bool InjectToRunningProcess([MarshalAs(UnmanagedType.LPStr)] string lpString);

	}
}
