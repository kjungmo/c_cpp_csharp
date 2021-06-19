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
        }
        
        public static void HandleLogs(this ZipArchive archive, string rootPath, DateTime zipDate, DateTime deleteDate)
        {
            foreach (var file in new DirectoryInfo(Path.Combine(rootPath, "LOG"))
                .GetFileSystemInfos()
                .Where(f => ZipHelper.isDueDate(zipDate, ZipHelper.ParseFilenameToDateTime(f.Name))) // ***************
            //.Where(f => ZipHelper.isDueDate(zip, f.CreationTime)).ToList())
                .ToList())
            {
                if (ZipHelper.DeleteFileAfterDelDate(deleteDate, file))
                {
                    continue;
                }
                ZipHelper.CompressFileIntoZipFile(rootPath, file, archive);
            }
        }

        public static void HandleCapturedImagesByFolder(this ZipArchive archive, string rootPath, string folder, DateTime zipDate, DateTime deleteDate,
            List<string> temp)
        {
            foreach (var dir in new DirectoryInfo(Path.Combine(rootPath, folder))
                .GetFileSystemInfos()
                .Where(f => ZipHelper.isDueDate(zipDate, ZipHelper.ParseFoldernameToDateTime(f.Name))) // ***************
            //.Where(f => ZipHelper.isDueDate(zip, f.CreationTime)).ToList()) //
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
