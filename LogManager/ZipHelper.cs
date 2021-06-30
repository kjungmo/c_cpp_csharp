using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace LogManager
{
    public class ZipHelper
    {
        public static void UpdateLogsInExistingZip(ZipArchive archive, DateTime deleteDate)
        {
            foreach (var file in archive.Entries
                .Where(x => x.FullName.Contains("LOG"))
                .Where(x => isDueDate(deleteDate, ParseFilenameToDateTime(x.Name)))
                .ToList())
            {
                archive.GetEntry(file.FullName).Delete();
            }
        }

        public static void UpdateCapturedImageInExistingZip(ZipArchive archive, DateTime deleteDate)
        {
            foreach (var file in archive.Entries
                .Where(x => x.FullName.Contains("OK") || x.FullName.Contains("NG"))
                .Where(x => isDueDate(deleteDate, ParseImgArchiveFoldernameToDateTime(x.FullName)))
                .ToList())
            {
                archive.GetEntry(file.FullName).Delete();
            }
        }

        public static void UpdateCsvInExistingZip(ZipArchive archive, DateTime deleteDate)
        {
            foreach (var file in archive.Entries
                .Where(x => x.FullName.Contains("VALUES"))
                .Where(x => isDueDate(deleteDate, ParseCsvArchiveFoldernameToDateTime(x.FullName)))
                .ToList())
            {
                archive.GetEntry(file.FullName).Delete();
            }
        }

        public static bool isDueDate(DateTime setDate, DateTime fileDate)
        {
            if (DateTime.Compare(setDate.Date, fileDate.Date) >= 0)
            {
                return true;
            }
            return false;
        }

        public static bool DeleteFileAfterDelDate(DateTime deleteDate, FileSystemInfo file)
        {
            if (isDueDate(deleteDate, ParseFilenameToDateTime(file.Name)))
            {
                File.Delete(file.FullName);
                return true;
            }
            return false;
        }

        public static bool DeleteFolderAfterDelDate(DateTime deleteDate, FileSystemInfo directory)
        {
            if (isDueDate(deleteDate, ParseFoldernameToDateTime(directory.Name)))
            {
                Directory.Delete(directory.FullName, true);
                return true;
            }
            return false;
        }

        public static DateTime ParseFilenameToDateTime(string fileName, string pattern = "yyyy-MM-dd")
        {
            DateTime dtDate;
            if (!DateTime.TryParseExact(fileName.Substring(0, pattern.Length), pattern,
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None, out dtDate))
            {
                return DateTime.Today;
            }
            return dtDate;
        }

        public static DateTime ParseFoldernameToDateTime(string folderName, string pattern = "yyyyMMdd")
        {
            DateTime dtDate;
            if (!DateTime.TryParseExact(folderName, pattern,
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None, out dtDate))
            {
                return DateTime.Today;
            }
            return dtDate;
        }

        public static DateTime ParseImgArchiveFoldernameToDateTime(string folderName, string pattern = "yyyyMMdd")
        {
            return ParseFoldernameToDateTime(folderName.Substring(3, pattern.Length));
        }
        public static DateTime ParseCsvArchiveFoldernameToDateTime(string folderName, string pattern = "yyyyMMdd")
        {
            return ParseFoldernameToDateTime(folderName.Substring(7, pattern.Length));
        }

        public static void CompressFolderIntoZipFile(string sourcePath, FileSystemInfo folderName,
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

        private static List<string> GetFiles(string rootPath, ref List<string> fileLists)
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
                    .Where(f => f.Extension == ".log"
                    || f.Extension == "." + ImageFormat.Png.ToString().ToLower()
                    || f.Extension == ".csv"))
                {
                    GetFiles(file.FullName, ref fileLists);
                }
            }
            else if ((attr & FileAttributes.Archive) == FileAttributes.Archive)
            {
                var fileInfo = new FileInfo(rootPath);
                if (fileInfo.Extension == ".log"
                    || fileInfo.Extension == "." + ImageFormat.Png.ToString().ToLower()
                    || fileInfo.Extension == ".csv")
                    fileLists.Add(fileInfo.FullName);
            }
            return fileLists;
        }

        public static void CompressFileIntoZipFile(string sourcePath, FileSystemInfo file, ZipArchive archive)
        {
            archive.CreateEntryFromFile(file.FullName, file.FullName.Substring(sourcePath.Length), CompressionLevel.Optimal);
            File.Delete(file.FullName);
        }

        public static void SortLogs(string rootPath, DateTime deleteDate)
        {
            foreach (var file in new DirectoryInfo(Path.Combine(rootPath, "LOG"))
                .GetFileSystemInfos()
                .Where(f => isDueDate(deleteDate, ParseFilenameToDateTime(f.Name)))
                .ToList())
            {
                if (DeleteFileAfterDelDate(deleteDate, file))
                {
                    continue;
                }
            }
        }

        public static void SortImgs(string rootPath, DateTime deleteDate)
        {
            List<string> capturedLogFolders = new List<string> { "OK", "NG" };
            foreach (var folder in capturedLogFolders)
            {
                foreach (var dir in new DirectoryInfo(Path.Combine(rootPath, folder))
                .GetFileSystemInfos()
                .Where(f => isDueDate(deleteDate, ParseFoldernameToDateTime(f.Name)))
                .ToList())
                {
                    if (DeleteFolderAfterDelDate(deleteDate, dir))
                    {
                        continue;
                    }
                }
            }
        }

        public static void SortCSVs(string rootPath, DateTime deleteDate)
        {
            foreach (var dir in new DirectoryInfo(Path.Combine(rootPath, "VALUES"))
            .GetFileSystemInfos()
            .Where(f => isDueDate(deleteDate, ParseFoldernameToDateTime(f.Name)))
            .ToList())
            {
                if (DeleteFolderAfterDelDate(deleteDate, dir))
                {
                    continue;
                }
            }
        }
    }
}
