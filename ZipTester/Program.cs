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
            Console.WriteLine("[ZIP MODE] [Directory] [Zipfile Name] [Extension Type] [Zip Separation(default ] [Zip Interval] "); //count 6
            Console.WriteLine("[UNZIP MODE] [Zipfile Directory] [Unzip Directory]"); // count 3
            Console.WriteLine("Put your value in \" \" for more than two things as one argument. ");


            // Time-Interval applied ( Daily / Weekly / Monthly / 
            //string path = Directory.GetCurrentDirectory();
            //string folderFullname = new DirectoryInfo(path).FullName;
            //string folderName = new DirectoryInfo(path).Name;
            //DirectoryInfo folder = new DirectoryInfo(path).Parent;
            //Console.WriteLine($"path: {path}");
            //Console.WriteLine($"FolderFullName : {folderFullname}");
            //Console.WriteLine($"FolderName : {folderName}");
            //Console.WriteLine($"path 's parent directory is {folder}");



            #region CommandLineArgs
            string[] arguments = Environment.GetCommandLineArgs();
            arguments = args.Where(condition => condition != args[0]).ToArray();
            List<string> userInput = arguments.ToList();

            //ZipTester.exe zip d:\ZipTest zipper .log 
            string mode = userInput[0];
            string path = userInput[1];
            string zipName = userInput[2];
            string logs = userInput[3];
            //string interval = userInput[4];


            Console.WriteLine($"arguments count : {userInput.Count()}");
            if (userInput[0].ToLower() == "zip")
            {

                Console.WriteLine("Zip Mode Selected!");
                if (userInput.Count() < 4)
                {
                    Console.WriteLine("Arguments Error! Check again.");
                    return;
                }
                CompressZipByIO(path, MakeZipDir(path, zipName), CompressionLevel.Optimal);

            }
            else if (userInput[0].ToLower() == "unzip")
            {
                Console.WriteLine("Unzip Mode Selected!");
                if (userInput.Count() != 3)
                {
                    Console.WriteLine("Arguments Error! Check again.");
                }
            }
            else
            {
                Console.WriteLine("Select an appropriate Mode first.");
            }

            // [TEST] for when default directory is needed
            if (userInput.Count() < 2)
            {
                userInput.Add(Directory.GetCurrentDirectory());
            }
            Console.WriteLine(userInput.Count());


            foreach (string input in userInput)
            {
                Console.WriteLine(input);
            }

            // when user input directory does not exist
            if (!Directory.Exists(userInput[1]))
            {
                Directory.CreateDirectory(userInput[1]);
            }

            //string zipName = 
            //CompressZipByIO(userInput[1], MakeZipDir(userInput[1], zipname));


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
            #endregion
            Console.WriteLine("press any key...");
            Console.ReadKey();
        }

        private static string MakeZipDir(string folderName, string fileName, string timeInterval = "daily")
        {
            string fileType = ".zip";
            string directory = "";
            DateTime dateToday = DateTime.Now;
            string year = "";
            string month = "";
            switch (timeInterval.ToLower())
            {
                case "daily":
                    string today = string.Format("{0:d}", dateToday);
                    string daily = string.Concat(today, "_");
                    directory = folderName + "\\" + daily + fileName + fileType;
                    break;
                case "weekly":
                    break;
                case "monthly":
                    month = dateToday.ToString("yyyy-MM");
                    string monthly = string.Concat(month, "_");
                    directory = folderName + "\\" + monthly + fileName + fileType;
                    break;
                case "quarter":
                    directory = folderName + "\\" + fileName + fileType;
                    break;
                case "half":
                    directory = folderName + "\\" + fileName + fileType;
                    break;
                case "yearly":
                    year = dateToday.Year.ToString();
                    string yearly = string.Concat(year, "_");
                    directory = folderName + "\\" + yearly + fileName + fileType;
                    break;

            }
            

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


        private static void CompressZipByIO(string sourcePath, string zipPath, CompressionLevel compressionLevel)
        {
            var fileList = GetFileList(sourcePath, new List<string>());
            using (FileStream fileStream = new FileStream(zipPath, FileMode.Create, FileAccess.ReadWrite))
            {
                using (ZipArchive zipArchive = new ZipArchive(fileStream, ZipArchiveMode.Create))
                {
                    foreach (string file in fileList)
                    {
                        if(Path.GetExtension(file) == ".log" || Path.GetExtension(file) == ".jpg")
                        {
                            string path = file.Substring(sourcePath.Length + 1);
                            try
                            {
                                zipArchive.CreateEntryFromFile(file, path); // if already exists, throws IOException
                            }
                            catch (Exception e)
                            {

                            }
                        }
                    }
                    DeleteFile(sourcePath);
                }
                Console.WriteLine("Created!");
            }
        }

        private static void DeleteFile(string sourcePath)
        {
            DirectoryInfo dirInfo = new DirectoryInfo(sourcePath);

            //foreach (FileInfo file in dirInfo.GetFiles().Where(f => f.Extension != ".zip"))
            foreach (FileInfo file in dirInfo.GetFiles().Where(f => f.Extension == ".log" || f.Extension == ".jpg"))
            {
                //Console.WriteLine(file.FullName);
                file.Delete();
            }

            Console.WriteLine("Deleted!");
        }
    }
}
