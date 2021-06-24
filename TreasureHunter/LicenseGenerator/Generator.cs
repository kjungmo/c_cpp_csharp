using System;
using System.Runtime.InteropServices;

namespace LicenseGenerator
{
	public static class Generator
	{
		[DllImport("Treasure.dll")]
		public static extern IntPtr GenerateLicenseKey([MarshalAs(UnmanagedType.I1)] bool initCOM);
	}
}

