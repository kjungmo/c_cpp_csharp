using System;
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
            string date1 = DateTime.Today.AddDays(0).ToString();
            string date2 = DateTime.Today.AddDays(-7).ToString();
            string date3 = DateTime.Today.AddDays(-14).ToString();
            Console.WriteLine($"date1 : {date1}");
            Console.WriteLine($"date2 : {date2}");
            Console.WriteLine($"date3 : {date3}");
            Console.WriteLine($"date1.compared to date1 : {date1.CompareTo(date1)}");
            Console.WriteLine($"date1.compared to date2 : {date1.CompareTo(date2)}");
            Console.WriteLine($"date1.compared to date3 : {date1.CompareTo(date3)}");
            Console.WriteLine($"date2.compared to date1 : {date2.CompareTo(date1)}");
            Console.WriteLine($"date2.compared to date2 : {date2.CompareTo(date2)}");
            Console.WriteLine($"date2.compared to date3 : {date2.CompareTo(date3)}");
            Console.WriteLine($"date3.compared to date1 : {date3.CompareTo(date1)}");
            Console.WriteLine($"date3.compared to date2 : {date3.CompareTo(date2)}");
            Console.WriteLine($"date3.compared to date3 : {date3.CompareTo(date3)}");

            Console.ReadLine();


            //// Create a new task definition for the local machine and assign properties
            //TaskDefinition td = TaskService.Instance.NewTask();
            //td.RegistrationInfo.Description = "Does something";

            //// Add a trigger that, starting tomorrow, will fire every other week on Monday
            //// and Saturday and repeat every 10 minutes for the following 11 hours
            //WeeklyTrigger wt = new WeeklyTrigger();
            //wt.StartBoundary = DateTime.Now.AddSeconds(5);
            //wt.DaysOfWeek = DaysOfTheWeek.AllDays;
            //wt.WeeksInterval = 3;
            //wt.Repetition.Duration = TimeSpan.FromHours(.5);
            //wt.Repetition.Interval = TimeSpan.FromSeconds(20);
            //td.Triggers.Add(wt);

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
    }
}
