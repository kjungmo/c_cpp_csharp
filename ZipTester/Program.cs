﻿using System;
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
            Console.Title = "CogAplex Log File Management System";
            Console.WriteLine("CogAplex Log File Management System\r");
            Console.WriteLine("-----------------------------------");
            Console.WriteLine("<<<  Modes  >>>   ZIP   ||  UNZIP ");
            Console.WriteLine("[ZIP MODE] [Directory] [Zipfile Name] [Zip Interval] "); //count 4  -> [ZIP MODE] [Log Directory] [Img Directory] [Zip Directory] [Zipfile Name] [Zip Interval] 
            Console.WriteLine("[UNZIP MODE] [Zipfile Directory] "); // count 2
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
            // when deploying, must use 

            //string[] cmdArgs = Environment.GetCommandLineArgs();
            //List<string> userInput = cmdArgs.Where(arg => arg != cmdArgs[0]).ToList();

            
            List<string> userInput = args.Where(condition => condition != args[0]).ToList();
            foreach (var item in userInput)
            {
                Console.Write(item + "\t");
            }
            Console.WriteLine();

            //ZipTester.exe zip d:\ZipTest zipp monthly

            //ZipTester.exe unzip ‪D:\ZipTest\2021-04_zipp.zip

            Console.WriteLine($"arguments count : {userInput.Count()}");
            
            switch (userInput[0].ToLower())
            {
                case "zip":
                    Console.WriteLine("Zip Mode Selected!");
                    if (userInput.Count() < 4)
                    {
                        Console.WriteLine("Arguments Error! Check again.");
                        return;
                    }
                    CompressZIPFile(userInput[1], MakeZipDir(userInput[1], userInput[2], userInput[3]), CompressionLevel.Optimal);
                    AddTaskSchedule(userInput[3]);
                    break;

                case "unzip":
                    Console.WriteLine("Unzip Mode Selected!");
                    if (userInput.Count() != 2)
                    {
                        Console.WriteLine("Arguments Error! Check again.");
                    }
                    ExtractZIPFile(userInput[1]);
                    break;

                default:
                    Console.WriteLine("Select an appropriate Mode first.");
                    break;
            }

            foreach (string input in userInput)
            {
                Console.WriteLine(input);
            }


            #endregion
            Console.WriteLine("press any key...");
            Console.ReadKey();
        }

        #region Makeing Zip File directory
        private static string MakeZipDir(string folderName, string fileName, string timeInterval)
        {
            string fileType = ".zip";
            string directory = "";
            DateTime dateToday = DateTime.Today;
            string today = "";
            string year = "";
            string month = "";
            switch (timeInterval.ToLower())
            {
                case "daily":
                    today = string.Format("{0:d}", dateToday);   // {0:d} -> yyyy-MM-dd format
                    string daily = string.Concat(today, "_");
                    directory = folderName + "\\" + daily + fileName + fileType;
                    break;
                case "weekly":
                    today = string.Format("{0:d}", dateToday);
                    string lastWeek = string.Format("{0:d}", dateToday.AddDays(-7));
                    string weekly = string.Concat(lastWeek, "_", today, "_");
                    directory = folderName + "\\" + weekly +  fileName + fileType;
                    break;
                case "monthly":
                    month = dateToday.ToString("yyyy-MM");
                    string monthly = string.Concat(month, "_");
                    directory = folderName + "\\" + monthly + fileName + fileType;
                    break;
                //case "quarter":
                //    directory = folderName + "\\" + fileName + fileType;
                //    break;
                //case "half":
                //    directory = folderName + "\\" + fileName + fileType;
                //   break;
                //case "yearly":
                //   year = dateToday.Year.ToString();
                //    string yearly = string.Concat(year, "_");
                //    directory = folderName + "\\" + yearly + fileName + fileType;
                //   break;
            }
            return directory;
        }
        #endregion

        #region getting File Lists
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
        #endregion

        #region Compressing Files into Zipfile
        private static void CompressZIPFile(string sourcePath, string zipPath, CompressionLevel compressionLevel)
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
                DeleteZIPFile(sourcePath);
            }
        }
        #endregion

        #region Deleting Files after the files has been Compressed
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
        #endregion

        #region Deleting Zipfiles after certain amount of time
        private static void DeleteZIPFile(string sourcePath, int deleteInterval = -5)
        {
            DirectoryInfo dirInfo = new DirectoryInfo(sourcePath);
            if(dirInfo.Exists)
            {
                FileInfo[] files = dirInfo.GetFiles();
                foreach (FileInfo file in files)
                {
                    if (DateTime.Today.AddDays(deleteInterval).ToString("yyyy-MM-dd").CompareTo(file.LastWriteTime.ToString("yyyy-MM-dd")) == 0)
                    {
                        if (System.Text.RegularExpressions.Regex.IsMatch(file.Name, ".zip"))
                        {
                            Console.WriteLine(dirInfo + "\\" + file.Name);
                            File.Delete(dirInfo + "\\" + file.Name);
                            Console.WriteLine("Deleted");
                        }
                    }
                }
                
            }

            //string zipFile = File.GetLastWriteTime(sourcePath).ToString("yyyy-MM-dd");
            //string date = "";
            //switch (interval)
            //{
            //    case "daily":
            //    case "weekly":
            //        date = DateTime.Today.AddMonths(-6).ToString("yyyy-MM-dd");
            //        if (date.CompareTo(zipFile) == 0 )
            //        {

            //        }
            //        break;
                    
            //    case "monthly":
            //        break;
            //    default:
            //        break;
            

        }
        #endregion

        #region Extracting Zipfiles to its own directory        
        private static void ExtractZIPFile(string zipFilePath) 
        {
            string extractPath = Path.Combine(Path.GetDirectoryName(zipFilePath), Path.GetFileNameWithoutExtension(zipFilePath));
            try
            {
                ZipFile.ExtractToDirectory(zipFilePath, extractPath);
            }
            catch(Exception e)
            {

            }

            //string destinationFolder = Path.Combine(Path.GetDirectoryName(zipFilePath), Path.GetFileNameWithoutExtension(zipFilePath));
            //if (!Directory.Exists(destinationFolder))
            //{
            //    Directory.CreateDirectory(destinationFolder);
            //}

            //using (ZipArchive zipArchive = ZipFile.OpenRead(zipFilePath))
            //{
            //    foreach (ZipArchiveEntry zipArchiveEntry in zipArchive.Entries)
            //    {
            //        try
            //        {
            //            var filePath = Path.Combine(destinationFolder, zipArchiveEntry.FullName);
            //            var subDir = Path.GetDirectoryName(filePath);
            //            if (!Directory.Exists(subDir))
            //            {
            //                Directory.CreateDirectory(subDir);
            //            }
            //            try
            //            {
            //                zipArchiveEntry.ExtractToFile(filePath);
            //            }
            //            catch (Exception e)
            //            {

            //            }
            //        }
            //        catch (PathTooLongException)
            //        {

            //        }
            //    }
            //}
        }
        #endregion

        #region Task Scheduler
        private static void AddTaskSchedule(string timeInterval)
        {
            // Create a new task definition for the local machine and assign properties
            TaskDefinition td = TaskService.Instance.NewTask();
            td.RegistrationInfo.Description = "Compressing Log files.";
            switch (timeInterval)
            {
                case "daily":
                    // Create a trigger that runs every other day and will start randomly between 10 a.m. and 12 p.m.
                    DailyTrigger dt = new DailyTrigger();
                    dt.StartBoundary = DateTime.Today + TimeSpan.FromHours(10);
                    dt.DaysInterval = 2;
                    dt.RandomDelay = TimeSpan.FromHours(2);
                    break;
                case "weekly":
                    // Create a trigger that runs on Monday every third week just after midnight.
                    WeeklyTrigger wTrigger = new WeeklyTrigger();
                    wTrigger.StartBoundary = DateTime.Today + TimeSpan.FromSeconds(15);
                    wTrigger.DaysOfWeek = DaysOfTheWeek.Monday;
                    wTrigger.WeeksInterval = 3;
                    break;
                case "monthly":
                    // Create a trigger that will run at 10 a.m. on the 10th, 20th and last days of July and November
                    MonthlyTrigger mTrigger = new MonthlyTrigger();
                    mTrigger.StartBoundary = DateTime.Today + TimeSpan.FromHours(10);
                    mTrigger.DaysOfMonth = new int[]{ 10, 20 };
                    mTrigger.MonthsOfYear = MonthsOfTheYear.July | MonthsOfTheYear.November;
                    mTrigger.RunOnLastDayOfMonth = true; // V2 only

                    // Create a trigger that runs Saturday in the first and last week of January and Decemeber at 1 a.m.
                    MonthlyDOWTrigger mdTrigger = new MonthlyDOWTrigger();
                    mdTrigger.StartBoundary = DateTime.Today + TimeSpan.FromHours(1);
                    mdTrigger.DaysOfWeek = DaysOfTheWeek.Saturday;
                    mdTrigger.MonthsOfYear = MonthsOfTheYear.January | MonthsOfTheYear.December;
                    mdTrigger.WeeksOfMonth = WhichWeek.FirstWeek | WhichWeek.LastWeek;
                    break;
            }



            // Add a trigger that, starting tomorrow, will fire every other week on Monday
            // and Saturday and repeat every 10 minutes for the following 11 hours
            //WeeklyTrigger wt = new WeeklyTrigger();
            //wt.StartBoundary = DateTime.Now.AddSeconds(5);
            //wt.DaysOfWeek = DaysOfTheWeek.AllDays;
            //wt.WeeksInterval = 3;
            //wt.Repetition.Duration = TimeSpan.FromHours(.5);
            //wt.Repetition.Interval = TimeSpan.FromSeconds(20);
            //td.Triggers.Add(wt);

            
            ExecAction CogAplex = new ExecAction();
            CogAplex.Path = "zip";
            CogAplex.Arguments = "d";
            CogAplex.WorkingDirectory = "";

            td.Actions.Add("notepad.exe", "d:\\test.log");

            // Register the task in the root folder of the local machine
            TaskService.Instance.RootFolder.RegisterTaskDefinition("Test", td);
        }
        #endregion
    }
}
