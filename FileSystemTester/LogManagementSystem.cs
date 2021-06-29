using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace LogManagementSystem
{
    public class LogManagementSystem
    {
        static void Main(string[] args)
        {
            // |||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||
            // |                                                                                                                       |
            // |                                                                                                                       |
            // |      [SCHEDULER MODE] : "args" are Mode, RootPath, numOfDaysUntilZip(int), numOfDaysUntilDelete(int), interval        |
            // |                                                                                                                       |
            // |   [DEFAULT MODE(zip)] : "args" are RootPath, numOfDaysUntilZip(int), numOfDaysUntilDelete(int)                        |
            // |                                                                                                                       |
            // |                                                                                                                       |
            // |||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||


            #region [[ Development mode ]] ( to be deleted ) 
            //string rootPath = @"e:\Dummy\";
            //List<string> temp = new List<string>();
            //string zipFilePath = Path.Combine(rootPath, "LOG.zip");

            //DateTime zipDate = new DateTime(2021, 5, 1);
            //DateTime deleteDate = new DateTime(2021, 5, 11);
            #endregion

            #region [[ Release mode ]]
            #region Variables
            string[] cmdArgs = Environment.GetCommandLineArgs();
            List<string> userInput = cmdArgs.Where(arg => arg != cmdArgs[0]).ToList();

            //string rootPath = userInput[1];
            //string zip = userInput[2];
            //string del = userInput[3];
            //string interval = userInput[4];

            //List<string> temp = new List<string>();
            //DateTime zipDate = DateTime.Today.AddDays(-Convert.ToInt32(zip));
            //DateTime deleteDate = zipDate.AddDays(-Convert.ToInt32(del));
            //string zipFilePath = Path.Combine(rootPath, "LOG.zip");
            #endregion
            //DateTime startZip = DateTime.Today.AddDays(-10);
            //DateTime startZip = DateTime.Today.AddDays(-args1);

            //DateTime startDel = startZip.AddDays(-10);
            //DateTime startDel = startZip.AddDays(-args2);
            #endregion

            List<string> temp = new List<string>();
            string rootPath;
            int numOfDaysUntilZip;
            int numOfDaysUntilDelete;

            if (userInput[0].ToLower() =="schedulerZ" && userInput.Count() == 5)
            {
                rootPath = userInput[1];
                numOfDaysUntilZip = Convert.ToInt32(userInput[2]);
                numOfDaysUntilDelete = Convert.ToInt32(userInput[3]);
                string interval = userInput[4];
                string weekday = "monday";
                int month = 1;
                int dayInMonth = 1;
                Scheduler taskScheduler = new Scheduler("zip", rootPath ,numOfDaysUntilZip, numOfDaysUntilDelete, interval, weekday, month, dayInMonth); // Setting Manager -> Date input
                taskScheduler.AddTaskSchedule(taskScheduler.CreateTrigger());
            }
            
            else if (userInput[0].ToLower() == "zip" && userInput.Count() == 4)
            {
                rootPath = userInput[1];
                numOfDaysUntilZip = Convert.ToInt32(userInput[2]);
                numOfDaysUntilDelete = Convert.ToInt32(userInput[3]);
                DateTime zipDate = DateTime.Today.AddDays(-numOfDaysUntilZip);
                DateTime deleteDate = zipDate.AddDays(-numOfDaysUntilDelete);
                string zipFilePath = Path.Combine(rootPath, "LOG.zip");

                if (Directory.Exists(rootPath))
                {
                    if (!File.Exists(zipFilePath))
                    {
                        try
                        {
                            var myfile = File.Create(zipFilePath);
                            myfile.Close();
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine($"{e.Message} \nUnable to create zipfile");
                        }
                    }

                    using (ZipArchive archive = ZipFile.Open(zipFilePath, ZipArchiveMode.Update))
                    {
                        archive.FilterExpiredFilesInZip(deleteDate);
                        archive.SortLogs(rootPath, zipDate, deleteDate);
                        archive.SortCapturedImagesByFolder(rootPath, zipDate, deleteDate, temp);
                        archive.SortCSVs(rootPath, zipDate, deleteDate, temp);
                    }
                    Console.WriteLine("Management Success.");
                }
            }

            else if (userInput[0].ToLower() == "scheduler")
            {
                rootPath = userInput[1];
                Scheduler taskScheduler = new Scheduler("manage", rootPath, userInput[2], userInput[3], userInput[4]); // Setting Manager -> Date input
                taskScheduler.AddTaskSchedule(taskScheduler.CreateTrigger());
            }

            else if (userInput[0].ToLower() == "manage")
            {
                rootPath = userInput[1];
                DateTime deleteLog = DateTime.Today.AddDays(-Convert.ToInt32(userInput[2]));
                DateTime deleteImg = DateTime.Today.AddDays(-Convert.ToInt32(userInput[3]));
                DateTime deleteCsv = DateTime.Today.AddDays(-Convert.ToInt32(userInput[4]));

                ZipHelper.SortLogs(rootPath, deleteLog);
                ZipHelper.SortCapturedImageByFolder(rootPath, deleteImg);
                ZipHelper.SortCSVs(rootPath, deleteCsv);
            }

            else if (userInput[0].ToLower() == "schdedule_delete")
            {
                Scheduler deleteSchedule = new Scheduler();
                deleteSchedule.DeleteTaskSchedule();
            }
            Console.ReadKey();
        }
    }
}