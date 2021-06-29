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
        public DateTime StartExeFile { get; private set; }
        public int ExeRepititionInterval { get; private set; }

        public Scheduler()
        {

        }

        public Scheduler(
            string rootPath, 
            string zipDaysAfterLogged, 
            string deleteDaysAfterZip, 
            string exeInterval, 
            DateTime? startExeFile = null)
        {
            RootPath = rootPath;
            ZipDaysAfterLogged = zipDaysAfterLogged;
            DeleteDaysAfterZip = deleteDaysAfterZip;
            ExecutionInterval = exeInterval;
            StartExeFile = startExeFile ?? DateTime.Today.AddDays(1);
            ExeRepititionInterval = 1;
        }

        public void AddTaskSchedule(Trigger startTrigger)
        {
            TaskDefinition taskDefinition = TaskService.Instance.NewTask();
            taskDefinition.RegistrationInfo.Author = "cogaplex@cogaplex.com";
            taskDefinition.RegistrationInfo.Description = "Management of daily .log and captured image files.";
            taskDefinition.Triggers.Add(startTrigger);
            taskDefinition.Actions.Add(CreateExeAction());
            TaskService.Instance.RootFolder.RegisterTaskDefinition("CogAplex Log Management System", taskDefinition);
        }

        private ExecAction CreateExeAction()
        {
            ExecAction CogAplex = new ExecAction
            {
                Path = System.Reflection.Assembly.GetExecutingAssembly().Location,
                Arguments = $"zip {RootPath} {ZipDaysAfterLogged} {DeleteDaysAfterZip}"
            };
            return CogAplex;
        }

        public DailyTrigger SelectTrigger()
        {
            if (ExecutionInterval.ToLower() == "daily")
            {
                return CreateDailyTrigger();
            }
            return null;
        }

        public WeeklyTrigger SelectTrigger(
            DaysOfTheWeek dayofWeek = DaysOfTheWeek.Monday)
        {
            if (ExecutionInterval.ToLower() == "weekly")
            {
                return CreateWeeklyTrigger(dayofWeek);
            }
            return null;
        }

        public MonthlyTrigger SelectTrigger(
            int dayOfMonth = 1, 
            MonthsOfTheYear monthsOfTheYear = MonthsOfTheYear.AllMonths)
        {
            if (ExecutionInterval.ToLower() == "monthly")
            {
                return CreateMonthlyTrigger(dayOfMonth, monthsOfTheYear);
            }
            return null;
        }

        public Trigger CreateTrigger(
            DaysOfTheWeek daysOfTheWeek = DaysOfTheWeek.Monday,
            int dayOfMonth = 1, 
            MonthsOfTheYear monthsOfTheYear = MonthsOfTheYear.AllMonths
            )
        {
            switch (ExecutionInterval.ToLower())
            {
                case "daily":
                    return CreateDailyTrigger();

                case "weekly":
                    return CreateWeeklyTrigger(daysOfTheWeek);

                case "monthly":
                    return CreateMonthlyTrigger(dayOfMonth, monthsOfTheYear);
                default:
                    return CreateDailyTrigger();
            }
        }

        private DailyTrigger CreateDailyTrigger()
        {
            DailyTrigger dailyTrigger = new DailyTrigger
            {
                StartBoundary = StartExeFile,
                DaysInterval = (short)ExeRepititionInterval
            };
            return dailyTrigger;
        }

        private WeeklyTrigger CreateWeeklyTrigger(
            DaysOfTheWeek daysOfTheWeek = DaysOfTheWeek.Monday)
        {
            WeeklyTrigger weeklyTrigger = new WeeklyTrigger
            {
                StartBoundary = StartExeFile,
                DaysOfWeek = daysOfTheWeek,
                WeeksInterval = (short)ExeRepititionInterval
            };
            return weeklyTrigger;
        }

        private MonthlyTrigger CreateMonthlyTrigger(
            int day = 1, 
            MonthsOfTheYear monthsOfTheYear = MonthsOfTheYear.AllMonths)
        {
            MonthlyTrigger monthlyTrigger = new MonthlyTrigger
            {
                StartBoundary = StartExeFile,
                DaysOfMonth = setDayOfMonth(day),
                MonthsOfYear = monthsOfTheYear,
                RunOnLastDayOfMonth = false
            };
            return monthlyTrigger;
        }
        private int[] setDayOfMonth(int day = 1)
        {
            List<int> dayToSet = new List<int>();
            if (day > 0 && day <= 31)
            {
                dayToSet.Add(day);
            }
            return dayToSet.ToArray();
        }

        public void DeleteTaskSchedule()
        {
            using (TaskService service = new TaskService())
            {
                foreach (var item in service.FindAllTasks(new System.Text.RegularExpressions.Regex("CogAplex")))
                {
                    service.RootFolder.DeleteTask(item.Name, false);
                }
            }
        }

        public void WeekdaySelector(string weekday)
        {

        }

        public void MonthSelector(string month)
        {

        }
    }
}
