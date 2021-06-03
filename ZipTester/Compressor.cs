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

		public static void FileCompressor(string sourcePath, string interval, CompressionLevel compressionLevel = CompressionLevel.Fastest)
		{
			int inter = Convert.ToInt32(interval);
			DateTime pivotDate = DateTime.Today.AddDays(-1);

			string zipFilePath = Path.Combine(Path.GetFullPath(Path.Combine(sourcePath, @"..\")), "LOGS.zip");
			List<string> saveFiles = QueryTargets(sourcePath, inter);

			
			#region QueryTargets
			//for (DateTime dateToGet = DateTime.Today.AddDays(-inter); dateToGet < pivotDate; dateToGet = dateToGet.AddDays(1))
			//         {
			//             foreach (string folder in folderNames)
			//             {
			//                 string dir = Path.Combine(sourcePath, folder);
			//		if (folder == "LOG")
			//		{
			//			var matchedFileFolderQueryLOGS = (from fileOrFolder in GetTargetFilesNFoldersList(dir, new List<string>())
			//											  where fileOrFolder.Contains(dateToGet.ToString("yyyy-MM-dd"))
			//											  select fileOrFolder).ToList();

			//                     saveLogFiles.AddRange(matchedFileFolderQueryLOGS);
			//		}
			//		else
			//		{
			//			var matchedFileFolderQueryNGOK = (from fileOrFolder in GetTargetFilesNFoldersList(dir, new List<string>())
			//											  where fileOrFolder.Contains(dateToGet.ToString("yyyyMMdd"))
			//											  select fileOrFolder).ToList();

			//                     saveOKNGFolders.AddRange(matchedFileFolderQueryNGOK);
			//		}
			//             }
			//         }
			#endregion
			
			if (File.Exists(zipFilePath))
			{
				using (ZipArchive archive = ZipFile.Open(zipFilePath, ZipArchiveMode.Update))
				{
					archive.Entries
						.Where(x => x.FullName.Contains(pivotDate.AddDays(-inter).ToString("yyyy-MM-dd"))
								|| x.FullName.Contains(pivotDate.AddDays(-inter).ToString("yyyyMMdd"))).ToList()
						.ForEach(y =>
					{
						archive.GetEntry(y.FullName).Delete();
					});

					foreach (var item in saveFiles)
					{
						archive.CreateEntryFromFile(item, item.Substring(sourcePath.Length + 1));
					}
				}
			}
			using (FileStream fs = new FileStream(zipFilePath, FileMode.Create, FileAccess.ReadWrite))
			{
				using (ZipArchive archive = new ZipArchive(fs, ZipArchiveMode.Create))
				{
					foreach (var item in saveFiles)
					{
						string toBeZipped = item.Substring(sourcePath.Length + 1);
						archive.CreateEntryFromFile(item, toBeZipped);
					}
				}
			}
		}

		public static List<string> QueryTargets(string path, int interval)
		{
			DateTime pivotDate = DateTime.Today.AddDays(-1);
			List<string> query = new List<string>();
			for (DateTime dateToGet = DateTime.Today.AddDays(-interval); dateToGet < pivotDate; dateToGet = dateToGet.AddDays(1))
			{
				var matchedFileFolderQuery = (from fileOrFolder in GetFilesNFoldersList(path, new List<string>())
											  where fileOrFolder.Contains(dateToGet.ToString("yyyy-MM-dd")) || fileOrFolder.Contains(dateToGet.ToString("yyyyMMdd"))
											  select fileOrFolder).ToList();
				query.AddRange(matchedFileFolderQuery);
			}
			return query;
		}

		public static DateTime StringToDateTimeParser(string sourcePath, string path, string folderName)
		{
			// DO NOT FORGET TO INPUT CULTUREINFO TO GET EXACT DATEFORMAT
			DateTime dtDate;
			string parseToTime = path.Substring(Path.Combine(sourcePath, folderName).Length + 1, 10);
			if (!DateTime.TryParse(parseToTime, out dtDate))
			{
				return DateTime.Today;
			}
			return dtDate;
		}

		public static void DateTimeParser(string sourcePath, string filePath, int timeToZIP, int timeToDEL)
		{
			DateTime exeDay = DateTime.Today;
			DateTime zipDay = exeDay.AddDays(-timeToZIP);
			DateTime delDay = exeDay.AddDays(-timeToZIP - timeToDEL);

			List<string> fileList = new List<string>();
		}

		public static bool CompareTime(DateTime standard, DateTime comparison)
		{
			int result = DateTime.Compare(standard, comparison);
			if (result > 0)
			{
				return false;
			}
			else if (result < 0)
			{
				return true;
			}
			return true;
		}
		public static List<string> GetFilesNFoldersList(string rootPath, List<string> fileList, DateTime today, DateTime zip, DateTime del)
		{
			if (fileList == null)
			{
				return null;
			}
			var attr = File.GetAttributes(rootPath);
			if (attr == FileAttributes.Directory)
			{
				var dirInfo = new DirectoryInfo(rootPath);
				foreach (var dir in dirInfo.GetDirectories())
				{
					GetFilesNFoldersList(dir.FullName, fileList, today, zip, del);
				}
				foreach (var file in dirInfo.GetFiles())
				{
					GetFilesNFoldersList(file.FullName, fileList, today, zip, del);
				}
			}
			else
			{
				var fileInfo = new FileInfo(rootPath);
				DateTime myDay = StringToDateTimeParser(rootPath, fileInfo.FullName, fileInfo.DirectoryName.Substring(rootPath.Length + 1));
				if (DateTime.Compare(myDay, zip) > 0)
				{

				}
				File.Delete(fileInfo.FullName);
				fileList.Add(fileInfo.FullName);
			}
			return fileList;
		}
	}
}