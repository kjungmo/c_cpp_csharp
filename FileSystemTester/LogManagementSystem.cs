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
            // |      [SCHEDULER MODE] : "args" are Mode, RootPath, ZipDaysAfterLogged(int), DeleteDaysAfterZip(int), interval         |
            // |                                                                                                                       |
            // |   [DEFAULT MODE(zip)] : "args" are RootPath, ZipDaysAfterLogged(int), DelDaysAfterZip(int)                            |
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

            string rootPath, zipDaysAfterLogged, deleteDaysAfterZip;
            List<string> temp = new List<string>();

            if(userInput[0].ToLower() =="scheduler")
            {
                Console.WriteLine("TaskScheduler Registration Mode Selected!");
                rootPath = userInput[1];
                zipDaysAfterLogged = userInput[2];
                deleteDaysAfterZip = userInput[3];
                string interval = userInput[4];

                Scheduler taskScheduler = new Scheduler(rootPath ,zipDaysAfterLogged, deleteDaysAfterZip, interval);
                taskScheduler.AddTaskSchedule(taskScheduler.SelectTrigger());
                if (userInput.Count() > 5 && userInput[5].ToLower() == "-stop")
                {
                    taskScheduler.AddTaskSchedule(taskScheduler.SelectTrigger(false));
                }
            }

            else if (userInput[0].ToLower() == "zip")
            {
                rootPath = userInput[0];
                zipDaysAfterLogged = userInput[1];
                deleteDaysAfterZip = userInput[2];

                DateTime zipDate = DateTime.Today.AddDays(-Convert.ToInt32(zipDaysAfterLogged));
                DateTime deleteDate = zipDate.AddDays(-Convert.ToInt32(deleteDaysAfterZip));
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
                        archive.SortCapturedImagesByFolder(rootPath, "OK", zipDate, deleteDate, temp);
                        archive.SortCapturedImagesByFolder(rootPath, "NG", zipDate, deleteDate, temp);
                    }
                    Console.WriteLine("Management Success.");
                }
            }

            else
            {
                Console.WriteLine("Select Correct Mode");
                Console.WriteLine("To Register .exe file to Windows Task Scheduler : [scheduler] ");
                Console.WriteLine("To sort files and manage them : [zip]");
            }
            
            Console.WriteLine("No such Root Path");
            Console.ReadKey();
        }
    }
}