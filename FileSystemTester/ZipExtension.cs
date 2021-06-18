using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Compression;

namespace FileSystemTester
{
    public static class ZipExtension
    {
        public static void FilterExpiredFilesInZip(this ZipArchive archive, DateTime deletionDate)
        {
            ZipHelper.UpdateLogsInExistingZip(archive, deletionDate);
            ZipHelper.UpdateCapturedImageInExistingZip(archive, deletionDate);
        }
        
        public static void HandleLogs(this ZipArchive archive, string rootPath, DateTime zipDate, DateTime deletionDate)
        {
            foreach (var file in new DirectoryInfo(Path.Combine(rootPath, "LOG"))
                .GetFileSystemInfos()
                .Where(f => ZipHelper.isDueDate(zipDate, ZipHelper.ParseFilenameToDateTime(f.Name))) // ***************
            //.Where(f => ZipHelper.isDueDate(zip, f.CreationTime)).ToList())
                .ToList())
            {
                if (ZipHelper.DeleteFileAfterDelDate(deletionDate, file))
                {
                    continue;
                }
                ZipHelper.CompressFileIntoZipFile(rootPath, file, archive);
            }
        }

        public static void HandleCapturedImages(this ZipArchive archive, string rootPath, string folder, DateTime zipDate, DateTime deletionDate,
            List<string> temp)
        {
            foreach (var dir in new DirectoryInfo(Path.Combine(rootPath, folder))
                .GetFileSystemInfos()
                .Where(f => ZipHelper.isDueDate(zipDate, ZipHelper.ParseFoldernameToDateTime(f.Name))) // ***************
            //.Where(f => ZipHelper.isDueDate(zip, f.CreationTime)).ToList()) //
                .ToList())
            {
                if (ZipHelper.DeleteFolderAfterDelDate(deletionDate, dir))
                {
                    continue;
                }
                ZipHelper.CompressFolderIntoZipFile(rootPath, dir, temp, archive);
            }
        }

    }
}
