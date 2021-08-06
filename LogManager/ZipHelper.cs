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
        public string RootPath { get; set; }
        public DateTime ZipLogDaysAfterLogged { get; set; }
        public DateTime DeleteLogDaysAfterLogged { get; set; }
        public DateTime ZipImgDaysAfterLogged { get; set; }
        public DateTime DeleteImgDaysAfterLogged { get; set; }
        public DateTime ZipCsvDaysAfterLogged { get; set; }
        public DateTime DeleteCsvDaysAfterLogged { get; set; }
        public ZipHelper(string[] cLArguments)
        {
            RootPath = cLArguments[1];
            ZipLogDaysAfterLogged = DateTime.Today.AddDays(-Convert.ToInt32(cLArguments[2]));
            DeleteLogDaysAfterLogged = ZipLogDaysAfterLogged.AddDays(-Convert.ToInt32(cLArguments[3]));
            ZipImgDaysAfterLogged = DateTime.Today.AddDays(-Convert.ToInt32(cLArguments[4]));
            DeleteImgDaysAfterLogged = ZipImgDaysAfterLogged.AddDays(-Convert.ToInt32(cLArguments[5]));
            ZipCsvDaysAfterLogged = DateTime.Today.AddDays(-Convert.ToInt32(cLArguments[6]));
            DeleteCsvDaysAfterLogged = ZipCsvDaysAfterLogged.AddDays(-Convert.ToInt32(cLArguments[7]));
        }
        public void UpdateZipFileEntries(ZipArchive archive)
        {
            UpdateZipFileLogEntries(archive);
            UpdateZipFileImageEntries(archive);
            UpdateZipFileCsvEntries(archive);
        }

        private void UpdateZipFileLogEntries(ZipArchive archive)
        {
            foreach (var file in archive.Entries
                .Where(x => x.FullName.Contains("LOG"))
                .Where(x => CheckDueDate(DeleteLogDaysAfterLogged, ParseFilenameToDateTime(x.Name)))
                .ToList())
            {
                archive.GetEntry(file.FullName).Delete();
            }
        }

        private bool CheckDueDate(DateTime setDate, DateTime fileDate)
        {
            return DateTime.Compare(setDate.Date, fileDate.Date) >= 0;
        }

        private DateTime ParseFilenameToDateTime(string fileName, string pattern = "yyyy-MM-dd")
        {
            return ParseFoldernameToDateTime(fileName.Substring(0, pattern.Length), pattern);
        }

        private void UpdateZipFileImageEntries(ZipArchive archive)
        {
            foreach (var file in archive.Entries
                .Where(x => x.FullName.Contains("OK") || x.FullName.Contains("NG"))
                .Where(x => CheckDueDate(DeleteImgDaysAfterLogged, ParseImgArchiveFoldernameToDateTime(x.FullName)))
                .ToList())
            {
                archive.GetEntry(file.FullName).Delete();
            }
        }
        private DateTime ParseImgArchiveFoldernameToDateTime(string folderName, string pattern = "yyyyMMdd")
        {
            return ParseFoldernameToDateTime(folderName.Substring(3, pattern.Length));
        }

        private void UpdateZipFileCsvEntries(ZipArchive archive)
        {
            foreach (var file in archive.Entries
                .Where(x => x.FullName.Contains("VALUES"))
                .Where(x => CheckDueDate(DeleteCsvDaysAfterLogged, ParseCsvArchiveFoldernameToDateTime(x.FullName)))
                .ToList())
            {
                archive.GetEntry(file.FullName).Delete();
            }
        }

        private DateTime ParseCsvArchiveFoldernameToDateTime(string folderName, string pattern = "yyyyMMdd")
        {
            return ParseFoldernameToDateTime(folderName.Substring(7, pattern.Length));
        }

        public void SortLoggedFiles(List<string> fileList = null, ZipArchive archive = null)
        {
            SortLogFiles(archive);
            SortImgFiles(fileList, archive);
            SortCsvFiles(fileList, archive);
        }

        private void SortLogFiles(ZipArchive archive)
        {
            foreach (var file in new DirectoryInfo(Path.Combine(RootPath, "LOG"))
            .GetFileSystemInfos()
            .Where(f => CheckDueDate(ZipLogDaysAfterLogged, ParseFilenameToDateTime(f.Name)))
            .ToList())
            {
                if (DeleteFileAfterDelDate(DeleteLogDaysAfterLogged, file))
                {
                    continue;
                }
                if (archive != null)
                {
                    CompressFileIntoZipFile(RootPath, file, archive);
                }
            }
        }

        private void CompressFileIntoZipFile(string sourcePath, FileSystemInfo file, ZipArchive archive)
        {
            archive.CreateEntryFromFile(file.FullName, file.FullName.Substring(sourcePath.Length), CompressionLevel.Optimal);
            File.Delete(file.FullName);
        }

        private bool DeleteFileAfterDelDate(DateTime deleteDate, FileSystemInfo file)
        {
            if (CheckDueDate(deleteDate, ParseFilenameToDateTime(file.Name)))
            {
                File.Delete(file.FullName);
                return true;
            }
            return false;
        }

        private void SortImgFiles(List<string> temp, ZipArchive archive)
        {
            List<string> imgFolders = new List<string> { "OK", "NG" };
            foreach (var folder in imgFolders)
            {
                foreach (var dir in new DirectoryInfo(Path.Combine(RootPath, folder))
                .GetFileSystemInfos()
                .Where(f => CheckDueDate(ZipImgDaysAfterLogged, ParseFoldernameToDateTime(f.Name)))
                .ToList())
                {
                    if (DeleteFolderAfterDelDate(DeleteImgDaysAfterLogged, dir))
                    {
                        continue;
                    }
                    if (archive != null)
                    {
                        CompressFolderIntoZipFile(RootPath, dir, temp, archive);
                    }
                }
            }
        }

        private DateTime ParseFoldernameToDateTime(string folderName, string pattern = "yyyyMMdd")
        {
            return DateTime.TryParseExact(
                folderName, pattern,
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None, out DateTime dtDate)
                ? dtDate
                : DateTime.Today;
        }

        private void CompressFolderIntoZipFile(string sourcePath, FileSystemInfo folderName,
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

        private List<string> GetFiles(string rootPath, ref List<string> fileLists)
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

        private void AddToFileList(string rootPath, ref List<string> fileLists)
        {
            var fileInfo = new FileInfo(rootPath);
            if (fileInfo.Extension == ".log"
                || fileInfo.Extension == "." + ImageFormat.Png.ToString().ToLower()
                || fileInfo.Extension == ".csv")
                fileLists.Add(fileInfo.FullName);
        }

        private bool DeleteFolderAfterDelDate(DateTime deleteDate, FileSystemInfo directory)
        {
            if (CheckDueDate(deleteDate, ParseFoldernameToDateTime(directory.Name)))
            {
                Directory.Delete(directory.FullName, true);
                return true;
            }
            return false;
        }

        private void SortCsvFiles(List<string> temp, ZipArchive archive)
        {
            foreach (var dir in new DirectoryInfo(Path.Combine(RootPath, "VALUES"))
            .GetFileSystemInfos()
            .Where(f => CheckDueDate(ZipCsvDaysAfterLogged, ParseFoldernameToDateTime(f.Name)))
            .ToList())
            {
                if (DeleteFolderAfterDelDate(DeleteCsvDaysAfterLogged, dir))
                {
                    continue;
                }
                if (archive != null)
                {
                    CompressFolderIntoZipFile(RootPath, dir, temp, archive);
                }
            }
        }
    }
}
