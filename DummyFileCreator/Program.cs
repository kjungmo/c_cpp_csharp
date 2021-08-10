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
			Console.WriteLine("How many days to create dummy files : ");
			string userInputDays = Console.ReadLine();
			Console.WriteLine($"Input days : {userInputDays}");
			int convertedDays = Convert.ToInt32(userInputDays);

			if (convertedDays <= 0 && 365 <= convertedDays)
			{
				Console.WriteLine("Invalid days input. 0 <= Days <= 365 ");
				return;
			}

			Console.WriteLine("Log folder name : ");
			string foldername = Console.ReadLine();
			Console.WriteLine($"Input folder name : {foldername}");
			string folder = Path.Combine(@"E:\", foldername);
			
			CreateDummyFilesByFolderLog(folder, "LOG", convertedDays);
			CreateDummyFilesByFolderImage(folder, "NG", convertedDays);
			CreateDummyFilesByFolderImage(folder, "OK", convertedDays);
			CreateDummyFilesByFolderCsv(folder, "VALUES", convertedDays);

			Console.ReadKey();
		}

		private static void CreateDummyFilesByFolderLog(string sourcePath, string folderName, int convertedDays)
        {
			string folderPath = Path.Combine(sourcePath, folderName);
			if (!Directory.Exists(folderPath))
			{
				Directory.CreateDirectory(folderPath);
			}
			for (int i = 0; i < convertedDays; i++)
			{
				string datetime = DateTime.Today.AddDays(-i).ToString("yyyy-MM-dd");
				string filepath = Path.Combine(folderPath, datetime + ".log");
				using (StreamWriter sw = File.CreateText(filepath)) { }

			}
			Console.WriteLine($"{folderName} fertig");
		}

		private static void CreateDummyFilesByFolderImage(string sourcePath, string folderName, int convertedDays)
        {
			string folderPath = Path.Combine(sourcePath, folderName);
			if (!Directory.Exists(folderPath))
			{
				Directory.CreateDirectory(folderPath);
			}
			for (int i = 0; i < convertedDays; i++)
			{
				string datetime = DateTime.Today.AddDays(-i).ToString("yyyyMMdd");
				string folderpath = Path.Combine(folderPath, datetime);
				string carPath = Path.Combine(folderpath, "CN7");
				string taskPath = Path.Combine(carPath, "FineLocalizer");
				if (!Directory.Exists(folderpath))
				{
					Directory.CreateDirectory(folderpath);
				}
				if (!Directory.Exists(carPath))
				{
					Directory.CreateDirectory(carPath);
				}
				if (!Directory.Exists(taskPath))
				{
					Directory.CreateDirectory(taskPath);
				}
				for (int j = 0; j < 15; j++)
				{
					GenerateDummyJpegAt(Path.Combine(taskPath, j.ToString() + "." + ImageFormat.Png.ToString()), "Captured Window", 200, 200);
				}
			}
			Console.WriteLine($"{folderName} fertig");
		}

		private static void CreateDummyFilesByFolderCsv(string sourcePath, string folderName, int convertedDays)
        {
			string folderPath = Path.Combine(sourcePath, folderName);
			if (!Directory.Exists(folderPath))
			{
				Directory.CreateDirectory(folderPath);
			}
			for (int i = 0; i < convertedDays; i++)
			{
				string datetime = DateTime.Today.AddDays(-i).ToString("yyyyMMdd");
				string folderpath = Path.Combine(folderPath, datetime);
				string carPath = Path.Combine(folderpath, "CN7");
				if (!Directory.Exists(folderpath))
				{
					Directory.CreateDirectory(folderpath);
				}
				if (!Directory.Exists(carPath))
				{
					Directory.CreateDirectory(carPath);
				}
				for (int j = 0; j < 10; j++)
				{
					string filepath = Path.Combine(carPath, "values.csv");
					using (StreamWriter sw = File.CreateText(filepath)) { }
				}
			}
			Console.WriteLine($"{folderName} fertig");
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
