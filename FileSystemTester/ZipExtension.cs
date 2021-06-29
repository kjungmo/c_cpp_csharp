using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Compression;

namespace LogManagementSystem
{
    public static class ZipExtension
    {
        public static void FilterExpiredFilesInZip(this ZipArchive archive, DateTime deleteDate)
        {
            ZipHelper.UpdateLogsInExistingZip(archive, deleteDate);
            ZipHelper.UpdateCapturedImageInExistingZip(archive, deleteDate);
            ZipHelper.UpdateCsvInExistingZip(archive, deleteDate);
        }
        
        public static void SortLogs(this ZipArchive archive, string rootPath, 
            DateTime zipDate, DateTime deleteDate)
        {
            foreach (var file in new DirectoryInfo(Path.Combine(rootPath, "LOG"))
                .GetFileSystemInfos()
                .Where(f => ZipHelper.isDueDate(zipDate, ZipHelper.ParseFilenameToDateTime(f.Name)))
                .ToList())
            {
                if (ZipHelper.DeleteFileAfterDelDate(deleteDate, file))
                {
                    continue;
                }
                ZipHelper.CompressFileIntoZipFile(rootPath, file, archive);
            }
        }

        public static void SortCapturedImagesByFolder(this ZipArchive archive, string rootPath, 
            DateTime zipDate, DateTime deleteDate, List<string> temp)
        {
            List<string> capturedLogFolders = new List<string> { "OK", "NG" };
            foreach (var folder in capturedLogFolders)
            {
                foreach (var dir in new DirectoryInfo(Path.Combine(rootPath, folder))
                .GetFileSystemInfos()
                .Where(f => ZipHelper.isDueDate(zipDate, ZipHelper.ParseFoldernameToDateTime(f.Name)))
                .ToList())
                {
                    if (ZipHelper.DeleteFolderAfterDelDate(deleteDate, dir))
                    {
                        continue;
                    }
                    ZipHelper.CompressFolderIntoZipFile(rootPath, dir, temp, archive);
                }
            }
        }

        public static void SortCSVs(this ZipArchive archive, string rootPath, 
            DateTime zipDate, DateTime deleteDate, List<string> temp)
        {
            foreach (var dir in new DirectoryInfo(Path.Combine(rootPath, "VALUES"))
            .GetFileSystemInfos()
            .Where(f => ZipHelper.isDueDate(zipDate, ZipHelper.ParseFoldernameToDateTime(f.Name)))
            .ToList())
            {
                if (ZipHelper.DeleteFolderAfterDelDate(deleteDate, dir))
                {
                    continue;
                }
                ZipHelper.CompressFolderIntoZipFile(rootPath, dir, temp, archive);
            }
        }
    }
}
