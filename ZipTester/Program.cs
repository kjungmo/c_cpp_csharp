using Microsoft.Win32.TaskScheduler;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
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

            //Console.Title = "CogAplex Log File Management System";
            //Console.WriteLine("CogAplex Log File Management System\r");
            //Console.WriteLine("-----------------------------------");
            //Console.WriteLine("<<<  Modes  >>>   ZIP   ||  UNZIP ");
            //Console.WriteLine("[ZIP MODE] [DIR to Save Zipfile] [Log DIR] [Img DIR] [Zip Interval]"); //count 5  -> [ZIP MODE] [Log Directory] [Img Directory] [Zip Directory] [Zipfile Name] [Zip Interval] 
            //Console.WriteLine("[UNZIP MODE] [Zipfiles(plural) Directory] [Zip Interval] [Date]"); // count 4
            //Console.WriteLine("[TASK ENROLL MODE] [Task Scheduler Interval] [Task Scheduler StopFlag]");
            //Console.WriteLine("Put your value in \" \" for more than two things as one argument. ");
            //Console.WriteLine($"Task Scheduler Library version : {TaskService.LibraryVersion}");

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
            //ZipTester.exe zip d:\ZipTest d:\ZipTest\log d:\ZipTest\img weekly
            //ZipTester.exe unzip ‪D:\ZipTest\2021-04_zipp.zip
            switch (userInput[0].ToLower())
            {

                //ZipTester.exe zip e:\ZipTest e:\ZipTest\log e:\ZipTest\img 7
                case "zip":
                    Console.WriteLine("Zip Mode Selected!");
                    //List<string> fileLists = GetFileList(userInput[1], userInput[4]);
                    //CompressZIPFile(MakeZipDir(userInput[1], userInput[4]), userInput[2], userInput[3], userInput[4], CompressionLevel.Optimal);
                    //Compressor.CompressFiles(Compressor.CreateZipFileDirectory(userInput[1], userInput[4]), userInput[2], userInput[3], userInput[4], CompressionLevel.Optimal);
                    //DeleteLogFiles(userInput[1], userInput[3]);
                    //Compressor.DeleteZipfile(userInput[1], userInput[4]);
                    Compressor.(userInput[1], userInput[4]);
					break;
                    
                case "unzip":
                    //Application.Run(form);
                    Console.WriteLine("Unzip Mode Selected!");

                    ExtractZIPFile(userInput[1], userInput[2], userInput[3]);
                    break;

                case "task":
                    AddTaskSchedule(userInput[3]);
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
        private static string MakeZipDir(string folderName, string timeInterval, string fileName = "logs.zip")
        {
            string directory = "";
            string today;
            DateTime dateToday = DateTime.Today; // the compression starts the day after the logs are written [AddDays(-1)]
            switch (timeInterval.ToLower())
            {
                case "daily":
                    today = string.Format("{0:d}", dateToday.AddDays(-1));   // {0:d} -> yyyy-MM-dd format
                    string daily = string.Concat(today, "_");
                    directory = folderName + "\\" + daily + fileName;  // yyyy-MM-dd_logs.zip
                    break;
                case "weekly":
                    string week1 = string.Format("{0:d}", dateToday.AddDays(-7));
                    string week7 = string.Format("{0:d}", dateToday.AddDays(-1));
                    string weekly = string.Concat(week1, "_", week7, "_");
                    directory = folderName + "\\" + weekly + fileName; // yyyy-MM-dd_yyyy-MM-dd_logs.zip
                    break;
                case "monthly":
                    string month = dateToday.AddMonths(-1).ToString("yyyy-MM");
                    string monthly = string.Concat(month, "_");
                    directory = folderName + "\\" + monthly + fileName; // yyyy-MM_logs.zip
                    break;
            }
            return directory;
        }
        #endregion

        #region getting File Lists
        private static List<string> GetFileList(String rootPath, List<String> fileList)
        {
            if (fileList == null) // validity inspection
            {
                return null;
            }

            var attr = File.GetAttributes(rootPath); // whether the rootPath is either a Directory or an Archive

            if (attr == FileAttributes.Directory)
            {
                var dirInfo = new DirectoryInfo(rootPath);
                foreach (var dir in dirInfo.GetDirectories())
                {
                    GetFileList(dir.FullName, fileList);
                }

                foreach (var file in dirInfo.GetFiles().Where(f => f.Extension == ".log" || f.Extension == ".jpg"))
                {
                    GetFileList(file.FullName, fileList);
                }
            }
            else if (attr == FileAttributes.Archive) // if it's a compressed or a zipfile, it becomes excluded
            {
                var fileInfo = new FileInfo(rootPath);
                if (fileInfo.Extension == ".log" || fileInfo.Extension == ".jpg")
                    fileList.Add(fileInfo.FullName);
                
            }
            return fileList;
        }
        #endregion

        #region Compressing Files into Zipfile 
        // arguments needs to be modified
        // (zipPath's Parent Dir , logPath, imgPath, Interval, compressionLevel ) 
        private static void CompressZIPFile(string zipPath, string logPath, string imgPath, 
            string zipInterval, CompressionLevel compressionLevel = CompressionLevel.Fastest)
        {
            if (!File.Exists(zipPath))
            {
                using (FileStream fileStream = new FileStream(zipPath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                {
                    using (ZipArchive zipArchive = new ZipArchive(fileStream, ZipArchiveMode.Create))
                    {
                        List<string> logImgPaths = new List<string> { logPath, imgPath };
                        foreach (var logOrImg in logImgPaths)
                        {
                            switch (zipInterval.ToLower())
                            {
                                case "daily":
                                    var matchedFileQuery = (from file in GetFileList(logOrImg, new List<string>())
                                                            where Path.GetExtension(file) == ".log"
                                                            || Path.GetExtension(file) == ".jpg"
                                                            where file.Contains(DateTime.Today.AddDays(-1).ToString("yyyy-MM-dd")) // compresses the logs that were made yesterday
                                                            select file).ToList();

                                    foreach (var file in matchedFileQuery)
                                    {
                                        string path = file.Substring(logOrImg.Length + 1);
                                        try
                                        {
                                            zipArchive.CreateEntryFromFile(file, path, compressionLevel); // if already exists, throws IOException
                                            File.Delete(file);          // file( actual file's path ) , path( entry name(or path in ziparchive) which is archived as in ZipArchive )
                                        }
                                        catch (Exception e)
                                        {

                                        }
                                    }
                                    break;

                                case "weekly":
                                    for (int i = 1; i <= 7; i++)
                                    {
                                        matchedFileQuery = (from file in GetFileList(logOrImg, new List<string>())
                                                            where Path.GetExtension(file) == ".log"
                                                            || Path.GetExtension(file) == ".jpg"
                                                            where file.Contains(DateTime.Today.AddDays(-i).ToString("yyyy-MM-dd")) // compresses the logs that were made yesterday
                                                            select file).ToList();

                                        foreach (var file in matchedFileQuery)
                                        {
                                            string path = file.Substring(logOrImg.Length + 1);
                                            try
                                            {
                                                zipArchive.CreateEntryFromFile(file, path, compressionLevel); // if already exists, throws IOException
                                                File.Delete(file);          // file( actual file's path ) , path( entry name(or path in ziparchive) which is archived as in ZipArchive )
                                            }
                                            catch (Exception e)
                                            {

                                            }
                                        }
                                    }
                                    break;

                                //List<string> weekdays = new List<string> {
                                //    DateTime.Today.AddDays(-1).ToString("yyyy-MM-dd"), // compresses the logs that were made a week ago starting from yesterday
                                //    DateTime.Today.AddDays(-2).ToString("yyyy-MM-dd"),
                                //    DateTime.Today.AddDays(-3).ToString("yyyy-MM-dd"),
                                //    DateTime.Today.AddDays(-4).ToString("yyyy-MM-dd"),
                                //    DateTime.Today.AddDays(-5).ToString("yyyy-MM-dd"),
                                //    DateTime.Today.AddDays(-6).ToString("yyyy-MM-dd"),
                                //    DateTime.Today.AddDays(-7).ToString("yyyy-MM-dd")
                                //};
                                //matchedFileQuery = (from file in GetFileList(logOrImg, new List<string>())
                                //                        where Path.GetExtension(file) == ".log" 
                                //                        || Path.GetExtension(file) == ".jpg"
                                //                        where weekdays.Contains(File.GetCreationTime(file).ToString("yyyy-MM-dd"))
                                //                        select file).ToList();

                                //foreach (var file in matchedFileQuery)
                                //{
                                //    string path = file.Substring(logOrImg.Length + 1);
                                //    try
                                //    {
                                //        zipArchive.CreateEntryFromFile(file, path, compressionLevel); // if already exists, throws IOException
                                //        File.Delete(file);          // file( actual file's path ) , path( entry name(or path in ziparchive) which is archived as in ZipArchive )
                                //    }
                                //    catch (Exception e)
                                //    {

                                //    }
                                //}
                                //break;

                                case "monthly":
                                    matchedFileQuery = (from file in GetFileList(logOrImg, new List<string>())
                                                        where Path.GetExtension(file) == ".log"
                                                        || Path.GetExtension(file) == ".jpg"
                                                        where file.Contains(DateTime.Today.AddMonths(-1).ToString("yyyy-MM")) // compresses the logs that were made yesterday
                                                        select file).ToList();

                                    foreach (var file in matchedFileQuery)
                                    {
                                        string path = file.Substring(logOrImg.Length + 1);
                                        try
                                        {
                                            zipArchive.CreateEntryFromFile(file, path, compressionLevel); // if already exists, throws IOException
                                            File.Delete(file);          // file( actual file's path ) , path( entry name(or path in ziparchive) which is archived as in ZipArchive )
                                        }
                                        catch (Exception e)
                                        {

                                        }
                                    }
                                    break;
                            }

                            //DeleteLogFiles(logOrImg, zipInterval);
                        }
                    }
                    Console.WriteLine("Created!");
                }
            }
            using (FileStream fileStream = new FileStream(zipPath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                using (ZipArchive zipArchive = new ZipArchive(fileStream, ZipArchiveMode.Create))
                {
                    List<string> logImgPaths = new List<string> { logPath, imgPath };
                    foreach (var logOrImg in logImgPaths)
                    {
                        switch (zipInterval.ToLower())
                        {
                            case "daily":
                                var matchedFileQuery = (from file in GetFileList(logOrImg, new List<string>())
                                                        where Path.GetExtension(file) == ".log"
                                                        || Path.GetExtension(file) == ".jpg"
                                                        where file.Contains(DateTime.Today.AddDays(-1).ToString("yyyy-MM-dd")) // compresses the logs that were made yesterday
                                                        select file).ToList();

                                foreach (var file in matchedFileQuery)
                                {
                                    string path = file.Substring(logOrImg.Length + 1);
                                    try
                                    {
                                        zipArchive.CreateEntryFromFile(file, path, compressionLevel); // if already exists, throws IOException
                                        File.Delete(file);          // file( actual file's path ) , path( entry name(or path in ziparchive) which is archived as in ZipArchive )
                                    }
                                    catch (Exception e)
                                    {

                                    }
                                }
                                break;

                            case "weekly":
                                for (int i = 1; i <= 7; i++)
                                {
                                    matchedFileQuery = (from file in GetFileList(logOrImg, new List<string>())
                                                        where Path.GetExtension(file) == ".log"
                                                        || Path.GetExtension(file) == ".jpg"
                                                        where file.Contains(DateTime.Today.AddDays(-i).ToString("yyyy-MM-dd")) // compresses the logs that were made yesterday
                                                        select file).ToList();

                                    foreach (var file in matchedFileQuery)
                                    {
                                        string path = file.Substring(logOrImg.Length + 1);
                                        try
                                        {
                                            zipArchive.CreateEntryFromFile(file, path, compressionLevel); // if already exists, throws IOException
                                            File.Delete(file);          // file( actual file's path ) , path( entry name(or path in ziparchive) which is archived as in ZipArchive )
                                        }
                                        catch (Exception e)
                                        {

                                        }
                                    }
                                }
                                break;

                            //List<string> weekdays = new List<string> {
                            //    DateTime.Today.AddDays(-1).ToString("yyyy-MM-dd"), // compresses the logs that were made a week ago starting from yesterday
                            //    DateTime.Today.AddDays(-2).ToString("yyyy-MM-dd"),
                            //    DateTime.Today.AddDays(-3).ToString("yyyy-MM-dd"),
                            //    DateTime.Today.AddDays(-4).ToString("yyyy-MM-dd"),
                            //    DateTime.Today.AddDays(-5).ToString("yyyy-MM-dd"),
                            //    DateTime.Today.AddDays(-6).ToString("yyyy-MM-dd"),
                            //    DateTime.Today.AddDays(-7).ToString("yyyy-MM-dd")
                            //};
                            //matchedFileQuery = (from file in GetFileList(logOrImg, new List<string>())
                            //                        where Path.GetExtension(file) == ".log" 
                            //                        || Path.GetExtension(file) == ".jpg"
                            //                        where weekdays.Contains(File.GetCreationTime(file).ToString("yyyy-MM-dd"))
                            //                        select file).ToList();

                            //foreach (var file in matchedFileQuery)
                            //{
                            //    string path = file.Substring(logOrImg.Length + 1);
                            //    try
                            //    {
                            //        zipArchive.CreateEntryFromFile(file, path, compressionLevel); // if already exists, throws IOException
                            //        File.Delete(file);          // file( actual file's path ) , path( entry name(or path in ziparchive) which is archived as in ZipArchive )
                            //    }
                            //    catch (Exception e)
                            //    {

                            //    }
                            //}
                            //break;

                            case "monthly":
                                matchedFileQuery = (from file in GetFileList(logOrImg, new List<string>())
                                                    where Path.GetExtension(file) == ".log"
                                                    || Path.GetExtension(file) == ".jpg"
                                                    where file.Contains(DateTime.Today.AddMonths(-1).ToString("yyyy-MM")) // compresses the logs that were made yesterday
                                                    select file).ToList();

                                foreach (var file in matchedFileQuery)
                                {
                                    string path = file.Substring(logOrImg.Length + 1);
                                    try
                                    {
                                        zipArchive.CreateEntryFromFile(file, path, compressionLevel); // if already exists, throws IOException
                                        File.Delete(file);          // file( actual file's path ) , path( entry name(or path in ziparchive) which is archived as in ZipArchive )
                                    }
                                    catch (Exception e)
                                    {

                                    }
                                }
                                break;
                        }

                        //DeleteLogFiles(logOrImg, zipInterval);
                    }
                }
                Console.WriteLine("Created!");
            }
        }
        #endregion

        #region Deleting Files after the files has been Compressed 
        private static void DeleteLogFiles(string sourcePath, string deleteInterval) // File.delete(File) in CompressZIPFile makes it more delicate 
        {
            DirectoryInfo dirInfo = new DirectoryInfo(sourcePath);

            switch (deleteInterval.ToLower())
            {
                //case "daily":
                //    foreach (FileInfo file in dirInfo.GetFiles().Where(f => f.Extension == ".log" || f.Extension == ".jpg"))
                //    {
                //        Console.WriteLine(file.FullName);
                //        file.Delete();
                //    }
                //    Console.WriteLine("Deleted!");
                //    break;
                case "weekly":
                    break;
                case "monthly":
                    break;
            }
            
            
        }
        #endregion

        #region Deleting Zipfiles after a certain amount of time
        private static void DeleteZIPFiles(string sourcePath, string deleteInterval) // deleteInterval = { "daily", "weekly", "monthly" }
        {
            DirectoryInfo dirInfo = new DirectoryInfo(sourcePath);
            
            DateTime pivot = DateTime.Today;
            switch (deleteInterval.ToLower())
            {

                case "daily":
                    if (dirInfo.Exists)
                    {
                        FileInfo[] files = dirInfo.GetFiles();
                        foreach (FileInfo file in files)
                        {
                            if (System.Text.RegularExpressions.Regex.IsMatch(file.Name, ".zip"))
                            {
                                if (pivot.AddDays(-2).ToString("yyyy-MM-dd").CompareTo(file.CreationTime.ToString("yyyy-MM-dd")) == 0)
                                {
                                    string filedir = dirInfo + "\\" + file.Name;
                                    Console.WriteLine(filedir);
                                    File.Delete(filedir);
                                    Console.WriteLine("Deleted");
                                }
                            }
                        }
                    }
                    break;
                case "weekly":
                    //pivot = pivot.AddDays(-7);

                    //List<string> weekdays = new List<string> {
                    //    DateTime.Today.AddDays(-1).ToString("yyyy-MM-dd"), // compresses the logs that were made a week ago starting from yesterday
                    //    DateTime.Today.AddDays(-2).ToString("yyyy-MM-dd"),
                    //    DateTime.Today.AddDays(-3).ToString("yyyy-MM-dd"),
                    //    DateTime.Today.AddDays(-4).ToString("yyyy-MM-dd"),
                    //    DateTime.Today.AddDays(-5).ToString("yyyy-MM-dd"),
                    //    DateTime.Today.AddDays(-6).ToString("yyyy-MM-dd"),
                    //    DateTime.Today.AddDays(-7).ToString("yyyy-MM-dd")
                    //};
                    //matchedFileQuery = (from file in GetFileList(logOrImg, new List<string>())
                    //                        where Path.GetExtension(file) == ".log" 
                    //                        || Path.GetExtension(file) == ".jpg"
                    //                        where weekdays.Contains(File.GetCreationTime(file).ToString("yyyy-MM-dd"))
                    //                        select file).ToList();


                    if (dirInfo.Exists)
                    {
                        FileInfo[] files = dirInfo.GetFiles();
                        foreach (FileInfo file in files)
                        {
                            if (System.Text.RegularExpressions.Regex.IsMatch(file.Name, ".zip"))
                            {
                                if (pivot.AddDays(-7).ToString("yyyy-MM-dd").CompareTo(file.CreationTime.ToString("yyyy-MM-dd")) == 0)
                                {
                                    string filedir = dirInfo + "\\" + file.Name;
                                    Console.WriteLine(filedir);
                                    File.Delete(filedir);
                                    Console.WriteLine("Deleted");
                                }
                            }
                        }
                    }
                    break;
                case "monthly":
                    //pivot = pivot.AddMonths(-1);
                    if (dirInfo.Exists)
                    {
                        FileInfo[] files = dirInfo.GetFiles();
                        foreach (FileInfo file in files)
                        {
                            if (System.Text.RegularExpressions.Regex.IsMatch(file.Name, ".zip"))
                            {
                                if (pivot.AddMonths(-2).ToString("yyyy-MM").CompareTo(file.CreationTime.ToString("yyyy-MM")) == 0)
                                {
                                    string filedir = dirInfo + "\\" + file.Name;
                                    Console.WriteLine(filedir);
                                    File.Delete(filedir);
                                    Console.WriteLine("Deleted");
                                }
                            }
                        }
                    }
                    break;
            }

            
        }
        #endregion

        #region Extracting Zipfiles to its own directory        
        private static void ExtractZIPFile(string zipFilesPath, string zipInterval, string date) 
        {
            string zipfile = "";
            string extractFolderPath = "";
            switch (zipInterval.ToLower())
            {
                case "daily":
                    zipfile = SearchDailyData(zipFilesPath, date);
                    extractFolderPath = Path.Combine(Path.GetDirectoryName(zipfile), Path.GetFileNameWithoutExtension(zipfile));
                    if(!Directory.Exists(extractFolderPath))
                    {
                        Directory.CreateDirectory(extractFolderPath);
                    }
                    try
                    {
                        ZipFile.ExtractToDirectory(zipfile, extractFolderPath);
                    }
                    catch (Exception e)
                    {

                    }
                    break;

                case "weekly":
                    zipfile = SearchWeeklyData(zipFilesPath, date);
                    extractFolderPath = Path.Combine(Path.GetDirectoryName(zipfile), Path.GetFileNameWithoutExtension(zipfile));
                    try
                    {
                        //extractToFile
                        ZipFile.ExtractToDirectory(zipfile, extractFolderPath);
                    }
                    catch (Exception e)
                    {

                    }
                    break;

                case "monthly":
                    zipfile = SearchMonthlyData(zipFilesPath, date);
                    extractFolderPath = Path.Combine(Path.GetDirectoryName(zipfile), Path.GetFileNameWithoutExtension(zipfile), "[", date, "]");
                    if (!Directory.Exists(extractFolderPath))
                    {
                        Directory.CreateDirectory(extractFolderPath);
                    }
                    try
                    {
                        ZipFile.ExtractToDirectory(zipfile, extractFolderPath);
                    }
                    catch (Exception e)
                    {

                    }
                    break;

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
        private static string SearchDailyData(string path, string dailyRecord) // dailyRecord = yyyy-MM-dd
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
        }
        #endregion

        #region Finding Weekly Zipfiles
        private static string SearchWeeklyData(string path, string weeklyRecord) // weeklyRecord = yyyy-MM-dd
        {
            string selectedZipfile = "";
            DirectoryInfo dir = new DirectoryInfo(path);
            IEnumerable<FileInfo> fileList = dir.GetFiles("*.*", SearchOption.AllDirectories);

            DateTime convertedFromString = DateTime.Parse(weeklyRecord);
            List<string> daysInAWeek = new List<string>
            { convertedFromString.AddDays(-1).ToString("yyyy-MM-dd"),
              convertedFromString.AddDays(-2).ToString("yyyy-MM-dd"),
              convertedFromString.AddDays(-3).ToString("yyyy-MM-dd"),
              convertedFromString.AddDays(-4).ToString("yyyy-MM-dd"),
              convertedFromString.AddDays(-5).ToString("yyyy-MM-dd")
            };

            // from path, get all files( and using LINQ, search especially for .zip)
            // from there, get the zip which matches the date in its name
            IEnumerable<FileInfo> matchedFileQuery1 =
                from file in fileList
                where file.Extension == ".zip"
                orderby file.Name
                where file.Name.Contains(weeklyRecord)
                select file;

            if(matchedFileQuery1.Count() != 0)
            {
                foreach (var item in matchedFileQuery1)
                {
                    selectedZipfile = item.FullName;
                }

                Console.WriteLine($"Matched : {selectedZipfile}");
                return selectedZipfile;
            }
            else
            {
                IEnumerable<FileInfo> matchedFileQuery2 =
                    from file in fileList
                    where file.Extension == ".zip"
                    orderby file.Name
                    where daysInAWeek.Any(date => file.Name.Contains(date))
                    select file;

                foreach (var item in matchedFileQuery2)
                {
                    selectedZipfile = item.FullName;
                }

                Console.WriteLine($"Matched : {selectedZipfile}");
                return selectedZipfile;
            }


            // when the matched zipfile is found, 
            // extract where the zip entries has date in its name
            //foreach(string filename in MatchedZipfile)
            //{
            //    Console.WriteLine(filename);
            //    using (ZipArchive archive = ZipFile.OpenRead(filename))
            //    {
            //        foreach (ZipArchiveEntry entry in archive.Entries.Where(e => e.FullName.Contains(monthlyRecord)))
            //        {
            //            entry.ExtractToFile(Path.Combine(extractPath, entry.FullName));
            //        }
            //    }
            //}


        }
        #endregion

        #region Finding Monthly Zipfiles
        private static string SearchMonthlyData(string path, string monthlyRecord) // monthlyRecord = yyyy-MM-dd
        {
            string monthZipfile = monthlyRecord.Substring(0, 6);
            DirectoryInfo dir = new DirectoryInfo(path);
            IEnumerable<FileInfo> fileList = dir.GetFiles("*.*", SearchOption.AllDirectories);

            // from path, get all files( and using LINQ, search especially for .zip)
            // from there, get the zip which matches the date in its name
            IEnumerable<FileInfo> matchedFileQuery =
                from file in fileList
                where file.Extension == ".zip"
                orderby file.Name
                where file.Name.Contains(monthZipfile)
                select file;
            // handling monthly zip file 
            // but how to get certain date's data into certain date's folder
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
            //        foreach (ZipArchiveEntry entry in archive.Entries.Where(e => e.FullName.Contains(monthlyRecord)))
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
                case "daily": // 오늘분을 저장해야 하니까 하루가 지나서 실행되어야 한다?
                    DailyTrigger dTrigger = new DailyTrigger();
                    dTrigger.StartBoundary = DateTime.Today.AddDays(1);
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
            CogAplex.WorkingDirectory = ""; // directory of exe OR dir of files that exe uses 

            taskDefinition.Actions.Add(CogAplex);
            // Register the task in the root folder of the local machine
            TaskService.Instance.RootFolder.RegisterTaskDefinition("CogAplex Log Management System", taskDefinition);
        }
        #endregion
    }
}
