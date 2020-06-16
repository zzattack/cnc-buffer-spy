using System;
using System.Runtime.InteropServices;
using System.Text;

namespace CncBufferSpyClient {
	static class Interop
	{

		[DllImport("CncBufferSpyHook.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
		public static extern bool StartProcess([MarshalAs(UnmanagedType.LPStr)] string lpString);
		
		[DllImport("CncBufferSpyHook.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
		public static extern bool InjectToRunningProcess([MarshalAs(UnmanagedType.LPStr)] string lpString);

		[DllImport("kernel32.dll")]
		public static extern IntPtr OpenProcess(uint processAccess, bool bInheritHandle, int processId);

		[DllImport("psapi.dll")]
		public static extern uint GetModuleFileNameEx(IntPtr hProcess, IntPtr hModule, [Out] StringBuilder lpBaseName, [In][MarshalAs(UnmanagedType.U4)] int nSize);

		[DllImport("psapi.dll", SetLastError = true)]
		public static extern bool EnumProcessModules(IntPtr hProcess, [Out] IntPtr lphModule, uint cb, [MarshalAs(UnmanagedType.U4)] out uint lpcbNeeded);


		[DllImport("kernel32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool CloseHandle(IntPtr hObject);

	}
}
