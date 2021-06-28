using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing.Imaging;
using System.IO.Compression;

namespace LogManagementSystem
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
                .Where(x => isDueDate(deleteDate, ParseArchiveFoldernameToDateTime(x.FullName)))
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

        public static DateTime ParseFilenameToDateTime(string fileName)
        {
            DateTime dtDate;
            string pattern = "yyyy-MM-dd";
            if (!DateTime.TryParseExact(fileName.Substring(0, pattern.Length), pattern,
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None, out dtDate))
            {
                return DateTime.Today;
            }
            return dtDate;
        }

        public static DateTime ParseFoldernameToDateTime(string folderName) //**************************
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
            //DeleteDirectory(folderName.FullName, true);
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
                    .Where(f => f.Extension == ".log" || f.Extension == "." + ImageFormat.Png.ToString().ToLower()))
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

        public static bool DeleteFileAfterDelDate(DateTime deleteDate, FileSystemInfo file)
        {
            if (isDueDate(deleteDate, ParseFilenameToDateTime(file.Name)))  // ***************
            //if (isDueDate(deleteDate, file.CreationTime))
            {
                File.Delete(file.FullName);
                return true;
            }
            return false;
        }

        public static bool DeleteFolderAfterDelDate(DateTime deleteDate, FileSystemInfo directory)
        {
            if (isDueDate(deleteDate, ParseFoldernameToDateTime(directory.Name)))  // ***************
            //if (isDueDate(deleteDate, directory.CreationTime))
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

        public static void DeleteDirectory(string directoryName, bool checkDirectoryExistance)
        {
            if (Directory.Exists(directoryName))
            {
                Directory.Delete(directoryName, true);
            }
            else if (checkDirectoryExistance)
            {
                throw new SystemException("No such Directory to Delete.");
            }
        }
    }
}
