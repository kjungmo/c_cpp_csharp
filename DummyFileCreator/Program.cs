using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DummyFileCreator
{
	class Program
	{
		static void Main(string[] args)
		{
			int dates = -100;
			string logPath = @"D:\ZipTest\LOG";
			string ngPath = @"D:\ZipTest\NG";
			string okPath = @"D:\ZipTest\OK";
			if (Directory.Exists(logPath))
			{
				for (int i = 0; i > dates; i--)
				{
					string datetime = DateTime.Today.AddDays(i).ToString("yyyy-MM-dd");
					string filepath = Path.Combine(logPath, datetime + ".log");
					using (StreamWriter sw = File.CreateText(filepath)) { }

				}
				Console.WriteLine("LOG fertig");
			}

			if (Directory.Exists(ngPath))
			{
				for (int i = 0; i > dates; i--)
				{
					string datetime = DateTime.Today.AddDays(i).ToString("yyyyMMdd");
					string folderpath = Path.Combine(ngPath, datetime);
					if (Directory.Exists(folderpath) == false)
					{
						Directory.CreateDirectory(folderpath);
					}
				}
				Console.WriteLine("NG fertig");
			}

			if (Directory.Exists(okPath))
			{
				for (int i = 0; i > dates; i--)
				{
					string datetime = DateTime.Today.AddDays(i).ToString("yyyyMMdd");
					string folderpath = Path.Combine(okPath, datetime);
					if (Directory.Exists(folderpath) == false)
					{
						Directory.CreateDirectory(folderpath);
					}
				}
				Console.WriteLine("OK fertig");
			}
			Console.ReadKey();
		}
	}
}
