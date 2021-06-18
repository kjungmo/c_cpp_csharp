using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace FileSystemTester
{
    public class LogManagementSystem
    {
        static void Main(string[] args)
        {

            // ||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||
            // |                                                                                      |
            // |                                                                                      |
            // |       [RELEASE] : "args" are RootPath, ZipDate(int), DelDate(int),                   |
            // |                                  interval, [deleteSchedule]                          |
            // |                                                                                      |
            // |                                                                                      |
            // |                                                                                      |
            // ||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||


            #region Development mode ( to be deleted ) 
            string rootPath = @"e:\Dummy\";
            List<string> temp = new List<string>();
            string zipFilePath = Path.Combine(rootPath, "LOG.zip");

            DateTime startZip = new DateTime(2021, 5, 1);
            DateTime startDel = new DateTime(2021, 5, 11);
            #endregion

            #region Release mode
            //DateTime startZip = DateTime.Today.AddDays(-10);
            //DateTime startZip = DateTime.Today.AddDays(-args1);

            //DateTime startDel = startZip.AddDays(-10);
            //DateTime startDel = startZip.AddDays(-args2);
            #endregion
            if (Directory.Exists(rootPath))
            {
                if (!File.Exists(zipFilePath))
                {
                    Console.WriteLine("No .zip File");
                    var myfile = File.Create(zipFilePath);
                    myfile.Close();
                }
                using (ZipArchive archive = ZipFile.Open(zipFilePath, ZipArchiveMode.Update))
                {
                    archive.FilterExpiredFilesInZip(startDel);

                    archive.HandleLogs(rootPath, startZip, startDel);

                    archive.HandleCapturedImages(rootPath, "OK", startZip, startDel, temp);
                    archive.HandleCapturedImages(rootPath, "NG", startZip, startDel, temp);
                }

                // Register to Windows TaskScheduler 
                #region Registering Task
                string arguments = Scheduler.CreateSchedulerArguments(startZip.ToString(), startDel.ToString());
                string interval = "";
                Scheduler.AddTaskSchedule(interval, arguments);

                Scheduler.AddTaskSchedule(interval, arguments, false);

                #endregion
            }


            Console.WriteLine("SourcePath None");
            Console.WriteLine("Management Success.");
            Console.ReadKey();
        }
    }
}