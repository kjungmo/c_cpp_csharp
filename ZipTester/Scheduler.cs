using Microsoft.Win32.TaskScheduler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZipTester
{
	public static class Scheduler
	{
        private static void AddTaskSchedule(string executionPath, string arguments, string timeInterval, string stopFlag = "true")
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
                    mTrigger.DaysOfMonth = new int[] { 1 };
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
            CogAplex.Path = executionPath;//"zip"; // where exe is located ( full address ) 
            CogAplex.Arguments = arguments;  // arguments
            CogAplex.WorkingDirectory = ""; // directory of exe OR dir of files that exe uses 

            taskDefinition.Actions.Add(CogAplex);
            // Register the task in the root folder of the local machine
            TaskService.Instance.RootFolder.RegisterTaskDefinition("CogAplex Log Management System", taskDefinition);
        }
    }
}
