using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace LogManager
{
    public static class ZipHelper
    {
        public static void UpdateZipFileEntries(ZipArchive archive,
            DateTime deleteDateLog, DateTime deleteDateImg, DateTime deleteDateCsv)
        {
            UpdateZipFileLogEntries(archive, deleteDateLog);
            UpdateZipFileImageEntries(archive, deleteDateImg);
            UpdateZipFileCsvEntries(archive, deleteDateCsv);
        }

        private static void UpdateZipFileLogEntries(ZipArchive archive, DateTime deleteDate)
        {
            foreach (var file in archive.Entries
                .Where(x => x.FullName.Contains("LOG"))
                .Where(x => CheckDueDate(deleteDate, ParseFilenameToDateTime(x.Name)))
                .ToList())
            {
                archive.GetEntry(file.FullName).Delete();
            }
        }

        private static bool CheckDueDate(DateTime setDate, DateTime fileDate)
        {
            return DateTime.Compare(setDate.Date, fileDate.Date) >= 0;
        }

        private static DateTime ParseFilenameToDateTime(string fileName, string pattern = "yyyy-MM-dd")
        {
            return ParseFoldernameToDateTime(fileName.Substring(0, pattern.Length), pattern);
        }

        private static void UpdateZipFileImageEntries(ZipArchive archive, DateTime deleteDate)
        {
            foreach (var file in archive.Entries
                .Where(x => x.FullName.Contains("OK") || x.FullName.Contains("NG"))
                .Where(x => CheckDueDate(deleteDate, ParseImgArchiveFoldernameToDateTime(x.FullName)))
                .ToList())
            {
                archive.GetEntry(file.FullName).Delete();
            }
        }
        private static DateTime ParseImgArchiveFoldernameToDateTime(string folderName, string pattern = "yyyyMMdd")
        {
            return ParseFoldernameToDateTime(folderName.Substring(3, pattern.Length));
        }

        private static void UpdateZipFileCsvEntries(ZipArchive archive, DateTime deleteDate)
        {
            foreach (var file in archive.Entries
                .Where(x => x.FullName.Contains("VALUES"))
                .Where(x => CheckDueDate(deleteDate, ParseCsvArchiveFoldernameToDateTime(x.FullName)))
                .ToList())
            {
                archive.GetEntry(file.FullName).Delete();
            }
        }

        private static DateTime ParseCsvArchiveFoldernameToDateTime(string folderName, string pattern = "yyyyMMdd")
        {
            return ParseFoldernameToDateTime(folderName.Substring(7, pattern.Length));
        }

        public static void SortLoggedFiles(string rootPath,
            DateTime zipDateLog, DateTime deleteDateLog,
            DateTime zipDateImg, DateTime deleteDateImg,
            DateTime zipDateCsv, DateTime deleteDateCsv,
            List<string> fileList, ZipArchive archive)
        {
            SortLogFiles(rootPath, zipDateLog, deleteDateLog, archive);
            SortImgFiles(rootPath, zipDateImg, deleteDateImg, fileList, archive);
            SortCsvFiles(rootPath, zipDateCsv, deleteDateCsv, fileList, archive);
        }

        private static void SortLogFiles(string rootPath,
            DateTime zipDate, DateTime deleteDate, ZipArchive archive)
        {
            foreach (var file in new DirectoryInfo(Path.Combine(rootPath, "LOG"))
            .GetFileSystemInfos()
            .Where(f => CheckDueDate(zipDate, ParseFilenameToDateTime(f.Name)))
            .ToList())
            {
                if (DeleteFileAfterDelDate(deleteDate, file))
                {
                    continue;
                }
                if (archive != null)
                {
                    CompressFileIntoZipFile(rootPath, file, archive);
                }
            }
        }

        private static void CompressFileIntoZipFile(string sourcePath, FileSystemInfo file, ZipArchive archive)
        {
            archive.CreateEntryFromFile(file.FullName, file.FullName.Substring(sourcePath.Length), CompressionLevel.Optimal);
            File.Delete(file.FullName);
        }

        private static bool DeleteFileAfterDelDate(DateTime deleteDate, FileSystemInfo file)
        {
            if (CheckDueDate(deleteDate, ParseFilenameToDateTime(file.Name)))
            {
                File.Delete(file.FullName);
                return true;
            }
            return false;
        }

        private static void SortImgFiles(string rootPath,
            DateTime zipDate, DateTime deleteDate, List<string> temp, ZipArchive archive)
        {
            List<string> imgFolders = new List<string> { "OK", "NG" };
            foreach (var folder in imgFolders)
            {
                foreach (var dir in new DirectoryInfo(Path.Combine(rootPath, folder))
                .GetFileSystemInfos()
                .Where(f => CheckDueDate(zipDate, ParseFoldernameToDateTime(f.Name)))
                .ToList())
                {
                    if (DeleteFolderAfterDelDate(deleteDate, dir))
                    {
                        continue;
                    }
                    if (archive != null)
                    {
                        CompressFolderIntoZipFile(rootPath, dir, temp, archive);
                    }
                }
            }
        }

        private static DateTime ParseFoldernameToDateTime(string folderName, string pattern = "yyyyMMdd")
        {
            return DateTime.TryParseExact(
                folderName, pattern,
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None, out DateTime dtDate)
                ? dtDate
                : DateTime.Today;
        }

        private static void CompressFolderIntoZipFile(string sourcePath, FileSystemInfo folderName,
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
            if ((File.GetAttributes(rootPath) & FileAttributes.Directory) == FileAttributes.Directory)
            {
                foreach (var dir in new DirectoryInfo(rootPath).GetDirectories())
                {
                    GetFiles(dir.FullName, ref fileLists);
                }

                foreach (var file in new DirectoryInfo(rootPath).GetFiles().Where(f
                    => f.Extension == ".log"
                    || f.Extension == "." + ImageFormat.Png.ToString().ToLower()
                    || f.Extension == ".csv"))
                {
                    GetFiles(file.FullName, ref fileLists);
                }
            }
            else if ((File.GetAttributes(rootPath) & FileAttributes.Archive) == FileAttributes.Archive)
            {
                AddToFileList(rootPath, ref fileLists);
            }
            return fileLists;
        }

        private static void AddToFileList(string rootPath, ref List<string> fileLists)
        {
            var fileInfo = new FileInfo(rootPath);
            if (fileInfo.Extension == ".log"
                || fileInfo.Extension == "." + ImageFormat.Png.ToString().ToLower()
                || fileInfo.Extension == ".csv")
                fileLists.Add(fileInfo.FullName);
        }

        private static bool DeleteFolderAfterDelDate(DateTime deleteDate, FileSystemInfo directory)
        {
            if (CheckDueDate(deleteDate, ParseFoldernameToDateTime(directory.Name)))
            {
                Directory.Delete(directory.FullName, true);
                return true;
            }
            return false;
        }

        private static void SortCsvFiles(string rootPath,
            DateTime zipDate, DateTime deleteDate, List<string> temp, ZipArchive archive)
        {
            foreach (var dir in new DirectoryInfo(Path.Combine(rootPath, "VALUES"))
            .GetFileSystemInfos()
            .Where(f => CheckDueDate(zipDate, ParseFoldernameToDateTime(f.Name)))
            .ToList())
            {
                if (DeleteFolderAfterDelDate(deleteDate, dir))
                {
                    continue;
                }
                if (archive != null)
                {
                    CompressFolderIntoZipFile(rootPath, dir, temp, archive);
                }
            }
        }
    }
}
