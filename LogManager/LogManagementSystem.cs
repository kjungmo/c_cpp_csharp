using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace LogManager
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

            switch (userInput[0].ToLower())
            {
                case "scheduler_z":
                    string rootPath = userInput[1];
                    int numOfDaysUntilZip = Convert.ToInt32(userInput[2]);
                    int numOfDaysUntilDelete = Convert.ToInt32(userInput[3]);
                    string interval = userInput[4];
                    string weekday = "monday";
                    int month = 1;
                    int dayInMonth = 1;
                    ScheduleRegister.Scheduler taskScheduler = new ScheduleRegister.Scheduler("zip", rootPath, numOfDaysUntilZip, numOfDaysUntilDelete, interval, weekday, month, dayInMonth); // Setting Manager -> Date input
                    taskScheduler.AddTaskSchedule();
                    break;

                case "zip":
                    List<string> temp = new List<string>();
                    rootPath = userInput[1];
                    DateTime zipDate = DateTime.Today.AddDays(-Convert.ToInt32(userInput[2]));
                    DateTime deleteDate = zipDate.AddDays(-Convert.ToInt32(userInput[3]));
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
                            archive.FilterZipFileEntries(deleteDate);
                            archive.SortLogs(rootPath, zipDate, deleteDate);
                            archive.SortImgs(rootPath, zipDate, deleteDate, temp);
                            archive.SortCSVs(rootPath, zipDate, deleteDate, temp);
                        }
                        Console.WriteLine("Management Success.");
                    }
                    break;

                case "scheduler":
                    rootPath = userInput[1];
                    string deleteLog = userInput[2];
                    string deleteImg = userInput[3];
                    string deleteCsv = userInput[4];
                    taskScheduler = new ScheduleRegister.Scheduler("manage", rootPath, deleteLog, deleteImg, deleteCsv); // Setting Manager -> Date input
                    taskScheduler.AddTaskSchedule();
                    break;

                case "manage":
                    rootPath = userInput[1];

                    ZipHelper.SortLogs(rootPath, DateTime.Today.AddDays(-Convert.ToInt32(userInput[2])));
                    ZipHelper.SortImgs(rootPath, DateTime.Today.AddDays(-Convert.ToInt32(userInput[3])));
                    ZipHelper.SortCSVs(rootPath, DateTime.Today.AddDays(-Convert.ToInt32(userInput[4])));
                    break;

                case "schedule_delete":
                    ScheduleRegister.Scheduler.DeleteTaskSchedule();
                    break;

            }
            Console.ReadKey();
        }
    }
}