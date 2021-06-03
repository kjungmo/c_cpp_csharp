using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace FileSystemTester
{
	class Program
	{
		static void Main(string[] args)
		{
			string rootPath = @"e:\ZipTest\";

			if (Directory.Exists(rootPath))
			{
				Console.WriteLine("SourcePath Exists");
				List<string> tempList = new List<string>();
				foreach (var file in new DirectoryInfo(Path.Combine(rootPath, "LOG")).GetFileSystemInfos())
				{
					
					if (DateTime.Compare(new DateTime(2021, 6, 1), file.CreationTime.Date) >= 0)
					{

						// Files that needs to be deleted
						if (DateTime.Compare(new DateTime(2021, 5, 29), file.CreationTime.Date) >= 0)
						{
							Console.WriteLine("\nThis is the Files to be deleted");
							Console.WriteLine(file.FullName);
						}

						// Files that needs to be zipped
						else
						{
							Console.WriteLine("to be zipped \n");
							Console.WriteLine(file.FullName);
						}
					}
				}

				foreach (var item in new DirectoryInfo(Path.Combine(rootPath, "OK")).GetFileSystemInfos())
				{
					if (DateTime.Compare(new DateTime(2021, 6, 1), item.CreationTime.Date) >= 0)
					{
						// Files that needs to be deleted
						if (DateTime.Compare(new DateTime(2021, 5, 29), item.CreationTime.Date) >= 0)
						{
							Console.WriteLine("\nThis is the Files to be deleted");
							tempList = GetFiles(item.FullName, tempList);
							foreach (var file in tempList)
							{
								Console.WriteLine(file);
								//File.Delete(file.FullName); // From FILE ZONE, these files are to be deleted
							}
							tempList.Clear();
						}

						// Files that needs to be zipped
						else
						{
							tempList = GetFiles(item.FullName, tempList);
							Console.WriteLine("to be zipped \n");
							foreach (var file in tempList)
							{
								Console.WriteLine(file);
							}
							tempList.Clear();
						}
					}
				}

				foreach (var item in new DirectoryInfo(Path.Combine(rootPath, "NG")).GetFileSystemInfos())
				{
					if (DateTime.Compare(new DateTime(2021, 6, 1), item.CreationTime.Date) >= 0)
					{
						// Files that needs to be deleted
						if (DateTime.Compare(new DateTime(2021, 5, 29), item.CreationTime.Date) >= 0)
						{
							Console.WriteLine("\nThis is the Files to be deleted");
							tempList = GetFiles(item.FullName, tempList);
							foreach (var file in tempList)
							{
								Console.WriteLine(file);
								//File.Delete(file.FullName); // From FILE ZONE, these files are to be deleted
							}
							tempList.Clear();
						}

						// Files that needs to be zipped
						else
						{
							tempList = GetFiles(item.FullName, tempList);
							Console.WriteLine("to be zipped \n");
							foreach (var file in tempList)
							{
								Console.WriteLine(file);
							}
							tempList.Clear();
						}
					}
				}
			}
			else
			{
				Console.WriteLine("SourcePath None");
			}
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
	}
}
