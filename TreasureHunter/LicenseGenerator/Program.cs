using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LicenseGenerator
{
	static class Program
	{
		/// <summary>
		/// 해당 애플리케이션의 주 진입점입니다.
		/// </summary>
		[STAThread]
		static void Main(string[] args)
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			if (args.Length > 0)
			{
				if (args[0].Equals("-W"))
				{
					Application.Run(new LicenseGenerator());
				}
			}
			else 
			{
				Marshal.PtrToStringAnsi(Generator.GenerateLicenseKey(false));
			}
		}
	}
}
