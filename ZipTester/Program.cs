using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Compression;
using System.IO;
using Microsoft.Win32.TaskScheduler;
using System.Windows.Forms;

namespace ZipTester
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Form form = new Form();

            Console.Title = "CogAplex Log File Management System";
            Console.WriteLine("CogAplex Log File Management System\r");
            Console.WriteLine("-----------------------------------");
            Console.WriteLine("<<<  Modes  >>>   ZIP   ||  UNZIP ");
            Console.WriteLine("[ZIP MODE] [DIR to Save Zipfile] [Log DIR] [Img DIR] [Zipfile Name] [Zip Interval] [Task Scheduler Interval] [Task Scheduler StopFlag]"); //count 5  -> [ZIP MODE] [Log Directory] [Img Directory] [Zip Directory] [Zipfile Name] [Zip Interval] 
            Console.WriteLine("[UNZIP MODE] [Zipfiles(plural) Directory] [Zip Interval] [Date]"); // count 4
            Console.WriteLine("Put your value in \" \" for more than two things as one argument. ");
            Console.WriteLine($"Task Scheduler Library version : {TaskService.LibraryVersion}");

            #region CommandLineArgs
            // when deploying, must use 

            //string[] cmdArgs = Environment.GetCommandLineArgs();
            //List<string> userInput = cmdArgs.Where(arg => arg != cmdArgs[0]).ToList();

            #region deleteAfterDevel.
            List<string> userInput = args.Where(condition => condition != args[0]).ToList();
            foreach (var item in userInput)
            {
                Console.Write(item + "\t");
            }
            Console.WriteLine();
            Console.WriteLine($"arguments count : {userInput.Count()}");
            #endregion
            //ZipTester.exe zip d:\ZipTest zipp monthly
            //ZipTester.exe unzip ‪D:\ZipTest\2021-04_zipp.zip
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
                    DeleteZIPFile(userInput[1], userInput[3]);
                    AddTaskSchedule(userInput[3]);
                    break;

                case "unzip":
                    Application.Run(form);
                    Console.WriteLine("Unzip Mode Selected!");
                    if (userInput.Count() < 4)
                    {
                        Console.WriteLine("Arguments Error! Check again.");
                    }
                    ExtractZIPFile(userInput[1], userInput[2], userInput[3]);
                    break;

                default:
                    Console.WriteLine("Select an appropriate Mode first.");
                    break;
            }

            #endregion
            Console.WriteLine("Press any key...");
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
            Console.WriteLine("-------------------------");
            Console.WriteLine($"attr from File.GetAttributes(rootPath) : {attr}");

            if (attr == FileAttributes.Directory)
            {
                var dirInfo = new DirectoryInfo(rootPath);
                Console.WriteLine($"dirInfo from  new DirectoryInfo(rootPath) : {dirInfo}");
                int count = 1;
                Console.WriteLine($"dirInfo.GetDirectories().Length : {dirInfo.GetDirectories().Length}");
                foreach (var dir in dirInfo.GetDirectories())
                {
                    GetFileList(dir.FullName, fileList);
                    Console.WriteLine($"dir : {dir}");
                    count++;
                }
                Console.WriteLine($"count completed : count({count})");
                foreach (var file in dirInfo.GetFiles().Where(f => f.Extension == ".log" || f.Extension == ".jpg"))
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
        private static void CompressZIPFile(string sourcePath, string zipPath, CompressionLevel compressionLevel = CompressionLevel.Fastest)
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
                                // file( actual file's path ) , path( entry path which is archived as in ZipArchive )
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

        #region Deleting Zipfiles after a certain amount of time
        private static void DeleteZIPFile(string sourcePath, string deleteInterval) // deleteInterval = { "daily", "weekly", "monthly" }
        {
            DirectoryInfo dirInfo = new DirectoryInfo(sourcePath);
            DateTime pivot = DateTime.Today;
            switch (deleteInterval)
            {

                case "daily":
                    pivot = pivot.AddDays(-1);
                    break;
                case "weekly":
                    pivot = pivot.AddDays(-7);
                    break;
                case "monthly":
                    pivot = pivot.AddMonths(-1);
                    break;
            }

            if(dirInfo.Exists)
            {
                FileInfo[] files = dirInfo.GetFiles();
                foreach (FileInfo file in files)
                {
                    if (pivot.ToString("yyyy-MM-dd").CompareTo(file.LastWriteTime.ToString("yyyy-MM-dd")) == 0)
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
        }
        #endregion

        #region Extracting Zipfiles to its own directory        
        private static void ExtractZIPFile(string zipFilesPath, string zipInterval, string date) 
        {
            string zipfile = SearchDailyData(zipFilesPath, date);
            string extractPath = Path.Combine(Path.GetDirectoryName(zipfile), Path.GetFileNameWithoutExtension(zipfile));

            try
            {
                ZipFile.ExtractToDirectory(zipfile, extractPath);
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

        #region Finding Daily Zipfiles
        private static string SearchDailyData(string path, string dailyRecord, string extractPath = "") // dailyRecord = yyyy-MM-dd
        {
            DirectoryInfo dir = new DirectoryInfo(path);
            IEnumerable<FileInfo> fileList = dir.GetFiles("*.*", SearchOption.AllDirectories);

            // from path, get all files( and using LINQ, search especially for .zip)
            // from there, get the zip which matches the date in its name
            IEnumerable<FileInfo> matchedFileQuery =
                from file in fileList
                where file.Extension == ".zip"
                orderby file.Name
                where file.Name.Contains(dailyRecord)
                select file;

            string selectedZipfile = "";
            foreach (var item in matchedFileQuery)
            {
                selectedZipfile = item.FullName;
            }

            Console.WriteLine($"Matched : {selectedZipfile}");
            return selectedZipfile;

            // when the matched zipfile is found, 
            // extract where the zip entries has date in its name
            //foreach(string filename in MatchedZipfile)
            //{
            //    Console.WriteLine(filename);
            //    using (ZipArchive archive = ZipFile.OpenRead(filename))
            //    {
            //        foreach (ZipArchiveEntry entry in archive.Entries.Where(e => e.FullName.Contains("a")))
            //        {
            //            entry.ExtractToFile(Path.Combine(extractPath, entry.FullName));
            //        }
            //    }
            //}

            
        }
        #endregion

        #region Task Scheduler
        private static void AddTaskSchedule(string timeInterval, string stopFlag = "true")
        {
            // Create a new task definition for the local machine and assign properties
            TaskDefinition taskDefinition = TaskService.Instance.NewTask();
            taskDefinition.RegistrationInfo.Description = "Compressing Log files.";

            // trigger settings
            switch (timeInterval)
            {
                case "daily":
                    DailyTrigger dTrigger = new DailyTrigger();
                    dTrigger.StartBoundary = DateTime.Now.AddSeconds(2);
                    dTrigger.DaysInterval = 1;
                    taskDefinition.Triggers.Add(dTrigger);
                    if (stopFlag.ToLower() == "false")
                    {
                        dTrigger.Enabled = false;
                    }
                    dTrigger.Enabled = false;
                    break;
                case "weekly":
                    WeeklyTrigger wTrigger = new WeeklyTrigger();
                    wTrigger.StartBoundary = DateTime.Now.AddSeconds(2);
                    wTrigger.DaysOfWeek = DaysOfTheWeek.Monday;
                    wTrigger.WeeksInterval = 1;
                    taskDefinition.Triggers.Add(wTrigger);
                    if (stopFlag.ToLower() == "false")
                    {
                        wTrigger.Enabled = false;
                    }
                    break;
                case "monthly":
                    // starts 2 seconds later, triggers on the first day of every month, doesn't run on the last day of the month
                    MonthlyTrigger mTrigger = new MonthlyTrigger();
                    mTrigger.StartBoundary = DateTime.Now.AddSeconds(2);
                    mTrigger.DaysOfMonth = new int[]{ 1 };
                    mTrigger.MonthsOfYear = MonthsOfTheYear.AllMonths;
                    mTrigger.RunOnLastDayOfMonth = false; // V2 only
                    taskDefinition.Triggers.Add(mTrigger);
                    if (stopFlag.ToLower() == "false")
                    {
                        mTrigger.Enabled = false;
                    }
                    //OR starts 2 seconds later, triggers on the first week's monday of every month
                    MonthlyDOWTrigger mdTrigger = new MonthlyDOWTrigger();
                    mdTrigger.StartBoundary = DateTime.Now.AddSeconds(2);
                    mdTrigger.DaysOfWeek = DaysOfTheWeek.Monday;
                    mdTrigger.MonthsOfYear = MonthsOfTheYear.AllMonths;
                    mdTrigger.WeeksOfMonth = WhichWeek.FirstWeek;
                    taskDefinition.Triggers.Add(mdTrigger);
                    if (stopFlag.ToLower() == "false")
                    {
                        mdTrigger.Enabled = false;
                    }

                    break;
            }
            
            // action settings 
            ExecAction CogAplex = new ExecAction();
            CogAplex.Path = "zip"; // where exe is located ( full address ) 
            CogAplex.Arguments = "d";  // arguments
            CogAplex.WorkingDirectory = ""; // directory of exe? arguments? 

            taskDefinition.Actions.Add(CogAplex);
            // Register the task in the root folder of the local machine
            TaskService.Instance.RootFolder.RegisterTaskDefinition("CogAplex Log Management System", taskDefinition);
        }
        #endregion
    }
}
