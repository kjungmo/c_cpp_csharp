using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Compression;

namespace FileSystemTester
{
	class Program
	{
		static void Main(string[] args)
		{ 

			// args : rootPath, startZipDate, startDelDate
			string rootPath = @"e:\ZipTest\";
			List<string> tempList = new List<string>();
			string zipFilePath = Path.Combine(rootPath, "LOG.zip");
			DateTime startZIP = new DateTime(2021, 6, 1);
			DateTime startDEL = new DateTime(2021, 5, 29);

			if (!Directory.Exists(rootPath))
			{
				Console.WriteLine("SourcePath None");
				return;
			}

			Console.WriteLine("SourcePath Exists");
			if (!File.Exists(zipFilePath))
			{
				Console.WriteLine("No .zip File");
				var myfile = File.Create(zipFilePath);
				myfile.Close();
			}

			using (ZipArchive archive = ZipFile.Open(zipFilePath, ZipArchiveMode.Update))
			{
				#region CHECKS THE ZIP FILE, WHICH HAS ALREADY BEEN MADE EARLIER, DELETES AND UPDATES FILES
				foreach (var file in archive.Entries.Where(x => x.FullName.Contains("LOG")))
				{

					if (DateTime.Compare(startDEL.Date, StringToDateTimeParser(file.FullName.Substring(4, 10))) >= 0)
					//if (DateTime.Compare(startDel, file.LastWriteTime.Date) >= 0)
					{
						archive.GetEntry(file.FullName).Delete();
					}
				}
				foreach (var file in archive.Entries.Where(x => x.FullName.Contains("OK") || x.FullName.Contains("NG")))
				{
					if (DateTime.Compare(startDEL.Date, StringToDateTimeParser(file.FullName.Substring(3, 8))) >= 0)
					//if (DateTime.Compare(new DateTime(2021, 5, 29), file.LastWriteTime.Date) >= 0)
					{
						archive.GetEntry(file.FullName).Delete();
					}
				}
				#endregion

				#region FILES ARE ZIPPED, AND WHICH SHOULDVE BEEN ZIPPED BUT NOT ARE DELETED
				ManageLOG(rootPath, startZIP, startDEL, archive);
				ManageOKNG(rootPath, "NG", startZIP, startDEL, tempList, archive);
				ManageOKNG(rootPath, "OK", startZIP, startDEL, tempList, archive);
				#endregion
			} // *****************************************************************************************
			Console.ReadKey();
		}

		// gets every files
		public static List<string> GetFiles(string rootPath, List<string> fileLists)
		{
			var attr = File.GetAttributes(rootPath);
			if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
			{
				var dirInfo = new DirectoryInfo(rootPath);
				foreach (var dir in dirInfo.GetDirectories())
				{
					GetFiles(dir.FullName, fileLists);
				}

				foreach (var file in dirInfo.GetFiles().Where(f => f.Extension == ".log" || f.Extension == ".jpg"))
				{
					GetFiles(file.FullName, fileLists);
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
						Console.WriteLine("\nThis is the Files to be deleted");
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
	}
}
