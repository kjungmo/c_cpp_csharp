using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace FileSystemTester
{
    public class Program
    {
        static void Main(string[] args)
        {
            // [RELEASE] : "args" are RootPath, ZipDate, DelDate INTEGER as if Timespan, 

            // args : rootPath, startZipPeriod(INTEGER), startDelPeriod(INTEGER)
            string rootPath = @"e:\Dummy\";
            List<string> temp = new List<string>();
            string zipFilePath = Path.Combine(rootPath, "LOG.zip");

            DateTime startZip = new DateTime(2021, 5, 1);
            DateTime startDel = new DateTime(2021, 5, 11);

            //DateTime startZip = DateTime.Today.AddDays(-10);
            //DateTime startZip = DateTime.Today.AddDays(-args1);

            //DateTime startDel = startZip.AddDays(-10);
            //DateTime startDel = startZip.AddDays(-args2);

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
                UpdateLogsInExistingZip(startDel, archive);
                UpdateCapturedImageInExistingZip(startDel, archive);

                HandleLogs(rootPath, startZip, startDel, archive);

                HandleCapturedImages(rootPath, "OK", startZip, startDel, temp, archive);
                HandleCapturedImages(rootPath, "NG", startZip, startDel, temp, archive);
            }


            //Scheduler.AddTaskSchedule(@"D:\Github\c_cpp_csharp\FileSystemTester\bin\Release\FileSystemTester.exe", "", ExeInterval.Weekly);
            Console.WriteLine("Management Success.");
            Console.ReadKey();
        }

        public static void UpdateLogsInExistingZip(DateTime del, ZipArchive archive)
        {
            foreach (var file in archive.Entries
                .Where(x => x.FullName.Contains("LOG"))
                .Where(x => isDueDate(del, ParseFilenameToDateTime(x.Name)))
                .ToList())
            {
                archive.GetEntry(file.FullName).Delete();
            }
        }

        public static void UpdateCapturedImageInExistingZip(DateTime del, ZipArchive archive)
        {
            foreach (var file in archive.Entries
                .Where(x => x.FullName.Contains("OK") || x.FullName.Contains("NG"))
                .Where(x => isDueDate(del, ParseArchiveFoldernameToDateTime(x.FullName)))
                .ToList())
            {
                archive.GetEntry(file.FullName).Delete();
            }
        }

        public static void HandleLogs(string rootPath, DateTime zip, DateTime del, ZipArchive archive)
        {
            foreach (var file in new DirectoryInfo(Path.Combine(rootPath, "LOG"))
                .GetFileSystemInfos()
                .Where(f => isDueDate(zip, ParseFilenameToDateTime(f.Name))) // ***************
            //.Where(f => CompareDates(zip, f.CreationTime)).ToList())
                .ToList())  
            {
                if (DeleteFileAfterDelDate(del, file))
                {
                    continue;
                }
                CompressFileIntoZipFile(rootPath, file, archive);
            }
        }

        public static void HandleCapturedImages(string rootPath, string folder, DateTime zip, DateTime del,
            List<string> temp, ZipArchive archive)
        {
            foreach (var dir in new DirectoryInfo(Path.Combine(rootPath, folder))
                .GetFileSystemInfos()
                .Where(f => isDueDate(zip, ParseFoldernameToDateTime(f.Name))) // ***************
            //.Where(f => CompareDates(zip, f.CreationTime)).ToList()) //
                .ToList()) 
            {
                if (DeleteDirectoryAfterDelDate(del, dir))
                {
                    continue;
                }
                CompressDirectoryIntoZipFile(rootPath, dir, temp, archive);
            }
        }

        public static DateTime ParseFilenameToDateTime(string fileName)
        {
            DateTime dtDate;
            string pattern ="yyyy-MM-dd";
            if (!DateTime.TryParseExact(fileName.Substring(0, pattern.Length), pattern, 
                System.Globalization.CultureInfo.InvariantCulture, 
                System.Globalization.DateTimeStyles.None, out dtDate))
            {
                return DateTime.Today;
            }
            return dtDate;
        }

        public static DateTime ParseFoldernameToDateTime(string folderName)
        {
            DateTime dtDate;
            string pattern = "yyyyMMdd";
            if (!DateTime.TryParseExact(folderName, pattern, 
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None, out dtDate))
            {
                return DateTime.Today;
            }
            return dtDate;
        }

        public static DateTime ParseArchiveFoldernameToDateTime(string folderName)
        {
            DateTime dtDate;
            string pattern = "yyyyMMdd";
            if (!DateTime.TryParseExact(folderName.Substring(3, pattern.Length), pattern,
                System.Globalization.CultureInfo.InvariantCulture,
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

                foreach (var file in dirInfo.GetFiles()
                    .Where(f => f.Extension == ".log" || f.Extension == "." + ImageFormat.Png.ToString().ToLower())
                    .Where(f => f.FullName.Contains(DateTime.Today.Date.ToString())))
                {
                    GetFiles(file.FullName, ref fileLists);
                }
            }
            else if ((attr & FileAttributes.Archive) == FileAttributes.Archive)
            {
                var fileInfo = new FileInfo(rootPath);
                if (fileInfo.Extension == ".log" || fileInfo.Extension == "." + ImageFormat.Png.ToString().ToLower())
                    fileLists.Add(fileInfo.FullName);
            }
            return fileLists;
        }

        public static bool isDueDate(DateTime setDate, DateTime fileDate)
        {
            if (DateTime.Compare(setDate.Date, fileDate.Date) >= 0)
            {
                return true;
            }
            return false;
        }

        public static bool DeleteFileAfterDelDate(DateTime deletionDate, FileSystemInfo file)
        {
            if (isDueDate(deletionDate, ParseFilenameToDateTime(file.Name)))  // ***************
            //if (CompareDates(deletionDate, file.CreationTime))
            {
                File.Delete(file.FullName);
                return true;
            }
            return false;
        }

        public static bool DeleteDirectoryAfterDelDate(DateTime deletionDate, FileSystemInfo directory)
        {
            if (isDueDate(deletionDate, ParseFoldernameToDateTime(directory.Name)))  // ***************
            //if (CompareDates(deletionDate, directory.CreationTime))
            {
                Directory.Delete(directory.FullName, true);
                return true;
            }
            return false;
        }
        public static void CompressFileIntoZipFile(string sourcePath, FileSystemInfo file, ZipArchive archive)
        {
            archive.CreateEntryFromFile(file.FullName, file.FullName.Substring(sourcePath.Length), CompressionLevel.Optimal);
            File.Delete(file.FullName);
        }

        public static void CompressDirectoryIntoZipFile(string sourcePath, FileSystemInfo folderName, 
            List<string> compressable, ZipArchive archive)
        {
            compressable = GetFiles(folderName.FullName, ref compressable);
            foreach (var file in compressable)
            {
                archive.CreateEntryFromFile(file, file.Substring(sourcePath.Length), CompressionLevel.Optimal);
            }
            compressable.Clear();
            Directory.Delete(folderName.FullName, true);

        }


        //public static bool ZipDirectoryAfterZipDate(string rootPath, DateTime compressionDate, FileSystemInfo folder, List<string> compressable, ZipArchive archive)
        //{
        //	if (CompareDates(compressionDate, folder.CreationTime))
        //	{
        //		CompressDirectoryIntoZipFile(rootPath, folder, compressable, archive);
        //		return true;
        //	}
        //	return false;
        //}

        //public static bool ZipFileAfterZipDate(string rootPath, DateTime compressionDate, FileSystemInfo file, ZipArchive archive)
        //{
        //	if (CompareDates(compressionDate, file.CreationTime))
        //	{
        //		CompressFileIntoZipFile(rootPath, file, archive);
        //		return true;
        //	}
        //	return false;
        //}
    }
}