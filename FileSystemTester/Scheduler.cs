using Microsoft.Win32.TaskScheduler;
using System;
using System.Collections.Generic;

namespace LogManagementSystem

{
    public class Scheduler
    {
        public string RootPath { get; private set; }
        public string ZipDaysAfterLogged { get; private set; }
        public string DeleteDaysAfterZip { get; private set; }
        public string ExecutionInterval { get; private set; }

        public Scheduler(string rootPath, string zipDaysAfterLogged, string deleteDaysAfterZip, string exeInterval)
        {
            RootPath = rootPath;
            ZipDaysAfterLogged = zipDaysAfterLogged;
            DeleteDaysAfterZip = deleteDaysAfterZip;
            ExecutionInterval = exeInterval;
        }

        public void AddTaskSchedule(Trigger startTrigger)
        {
            TaskDefinition taskDefinition = TaskService.Instance.NewTask();
            taskDefinition.RegistrationInfo.Author = "cogaplex@cogaplex.com";
            taskDefinition.RegistrationInfo.Description = "Management of daily .log and captured image files.";
            taskDefinition.Triggers.Add(startTrigger);
            taskDefinition.Actions.Add(CreateExeAction());
            // Register the task in the root folder of the local machine
            TaskService.Instance.RootFolder.RegisterTaskDefinition("CogAplex Log Management System", taskDefinition);
        }

        private ExecAction CreateExeAction()
        {
            ExecAction CogAplex = new ExecAction();
            CogAplex.Path = System.Reflection.Assembly.GetExecutingAssembly().Location;
            CogAplex.Arguments = $"zip {RootPath} {ZipDaysAfterLogged} {DeleteDaysAfterZip}";
            return CogAplex;
        }

        public DailyTrigger SelectTrigger(string interval, 
            DateTime? startingBoundary = null)
        {
            if (interval.ToLower() == "daily")
            {
                return CreateDailyTrigger(startingBoundary);
            }
            return null;
        }

        public WeeklyTrigger SelectTrigger(string interval, 
            DateTime? startingBoundary = null,
            DaysOfTheWeek dayofWeek = DaysOfTheWeek.Monday)
        {
            if (interval.ToLower() == "weekly")
            {
                return CreateWeeklyTrigger(startingBoundary, dayofWeek);
            }
            return null;
        }

        public MonthlyTrigger SelectTrigger(string interval, 
            DateTime? startingBoundary = null, 
            int day = 1, 
            MonthsOfTheYear monthsOfTheYear = MonthsOfTheYear.AllMonths)
        {
            if (interval.ToLower() == "monthly")
            {
                return CreateMonthlyTrigger(startingBoundary, day, monthsOfTheYear);
            }
            return null;
        }

        public MonthlyDOWTrigger SelectTrigger(string interval, 
            DateTime? startingBoundary = null, 
            DaysOfTheWeek daysOfTheWeek = DaysOfTheWeek.Monday, 
            MonthsOfTheYear monthsOfTheYear = MonthsOfTheYear.AllMonths,
            WhichWeek whichweek = WhichWeek.AllWeeks)
        {
            if (interval.ToLower() == "monthly2")
            {
                return CreateMonthlyTrigger2(startingBoundary, daysOfTheWeek, monthsOfTheYear, whichweek);
            }
            return null;
        }

        public Trigger SelectTrigger(DateTime? startingBoundary = null, int daysInterval = 1,
            DaysOfTheWeek dayofWeek = DaysOfTheWeek.Monday, int weeksInterval = 1,
            int day = 1, MonthsOfTheYear monthsOfTheYear = MonthsOfTheYear.AllMonths,
            DaysOfTheWeek daysOfTheWeek = DaysOfTheWeek.Monday,
            WhichWeek whichweek = WhichWeek.AllWeeks
            )
        {
            switch (ExecutionInterval.ToLower()) // enum말고 string 으로 switch할 것.
            {
                case "daily":
                    return CreateDailyTrigger(startingBoundary);

                case "weekly":
                    return CreateWeeklyTrigger(startingBoundary, dayofWeek);

                case "monthly":
                    return CreateMonthlyTrigger(startingBoundary, day, monthsOfTheYear);
                //return CreateMonthlyTrigger2(startingBoundary, daysOfTheWeek, monthsOfTheYear, whichweek);
                default:
                    return CreateDailyTrigger(startingBoundary);
            }
        }

        private DailyTrigger CreateDailyTrigger(
            DateTime? startBoundary = null)
        {
            // [DAILY] - StartBoundary(DateTime), DaysInterval(int), stopFlag(bool)
            DailyTrigger dailyTrigger = new DailyTrigger();
            dailyTrigger.StartBoundary = startBoundary ?? DateTime.Today.AddDays(1);
            dailyTrigger.DaysInterval = 1;
            return dailyTrigger;
        }

        private WeeklyTrigger CreateWeeklyTrigger(
            DateTime? startBoundary = null, 
            DaysOfTheWeek dayofWeek = DaysOfTheWeek.Monday)
        {
            // [WEEKLY] - StartBoundray(DateTime), DaysOfWeek(DaysOfTheWeek), WeeksInterval(int), stopFlag(bool)
            WeeklyTrigger weeklyTrigger = new WeeklyTrigger();
            weeklyTrigger.StartBoundary = startBoundary ?? DateTime.Today.AddDays(1);
            weeklyTrigger.DaysOfWeek = DaysOfTheWeek.Monday;
            weeklyTrigger.WeeksInterval = 1;
            return weeklyTrigger;
        }

        private MonthlyTrigger CreateMonthlyTrigger(
            DateTime? startBoundary = null, 
            int day = 1, 
            MonthsOfTheYear monthsOfTheYear = MonthsOfTheYear.AllMonths)
        {
            // [MONTHLY] - StartBoundary(DateTime), DaysOfWeek(DaysOfTheWeek), MonthsOfYear(MonthsOfTheYear), stopFlag(bool)
            // Type : Day
            // starts tomorrow, triggers on the first day of every month, doesn't run on the last day of the month

            MonthlyTrigger monthlyTrigger = new MonthlyTrigger();
            monthlyTrigger.StartBoundary = startBoundary ?? DateTime.Today.AddDays(1);
            monthlyTrigger.DaysOfMonth = setDaysToMonthlyTrigger(day);
            monthlyTrigger.MonthsOfYear = monthsOfTheYear;
            monthlyTrigger.RunOnLastDayOfMonth = false; // V2 only
            return monthlyTrigger;
        }
        private int[] setDaysToMonthlyTrigger(int day = 1)
        {
            List<int> dayToSet = new List<int>();
            if (day > 0 && day <= 31)
            {
                dayToSet.Add(day);
            }
            return dayToSet.ToArray();
        }


        private MonthlyDOWTrigger CreateMonthlyTrigger2(
            DateTime? startBoundary = null,
            DaysOfTheWeek daysOfTheWeek = DaysOfTheWeek.Monday, 
            MonthsOfTheYear monthsOfTheYear = MonthsOfTheYear.AllMonths,
            WhichWeek whichweek = WhichWeek.AllWeeks)
        {
            // [MONTHLY] - StartBoundary(DateTime), DaysOfWeek(DaysOfTheWeek), MonthsOfYear(MonthsOfTheYear), stopFlag(bool)
            //// Type : Weekday
            ////OR starts tomorrow, triggers on the first week's monday of every month

            MonthlyDOWTrigger monthlyTrigger = new MonthlyDOWTrigger();
            monthlyTrigger.StartBoundary = startBoundary ?? DateTime.Today.AddDays(1);
            monthlyTrigger.DaysOfWeek = daysOfTheWeek;
            monthlyTrigger.MonthsOfYear = monthsOfTheYear;
            monthlyTrigger.WeeksOfMonth = whichweek;
            return monthlyTrigger;
        }

        public void DeleteTaskSchedule(string deleteFlag)
        {
            using (TaskService service = new TaskService())
            {
                foreach (var item in service.FindAllTasks(new System.Text.RegularExpressions.Regex("CogAplex")))
                {
                    service.RootFolder.DeleteTask(item.Name, false);
                }
            }
        }
    }
}
