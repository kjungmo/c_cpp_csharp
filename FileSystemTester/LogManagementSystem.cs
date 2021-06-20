﻿using System;
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

            // ||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||
            // |                                                                                      |
            // |                                                                                      |
            // |       [RELEASE] : "args" are RootPath, ZipDate(int), DelDate(int),                   |
            // |                                  interval, [deleteSchedule]                          |
            // |                                                                                      |
            // |                                                                                      |
            // |                                                                                      |
            // ||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||


            #region [[ Development mode ]] ( to be deleted ) 
            //string rootPath = @"e:\Dummy\";
            //List<string> temp = new List<string>();
            //string zipFilePath = Path.Combine(rootPath, "LOG.zip");

            //DateTime zipDate = new DateTime(2021, 5, 1);
            //DateTime deleteDate = new DateTime(2021, 5, 11);
            #endregion

            #region [[ Release mode ]]
            string[] cmdArgs = Environment.GetCommandLineArgs();
            List<string> userInput = cmdArgs.Where(arg => arg != cmdArgs[0]).ToList();

            string rootPath = userInput[0];
            string zip = userInput[1];
            string del = userInput[2];
            string interval = userInput[3];

            List<string> temp = new List<string>();
            DateTime zipDate = DateTime.Today.AddDays(-Convert.ToInt32(zip));
            DateTime deleteDate = zipDate.AddDays(-Convert.ToInt32(del));
            string zipFilePath = Path.Combine(rootPath, "LOG.zip");

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
                    archive.FilterExpiredFilesInZip(deleteDate);
                    archive.HandleLogs(rootPath, zipDate, deleteDate);
                    archive.HandleCapturedImagesByFolder(rootPath, "OK", zipDate, deleteDate, temp);
                    archive.HandleCapturedImagesByFolder(rootPath, "NG", zipDate, deleteDate, temp);
                }

                #region [[ Registering Task ]]
                Scheduler.AddTaskSchedule(Scheduler.CreateSchedulerArguments(zip, del), Scheduler.SelectTrigger(interval));
                if (true)
                {
                    Scheduler.AddTaskSchedule(Scheduler.CreateSchedulerArguments(zip, del), Scheduler.SelectTrigger(interval), false);
                }
                #endregion
                Console.WriteLine("Management Success.");
            }

            Console.WriteLine("No such Root Path");
            Console.ReadKey();
        }
    }
}