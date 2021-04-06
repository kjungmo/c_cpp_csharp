using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Compression;
using System.IO;
using Microsoft.Win32.TaskScheduler;

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
            Console.WriteLine($"Task Scheduler Library version : {TaskService.LibraryVersion}");

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
            // when deploying, must use Environment.GetCommandLineArgs()
            //string[] cmdArgs = Environment.GetCommandLineArgs();
            //List<string> myArgs = cmdArgs.Where(arg => arg != cmdArgs[0]).ToList();

            
            List<string> userInput = args.Where(condition => condition != args[0]).ToList();
            foreach (var item in userInput)
            {
                Console.Write(item + "\t");
            }
            Console.WriteLine();

            //ZipTester.exe zip d:\ZipTest zipp .log monthly
            string mode = userInput[0];
            string path = userInput[1];
            //string zipName = userInput[2];
            //string logs = userInput[3];
            //string interval = userInput[4];
            //ZipTester.exe unzip ‪D:\ZipTest\2021-04_zipp.zip

            Console.WriteLine($"arguments count : {userInput.Count()}");

            if (mode.ToLower() == "zip")
            {

                Console.WriteLine("Zip Mode Selected!");
                if (userInput.Count() < 4)
                {
                    Console.WriteLine("Arguments Error! Check again.");
                    return;
                }
                //CompressZipByIO(path, MakeZipDir(path, zipName, interval), CompressionLevel.Optimal);

            }
            else if (mode.ToLower() == "unzip")
            {
                Console.WriteLine("Unzip Mode Selected!");
                if (userInput.Count() != 2)
                {
                    Console.WriteLine("Arguments Error! Check again.");
                }
                ExtractZIPFile(path);
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

            foreach (string input in userInput)
            {
                Console.WriteLine(input);
            }

            // when user input directory does not exist
            //if (!Directory.Exists(userInput[1]))
            //{
            //    Directory.CreateDirectory(userInput[1]);
            //}

            #endregion
            Console.WriteLine("press any key...");
            Console.ReadKey();
        }

        private static string MakeZipDir(string folderName, string fileName, string timeInterval)
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
                                zipArchive.CreateEntryFromFile(file, path, compressionLevel); // if already exists, throws IOException
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
            foreach (FileInfo file in dirInfo.GetFiles().Where(f => f.Extension == ".log" || f.Extension == ".jpg"))
            {
                Console.WriteLine(file.FullName);
                file.Delete();
            }

            Console.WriteLine("Deleted!");
        }

        #region Extraction
        //ExtractZIPFile(backupFolderPath, zipFilePath) 
        
        private static void ExtractZIPFile(string zipFilePath) 
        {
            string destinationFolder = Path.Combine(Path.GetDirectoryName(zipFilePath), Path.GetFileNameWithoutExtension(zipFilePath));
            if (!Directory.Exists(destinationFolder))
            {
                Directory.CreateDirectory(destinationFolder);
            }
            
            using(ZipArchive zipArchive = ZipFile.OpenRead(zipFilePath)) 
            {
                foreach(ZipArchiveEntry zipArchiveEntry in zipArchive.Entries) 
                {
                    try
                    {
                        var filePath = Path.Combine(destinationFolder, zipArchiveEntry.FullName);
                        var subDir = Path.GetDirectoryName(filePath);
                        if (!Directory.Exists(subDir))
                        {
                            Directory.CreateDirectory(subDir);
                        }
                        zipArchiveEntry.ExtractToFile(filePath);
                    }
                    catch(PathTooLongException)
                    { }
                } 
            } 
        }
        #endregion

        //#region Task Scheduler
        //public void TaskSchedule(string description, string startTime, string duration, string timeInterval)
        //{

        //    int start = Convert.ToInt32(startTime);
        //    // Create a new task definition for the local machine and assign properties
        //    TaskDefinition td = TaskService.Instance.NewTask();
        //    td.RegistrationInfo.Description = "Does something";
        //    TimeSpan timeSpan;
        //    switch (timeInterval)
        //    {
        //        case "daily":
        //            DailyTrigger dayy;
        //            break;
        //        case "weekly":
        //            WeeklyTrigger weekk;
        //            break;
        //        case "monthly":
        //            MonthlyTrigger monthh;
        //            break;
        //    }

            
            
        //    // Add a trigger that, starting tomorrow, will fire every other week on Monday
        //    // and Saturday and repeat every 10 minutes for the following 11 hours
        //    WeeklyTrigger wt = new WeeklyTrigger();
        //    wt.StartBoundary = DateTime.Now.AddSeconds(5);
        //    wt.DaysOfWeek = DaysOfTheWeek.AllDays;
        //    wt.WeeksInterval = 3;
        //    wt.Repetition.Duration = TimeSpan.FromHours(.5);
        //    wt.Repetition.Interval = TimeSpan.FromSeconds(20);
        //    td.Triggers.Add(wt);

        //    // Create an action that will launch Notepad whenever the trigger fires
        //    ExecAction CogAplex = new ExecAction();
        //    CogAplex.Path = "zip";
        //    CogAplex.Arguments = "d";
        //    CogAplex.WorkingDirectory = "";

        //    td.Actions.Add("notepad.exe", "d:\\test.log");

        //    // Register the task in the root folder of the local machine
        //    TaskService.Instance.RootFolder.RegisterTaskDefinition("Test", td);
        //}
        //#endregion
    }
}
