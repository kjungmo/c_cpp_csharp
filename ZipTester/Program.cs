using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Compression;
using System.IO;

namespace ZipTester
{
    class Program
    {

        static void Main(string[] args)
        {

            
            Console.WriteLine("CogAplex Log File Management System\r");
            Console.WriteLine("-----------------------------------");
            Console.WriteLine("<<<  Modes  >>>   ZIP   ||  UNZIP ");
            Console.WriteLine("[ZIP MODE] [Directory] [File Extension Type] [Interval] ");
            Console.WriteLine("[UNZIP MODE] [Directory]");

            string[] arguments = Environment.GetCommandLineArgs();
            arguments = arguments.Where(condition => condition != arguments[0]).ToArray();
            foreach (string arg in arguments)
            {
                Console.WriteLine(arg);
            }

            //ZipTester.exe zip f:\Github\ .log/.jpg daily

            switch (arguments[0].ToLower())
            {
                case "zip":
                    Console.WriteLine("Zip Mode Selected!");
                    break;
                case "unzip":
                    Console.WriteLine("Unzip Mode Selected!");
                    break;
                default:
                    break;
            }
            //Console.Write("Log Directory : ");
            //string logDir = Console.ReadLine();
            //Console.Write("Zipfile Name : ");
            //string zipName = Console.ReadLine();
            //CompressZipByIO(logDir, MakeZipDir(logDir, zipName));
            //Console.Write("Delete Compressed Files? (Y/N) : ");

            //switch(Console.ReadLine())
            //{
            //    case "y":
            //    case "Y":
            //        DeleteFile(logDir);
            //        break;
            //    case "n":
            //    case "N":
            //        Console.WriteLine("Files Left.");
            //        break;


            //}

            Console.WriteLine("press any key...");
            Console.ReadKey();
        }

        private static string MakeZipDir(string folderName, string fileName)
        {
            string fileType = ".zip";
            string directory = folderName + "\\" + fileName + fileType;
            return directory;
        }

        private static List<string> GetFileList(String rootPath, List<String> fileList)
        {
            if (fileList == null)
            {
                return null;
            }

            var attr = File.GetAttributes(rootPath);
            if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
            {
                var dirInfo = new DirectoryInfo(rootPath);
                foreach (var dir in dirInfo.GetDirectories())
                {
                    GetFileList(dir.FullName, fileList);
                }

                foreach (var file in dirInfo.GetFiles())
                {
                    GetFileList(file.FullName, fileList);
                }
            }
            else
            {
                var fileInfo = new FileInfo(rootPath);
                fileList.Add(fileInfo.FullName);
            }
            return fileList;
        }


        private static void CompressZipByIO(string sourcePath, string zipPath)
        {
            var fileList = GetFileList(sourcePath, new List<string>());
            using (FileStream fileStream = new FileStream(zipPath, FileMode.Create, FileAccess.ReadWrite))
            {
                using (ZipArchive zipArchive = new ZipArchive(fileStream, ZipArchiveMode.Create))
                {
                    foreach (string file in fileList)
                    {
                        string path = file.Substring(sourcePath.Length + 1);
                        try
                        {
                            zipArchive.CreateEntryFromFile(file, path); // if already exists, thros IOException
                        }
                        catch (Exception e)
                        {

                        }
                    }
                }
                Console.WriteLine("Created!");
            }
        }

        private static void DeleteFile(string sourcePath)
        {
            DirectoryInfo dirInfo = new DirectoryInfo(sourcePath);

            foreach (FileInfo file in dirInfo.GetFiles().Where(f => f.Extension != ".zip"))
            {
                Console.WriteLine(file.FullName);
                file.Delete();
            }

            Console.WriteLine("Deleted!");
        }
    }
}
