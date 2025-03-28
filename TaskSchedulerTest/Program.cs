﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Win32.TaskScheduler;

namespace TaskSchedulerTest
{
    class Program
    {
        static void Main(string[] args)
        {

            //string path = @"d:\ZipTest";
            //GetFileList(path, new List<string>());

            string file = @"C:\Users\kangj\Downloads\새 폴더\2021-03-04.log";
            //new FileInfo(file).CreationTime.CompareTo(DateTime.Now);
            //Console.WriteLine(new FileInfo(file).CreationTime);
            //Console.WriteLine(DateTime.Today);
            //Console.WriteLine(new FileInfo(file).CreationTime.CompareTo(DateTime.Today));
            string format = "yyyy-MM-dd";
            DateTime today = DateTime.Today;
            DateTime yesterday = DateTime.Today.AddDays(-1);
            Console.WriteLine(today.ToString(format));
            Console.WriteLine(yesterday.ToString(format));


            Console.ReadKey();

            //string dateWeekAgo = DateTime.Today.AddDays(-7).ToString();

            //dateWeekAgo.CompareTo(DateTime.Today.a.ToString());
            //string date2 = DateTime.Today.AddDays(-7).ToString();
            //string date3 = DateTime.Today.AddDays(-14).ToString();
            //Console.WriteLine($"date1 : {date1}");
            //Console.WriteLine($"date2 : {date2}");
            //Console.WriteLine($"date3 : {date3}");
            //Console.WriteLine($"date1.compared to date1 : {date1.CompareTo(date1)}");
            //Console.WriteLine($"date1.compared to date2 : {date1.CompareTo(date2)}");
            //Console.WriteLine($"date1.compared to date3 : {date1.CompareTo(date3)}");
            //Console.WriteLine($"date2.compared to date1 : {date2.CompareTo(date1)}");
            //Console.WriteLine($"date2.compared to date2 : {date2.CompareTo(date2)}");
            //Console.WriteLine($"date2.compared to date3 : {date2.CompareTo(date3)}");
            //Console.WriteLine($"date3.compared to date1 : {date3.CompareTo(date1)}");
            //Console.WriteLine($"date3.compared to date2 : {date3.CompareTo(date2)}");
            //Console.WriteLine($"date3.compared to date3 : {date3.CompareTo(date3)}");

            //DateTime dateToday = DateTime.Today;
            //Console.WriteLine($"Today : {dateToday}");

            //string today = string.Format("{0:d}", dateToday);
            //string lastWeek = string.Format("{0:d}", dateToday.AddDays(-7));
            //Console.WriteLine($"today : {today}");
            //Console.WriteLine($"lastWeek : {lastWeek}");
            //Console.ReadLine();

            // Create a new task definition for the local machine and assign properties
            //TaskDefinition td = TaskService.Instance.NewTask();

            //td.RegistrationInfo.Description = "Does something";

            //// Add a trigger that, starting tomorrow, will fire every other week on Monday
            //// and Saturday and repeat every 10 minutes for the following 11 hours
            //WeeklyTrigger wt = new WeeklyTrigger();
            //wt.StartBoundary = DateTime.Now.AddSeconds(5);
            //wt.DaysOfWeek = DaysOfTheWeek.AllDays;
            //wt.WeeksInterval = 3;
            //wt.Repetition.Duration = TimeSpan.FromHours(.5);
            //wt.Repetition.Interval = TimeSpan.FromMinutes(10);
            //td.Triggers.Add(wt);
            //wt.Enabled = false;

            //// Create an action that will launch Notepad whenever the trigger fires
            //td.Actions.Add("notepad.exe", "d:\\test.log");

            //// Register the task in the root folder of the local machine
            //TaskService.Instance.RootFolder.RegisterTaskDefinition("Test", td);









            //StreamWriter OurStream;
            //OurStream = File.CreateText(@"C:\temp\test.txt");
            //OurStream.WriteLine("This text is for task scheduler demo test file created on at: " + DateTime.Now);
            //OurStream.Close();
            //Console.WriteLine("File Created successfully!");

            //using (TaskService taskService = new TaskService())
            //{
            //    TaskDefinition taskDefinition = taskService.NewTask();
            //    // general settings
            //    taskDefinition.Principal.DisplayName = "Our Stream";
            //    taskDefinition.RegistrationInfo.Description = "Testing Task Scheduler";
            //    LogonTrigger login = new LogonTrigger();
            //    taskDefinition.Principal.UserId = string.Concat(Environment.UserDomainName, "\\", Environment.UserName);
            //    taskDefinition.Principal.LogonType = TaskLogonType.InteractiveToken;
            //    taskDefinition.Principal.RunLevel = TaskRunLevel.Highest;
            //    taskDefinition.Triggers.Add(login);

            //    // conditions
            //    taskDefinition.Settings.MultipleInstances = TaskInstancesPolicy.IgnoreNew;
            //    taskDefinition.Settings.DisallowStartIfOnBatteries = false;
            //    taskDefinition.Settings.StopIfGoingOnBatteries = false;
            //    taskDefinition.Settings.AllowHardTerminate = false;
            //    taskDefinition.Settings.StartWhenAvailable = false;
            //    taskDefinition.Settings.RunOnlyIfNetworkAvailable = false;
            //    taskDefinition.Settings.IdleSettings.StopOnIdleEnd = false;
            //    taskDefinition.Settings.IdleSettings.RestartOnIdle = false;

            //    // settings
            //    taskDefinition.Settings.AllowDemandStart = false;
            //    taskDefinition.Settings.Enabled = true;
            //    taskDefinition.Settings.Hidden = false;
            //    taskDefinition.Settings.RunOnlyIfIdle = false;
            //    taskDefinition.Settings.ExecutionTimeLimit = TimeSpan.Zero;
            //    taskDefinition.Settings.Priority = System.Diagnostics.ProcessPriorityClass.High;

            //    // action
            //    taskDefinition.Actions.Add(new ExecAction(@"D:\Github\c_cpp_csharp\TaskSchedulerTest\bin\Debug"));

            //    // registration
            //    taskService.RootFolder.RegisterTaskDefinition("Our Stream Task", taskDefinition);
            //}
        }
        private static List<string> GetFileList(String rootPath, List<String> fileList)
        {
            if (fileList == null)
            {
                return null;
            }

            var attr = File.GetAttributes(rootPath);
            Console.WriteLine("-------------------------");
            Console.WriteLine($"attr from File.GetAttributes(rootPath) : {attr}");

            // if an attribute is a Folder(Directory)
            if (attr == FileAttributes.Directory)
            {
                var dirInfo = new DirectoryInfo(rootPath);
                Console.WriteLine($"dirInfo from  new DirectoryInfo(rootPath) : {dirInfo}");
                int count = 1;
                Console.WriteLine($"dirInfo.GetDirectories().Length : {dirInfo.GetDirectories().Length}"); // directory(folder) numbers
                Console.WriteLine($"dirInfo.GetFiles().Length : {dirInfo.GetFiles().Length}"); // directory(folder) numbers
                foreach (var dir in dirInfo.GetDirectories()) // accessing every folder inside the rootPath [ when the number is 0 it means that there are no folders inside ]
                {
                    GetFileList(dir.FullName, fileList);
                    Console.WriteLine($"dir : {dir}");
                    count++;
                }
                Console.WriteLine($"count completed : count({count})");
                foreach (var file in dirInfo.GetFiles())
                {
                    GetFileList(file.FullName, fileList);
                }
            }
            //if the attribute is not a folder, it adds File(Archive) fullpath to FileList
            else
            {
                var fileInfo = new FileInfo(rootPath); // type is FileInfo
                fileList.Add(fileInfo.FullName); // FileInfo.FullName returns string 
            }
            return fileList;
        }
    }
}
