using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZipTester
{
	public static class Compressor
	{
		public static string SourcePath { get; set; }
		public static List<string> TempFileList { get; set; }

		public static DateTime StartZip { get; set; }

		public static DateTime StartDel { get; set; }

		
		public static DateTime StringToDateTimeParser(string date)
		{
			DateTime dtDate;
			string[] pattern = { "yyyyMMdd", "yyyy-MM-dd" };

			if (!DateTime.TryParseExact(date, pattern, System.Globalization.CultureInfo.InvariantCulture,
						   System.Globalization.DateTimeStyles.None, out dtDate))
			{
				return DateTime.Today;
			}
			return dtDate;
		}

		public static List<string> GetFiles(string rootPath, ref List<string> fileLists)
		{
			var attr = File.GetAttributes(rootPath);
			if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
			{
				var dirInfo = new DirectoryInfo(rootPath);
				foreach (var dir in dirInfo.GetDirectories())
				{
					GetFiles(dir.FullName, ref fileLists);
				}

				foreach (var file in dirInfo.GetFiles().Where(f => f.Extension == ".log" || f.Extension == ".jpg"))
				{
					GetFiles(file.FullName, ref fileLists);
				}
			}
			else if ((attr & FileAttributes.Archive) == FileAttributes.Archive)
			{
				var fileInfo = new FileInfo(rootPath);
				if (fileInfo.Extension == ".log" || fileInfo.Extension == ".jpg")
					fileLists.Add(fileInfo.FullName);
			}
			return fileLists;
		}


		public static void ManageLOG(string rootPath, DateTime zip, DateTime del, ZipArchive archive)
		{
			foreach (var file in new DirectoryInfo(Path.Combine(rootPath, "LOG")).GetFileSystemInfos())
			{

				if (DateTime.Compare(zip.Date, StringToDateTimeParser(file.Name.Substring(0, 10))) >= 0)
				//if (DateTime.Compare(new DateTime(2021, 6, 1), file.CreationTime.Date) >= 0)
				{

					// Files that needs to be deleted
					if (DateTime.Compare(del.Date, StringToDateTimeParser(file.Name.Substring(0, 10))) >= 0)
					//if (DateTime.Compare(new DateTime(2021, 5, 29), file.CreationTime.Date) >= 0)
					{
						//Console.WriteLine("\nThis is the Files to be deleted");
						//Console.WriteLine(file.FullName);
						File.Delete(file.FullName);
					}

					// Files that needs to be zipped
					else
					{
						//Console.WriteLine("to be zipped \n");
						//Console.WriteLine(file.FullName);
						archive.CreateEntryFromFile(file.FullName, file.FullName.Substring(rootPath.Length), CompressionLevel.Optimal);
						File.Delete(file.FullName);
					}
				}
			}
		}

		public static void ManageOKNG(string rootPath, string folder, DateTime zip, DateTime del, List<string> temp, ZipArchive archive)
		{
			foreach (var item in new DirectoryInfo(Path.Combine(rootPath, folder)).GetFileSystemInfos())
			{
				if (DateTime.Compare(zip, StringToDateTimeParser(item.FullName.Substring((item.FullName.Length - 8), 8))) >= 0)
				//if (DateTime.Compare(new DateTime(2021, 6, 1), item.CreationTime.Date) >= 0)
				{
					// Files that needs to be deleted
					if (DateTime.Compare(del, StringToDateTimeParser(item.FullName.Substring((item.FullName.Length - 8), 8))) >= 0)
					//if (DateTime.Compare(new DateTime(2021, 5, 29), item.CreationTime.Date) >= 0)
					{
						Console.WriteLine("\nThe Files to be deleted");
						Directory.Delete(item.FullName, recursive: true); // From FILE ZONE, these files are to be deleted
					}

					// Files that needs to be zipped
					else
					{
						temp = GetFiles(item.FullName, temp);
						Console.WriteLine("to be zipped \n");
						foreach (var file in temp)
						{
							Console.WriteLine(file);
							archive.CreateEntryFromFile(file, file.Substring(rootPath.Length), CompressionLevel.Optimal);
							File.Delete(file);
						}
						temp.Clear();
					}
				}
			}
		}

		public static void UpdateZip(DateTime startDEL, ZipArchive archive)
		{
			foreach (var file in archive.Entries.Where(x => x.FullName.Contains("LOG")).ToList())
			{

				if (DateTime.Compare(startDEL.Date, StringToDateTimeParser(file.FullName.Substring(4, 10))) >= 0)
				//if (DateTime.Compare(startDel, file.LastWriteTime.Date) >= 0)
				{
					archive.GetEntry(file.FullName).Delete();
				}
			}
			foreach (var file in archive.Entries.Where(x => x.FullName.Contains("OK") || x.FullName.Contains("NG")).ToList())
			{
				if (DateTime.Compare(startDEL.Date, StringToDateTimeParser(file.FullName.Substring(3, 8))) >= 0)
				//if (DateTime.Compare(new DateTime(2021, 5, 29), file.LastWriteTime.Date) >= 0)
				{
					archive.GetEntry(file.FullName).Delete();
				}
			}
		}

	}
}