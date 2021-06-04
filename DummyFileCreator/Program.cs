using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DummyFileCreator
{
	class Program
	{
		static void Main(string[] args)
		{
			int dates = -5;
			string logPath = @"E:\Dummy\LOG";
			string ngPath = @"E:\Dummy\NG";
			string okPath = @"E:\Dummy\OK";
			if (!Directory.Exists(logPath))
			{
				Directory.CreateDirectory(logPath);
				for (int i = 0; i > dates; i--)
				{
					string datetime = DateTime.Today.AddDays(i).ToString("yyyy-MM-dd");
					string filepath = Path.Combine(logPath, datetime + ".log");
					using (StreamWriter sw = File.CreateText(filepath)) { }

				}
				Console.WriteLine("LOG fertig");
			}

			if (!Directory.Exists(ngPath))
			{
				Directory.CreateDirectory(ngPath);
			}
			for (int i = 0; i > dates; i--)
			{
				string datetime = DateTime.Today.AddDays(i).ToString("yyyyMMdd");
				string folderpath = Path.Combine(ngPath, datetime);
				string carPath = "CN7";
				carPath = Path.Combine(folderpath, carPath);
				if (!Directory.Exists(folderpath))
				{
					Directory.CreateDirectory(folderpath);
				}
				if (!Directory.Exists(carPath))
				{
					Directory.CreateDirectory(carPath);
				}
				for (int j = 0; j < 15; j++)
				{
					GenerateDummyJpegAt(Path.Combine(carPath, j.ToString() + ".png"), "Captured Window", 1920, 1060);
				}
			}
			Console.WriteLine("NG fertig");

			if (!Directory.Exists(okPath))
			{
				Directory.CreateDirectory(okPath);
			}
			for (int i = 0; i > dates; i--)
			{
				string datetime = DateTime.Today.AddDays(i).ToString("yyyyMMdd");
				string folderpath = Path.Combine(okPath, datetime);
				string carPath = "CN7";
				carPath = Path.Combine(folderpath, carPath);
				if (!Directory.Exists(folderpath))
				{
					Directory.CreateDirectory(folderpath);
				}
				if (!Directory.Exists(carPath))
				{
					Directory.CreateDirectory(carPath);
				}
				for (int j = 0; j < 15; j++)
				{
					GenerateDummyJpegAt(Path.Combine(carPath, j.ToString() + ".png"), "Captured Window", 1920, 1060);
				}
			}
			Console.WriteLine("OK fertig");
			Console.ReadKey();
		}

		private static void GenerateDummyJpegAt(string outputPath, string nameToEmbed, int width, int height)
		{
			using (var bmp = new Bitmap(width, height, PixelFormat.Format24bppRgb))
			{
				BitmapData data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.WriteOnly, bmp.PixelFormat);
				byte[] noise = new byte[data.Width * data.Height * 3];
				new Random().NextBytes(noise); // note that if you do that in a loop or from multiple threads - you may want to store this random in outside variable
				Marshal.Copy(noise, 0, data.Scan0, noise.Length);
				bmp.UnlockBits(data);
				using (var g = Graphics.FromImage(bmp))
				{
					// draw white rectangle in the middle
					g.FillRectangle(Brushes.White, 0, height / 2 - 20, width, 40);
					var fmt = new StringFormat();
					fmt.Alignment = StringAlignment.Center;
					fmt.LineAlignment = StringAlignment.Center;
					// draw text inside that rectangle
					g.DrawString(nameToEmbed, SystemFonts.DefaultFont, Brushes.Black, new RectangleF(0, 0, bmp.Width, bmp.Height), fmt);
				}
				using (var fs = File.Create(outputPath))
				{
					bmp.Save(fs, System.Drawing.Imaging.ImageFormat.Jpeg);
				}
			}
		}
	}
}
