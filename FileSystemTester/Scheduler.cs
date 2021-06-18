using Microsoft.Win32.TaskScheduler;
using System;

namespace FileSystemTester
{
    public static class Scheduler
    {
        public enum ExeInterval
        {
            DAILY,
            WEEKLY,
            MONTHLY,
        }

        public static void AddTaskSchedule(string selectedInterval, string arguments, bool stopFlag = true)
        {
            TaskDefinition taskDefinition = TaskService.Instance.NewTask();
            taskDefinition.RegistrationInfo.Author = "cogaplex@cogaplex.com";
            taskDefinition.RegistrationInfo.Description = "Management of daily .log and captured image files.";

            ExeInterval interval;
            if (!Enum.TryParse<ExeInterval>(selectedInterval.ToUpper(), out interval))
            {
                return;
            }
            switch (interval)
            {
                // [DAILY] - StartBoundary(DateTime), DaysInterval(int), stopFlag(bool)
                case ExeInterval.DAILY: // 
                    taskDefinition.Triggers.Add(CreateDailyTrigger(stopFlag));
                    break;

                // [WEEKLY] - StartBoundray(DateTime), DaysOfWeek(DaysOfTheWeek), WeeksInterval(int), stopFlag(bool)
                case ExeInterval.WEEKLY:
                    taskDefinition.Triggers.Add(CreateWeeklyTrigger(stopFlag));
                    break;

                // [MONTHLY] - StartBoundary(DateTime), DaysOfWeek(DaysOfTheWeek), MonthsOfYear(MonthsOfTheYear), stopFlag(bool)
                case ExeInterval.MONTHLY: // Type : Day
                    // starts tomorrow, triggers on the first day of every month, doesn't run on the last day of the month
                    
                    taskDefinition.Triggers.Add(CreateMonthlyTrigger(stopFlag));
                    
                    //// Type : Weekday
                    ////OR starts tomorrow, triggers on the first week's monday of every month
                    //taskDefinition.Triggers.Add(CreateMonthlyTrigger2());
                    break;
            }
            
            taskDefinition.Actions.Add(CreateExeAction(arguments));
            // Register the task in the root folder of the local machine
            TaskService.Instance.RootFolder.RegisterTaskDefinition("CogAplex Log Management System", taskDefinition);
        }

        public static string CreateSchedulerArguments(string zipDate, string deletionDate)
        {
            string arguments = zipDate;
            arguments += " ";
            arguments += deletionDate;
            return arguments;
        }

        public static ExecAction CreateExeAction(string arguments)
        {
            ExecAction CogAplex = new ExecAction();
            CogAplex.Path = System.Reflection.Assembly.GetExecutingAssembly().Location;
            CogAplex.Arguments = arguments;
            return CogAplex;
        }

        public static DailyTrigger CreateDailyTrigger(bool stopFlag = true)
        {
            DailyTrigger dailyTrigger = new DailyTrigger();
            dailyTrigger.StartBoundary = DateTime.Today.AddDays(1);
            dailyTrigger.DaysInterval = 1;
            if (!stopFlag)
            {
                dailyTrigger.Enabled = false;
            }
            return dailyTrigger;
        }

        public static WeeklyTrigger CreateWeeklyTrigger(bool stopFlag = true)
        {
            WeeklyTrigger weeklyTrigger = new WeeklyTrigger();
            weeklyTrigger.StartBoundary = DateTime.Today.AddDays(1);
            weeklyTrigger.DaysOfWeek = DaysOfTheWeek.Monday;
            weeklyTrigger.WeeksInterval = 1;
            if (!stopFlag)
            {
                weeklyTrigger.Enabled = false;
            }
            return weeklyTrigger;
        }

        public static MonthlyTrigger CreateMonthlyTrigger(bool stopFlag = true)
        {
            MonthlyTrigger monthlyTrigger = new MonthlyTrigger();
            monthlyTrigger.StartBoundary = DateTime.Today.AddDays(1);
            monthlyTrigger.DaysOfMonth = new int[] { 1 };
            monthlyTrigger.MonthsOfYear = MonthsOfTheYear.AllMonths;
            monthlyTrigger.RunOnLastDayOfMonth = false; // V2 only
            if (!stopFlag)
            {
                monthlyTrigger.Enabled = false;
            }
            return monthlyTrigger;
        }

        public static MonthlyDOWTrigger CreateMonthlyTrigger2(bool stopFlag = true)
        {
            MonthlyDOWTrigger monthlyTrigger = new MonthlyDOWTrigger();
            monthlyTrigger.StartBoundary = DateTime.Now.AddSeconds(2);
            monthlyTrigger.DaysOfWeek = DaysOfTheWeek.Monday;
            monthlyTrigger.MonthsOfYear = MonthsOfTheYear.AllMonths;
            monthlyTrigger.WeeksOfMonth = WhichWeek.FirstWeek;
            if (!stopFlag)
            {
                monthlyTrigger.Enabled = false;
            }

            return monthlyTrigger;
        }

        public static void DeleteTaskSchedule(string deleteFlag)
        {
            if (deleteFlag.ToLower() == "true")
            {
                TaskService.Instance.RootFolder.DeleteTask("CogAplex Log Management System");
            }
        }
    }
}
