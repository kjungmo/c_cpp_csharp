using Microsoft.Win32.TaskScheduler;
using System;
using System.Collections.Generic;

namespace LogManagementSystem

{
    public class Scheduler
    {
        public string Mode { get; private set; }
        public string RootPath { get; private set; }
        public string ZipDaysAfterLogged { get; private set; }
        public string DeleteDaysAfterZip { get; private set; }
        public string ExecutionInterval { get; private set; }
        public DateTime StartExeFile { get; private set; }
        public int ExeRepititionInterval { get; private set; }
        public DaysOfTheWeek SelectedWeekday { get; private set; }
        public MonthsOfTheYear SelectedMonth { get; private set; }
        public int[] SelectedDayInMonth { get; private set; }
        public string DeleteLog { get; private set; }
        public string DeleteImg { get; private set; }
        public string DeleteCsv { get; private set; }

        public Scheduler()
        {

        }

        public Scheduler(
            string mode,
            string rootPath,
            string deleteLog,
            string deleteImg,
            string deleteCsv,
            DateTime? startExeFile = null
            )
        {
            Mode = mode;
            RootPath = rootPath;
            DeleteLog = deleteLog;
            DeleteImg = deleteImg;
            DeleteCsv = deleteCsv;
            ExecutionInterval = "daily";
            ExeRepititionInterval = 1;
            StartExeFile = startExeFile ?? DateTime.Today.AddDays(1);
        }

        public Scheduler(
            string mode,
            string rootPath, 
            int zipDaysAfterLogged, 
            int deleteDaysAfterZip, 
            string exeInterval,
            string weekday,
            int month,
            int dayInMonth,
            DateTime? startExeFile = null)
        {
            Mode = mode;
            RootPath = rootPath;
            ZipDaysAfterLogged = Convert.ToString(zipDaysAfterLogged);
            DeleteDaysAfterZip = Convert.ToString(deleteDaysAfterZip);
            ExecutionInterval = exeInterval;
            SelectedWeekday = SelectWeekday(weekday);
            SelectedMonth = SelectMonth(month);
            SelectedDayInMonth = SelectDayInMonth(dayInMonth);
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
            ExecAction CogAplex = new ExecAction();
            if (Mode == "zip")
            {
                CogAplex.Path = System.Reflection.Assembly.GetExecutingAssembly().Location;
                CogAplex.Arguments = $"{Mode} {RootPath} {ZipDaysAfterLogged} {DeleteDaysAfterZip}";
            }
            else if (Mode == "manage")
            {
                CogAplex.Path = System.Reflection.Assembly.GetExecutingAssembly().Location;
                CogAplex.Arguments = $"{Mode} {RootPath} {DeleteLog} {DeleteImg} {DeleteCsv}";
            }
            return CogAplex;
        }

        public Trigger CreateTrigger()
        {
            switch (ExecutionInterval.ToLower())
            {
                case "daily":
                    return CreateDailyTrigger();

                case "weekly":
                    return CreateWeeklyTrigger();

                case "monthly":
                    return CreateMonthlyTrigger();
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

        private WeeklyTrigger CreateWeeklyTrigger()
        {
            WeeklyTrigger weeklyTrigger = new WeeklyTrigger
            {
                StartBoundary = StartExeFile,
                DaysOfWeek = SelectedWeekday,
                WeeksInterval = (short)ExeRepititionInterval
            };
            return weeklyTrigger;
        }

        public DaysOfTheWeek SelectWeekday(string weekday)
        {
            switch (weekday.ToLower())
            {
                case "tuesday":
                    return DaysOfTheWeek.Tuesday;
                case "wednesday":
                    return DaysOfTheWeek.Wednesday;
                case "thursday":
                    return DaysOfTheWeek.Thursday;
                case "friday":
                    return DaysOfTheWeek.Friday;
                case "saturday":
                    return DaysOfTheWeek.Saturday;
                case "sunday":
                    return DaysOfTheWeek.Sunday;
                case "Allday":
                    return DaysOfTheWeek.AllDays;
                default:
                    return DaysOfTheWeek.Monday;
            }
        }

        private MonthlyTrigger CreateMonthlyTrigger()
        {
            MonthlyTrigger monthlyTrigger = new MonthlyTrigger
            {
                StartBoundary = StartExeFile,
                DaysOfMonth = SelectedDayInMonth,
                MonthsOfYear = SelectedMonth,
                RunOnLastDayOfMonth = false
            };
            return monthlyTrigger;
        }

        public MonthsOfTheYear SelectMonth(int month)
        {
            switch (month)
            {
                case 1:
                    return MonthsOfTheYear.January;
                case 2:
                    return MonthsOfTheYear.February;
                case 3:
                    return MonthsOfTheYear.March;
                case 4:
                    return MonthsOfTheYear.April;
                case 5:
                    return MonthsOfTheYear.May;
                case 6:
                    return MonthsOfTheYear.June;
                case 7:
                    return MonthsOfTheYear.July;
                case 8:
                    return MonthsOfTheYear.August;
                case 9:
                    return MonthsOfTheYear.September;
                case 10:
                    return MonthsOfTheYear.October;
                case 11:
                    return MonthsOfTheYear.November;
                case 12:
                    return MonthsOfTheYear.December;
                default:
                    return MonthsOfTheYear.AllMonths;
            }
        }

        private int[] SelectDayInMonth(int dayInMonth = 1)
        {
            List<int> dayToSet = new List<int>();
            if (dayInMonth > 0 && dayInMonth <= 31)
            {
                dayToSet.Add(dayInMonth);
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
    }
}
