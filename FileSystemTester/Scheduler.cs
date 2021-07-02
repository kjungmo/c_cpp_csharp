using System;
using System.Collections.Generic;
using Microsoft.Win32.TaskScheduler;

namespace ScheduleRegister
{
    public class Scheduler
    {
        public static string CogAplexLogManager { get { return "CogAplex Log Management System"; } }
        public string Mode { get; private set; }
        public string RootPath { get; private set; }
        public string ZipDaysAfterLogged { get; private set; }
        public string DeleteDaysAfterZip { get; private set; }
        public string ExecutionInterval { get; private set; }
        public DateTime StartExeFile { get; private set; }
        public short ExeRepititionInterval { get; private set; }
        public DaysOfTheWeek SelectedWeekday { get; private set; }
        public MonthsOfTheYear SelectedMonth { get; private set; }
        public int[] SelectedDayInMonth { get; private set; }
        public string DeleteLog { get; private set; }
        public string DeleteImg { get; private set; }
        public string DeleteCsv { get; private set; }
        public string ExeFilePath { get; private set; }

        public Scheduler()
        {
        }

        public Scheduler(
            string mode,
            string rootPath,
            string deleteLog,
            string deleteImg,
            string deleteCsv,
            DateTime? startExeFile = null,
            string exeFilePath = null,
            string exeInterval = "daily",
            short exeRepititionInterval = 1
            )
        {
            Mode = mode;
            RootPath = rootPath;
            DeleteLog = deleteLog;
            DeleteImg = deleteImg;
            DeleteCsv = deleteCsv;
            StartExeFile = startExeFile ?? DateTime.Today.AddDays(1);
            ExeFilePath = exeFilePath ?? System.Reflection.Assembly.GetExecutingAssembly().Location;
            ExecutionInterval = exeInterval;
            ExeRepititionInterval = exeRepititionInterval;
        }

        public Scheduler(
            string mode,
            string rootPath,
            int zipDaysAfterLogged,
            int deleteDaysAfterZip,
            string exeInterval = "daily",
            string weekday = "monday",
            int month = 1,
            int dayInMonth = 1,
            DateTime? startExeFile = null,
            string exeFilePath = null,
            short exeRepititionInterval = 1
            )
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
            ExeFilePath = exeFilePath ?? System.Reflection.Assembly.GetExecutingAssembly().Location;
            ExeRepititionInterval = exeRepititionInterval;
        }

        public void AddTaskSchedule()
        {
            TaskDefinition taskDefinition = TaskService.Instance.NewTask();
            taskDefinition.RegistrationInfo.Author = "cogaplex@cogaplex.com";
            taskDefinition.Triggers.Add(CreateTrigger());
            taskDefinition.Actions.Add(CreateExeAction());
            taskDefinition.RegistrationInfo.Description = CreateExeAction().Arguments;
            taskDefinition.RegistrationInfo.Description += " ";
            taskDefinition.RegistrationInfo.Description += StartExeFile.ToString("HH:mm");
            TaskService.Instance.RootFolder.RegisterTaskDefinition(CogAplexLogManager, taskDefinition);
        }

        private ExecAction CreateExeAction()
        {
            ExecAction CogAplex = new ExecAction();
            CogAplex.Path = ExeFilePath;
            if (Mode == "zip")
            {
                CogAplex.Arguments = $"{Mode} {RootPath} {ZipDaysAfterLogged} {DeleteDaysAfterZip}";
            }
            else if (Mode == "manage")
            {
                CogAplex.Arguments = $"{Mode} {RootPath} {DeleteLog} {DeleteImg} {DeleteCsv}";
            }
            return CogAplex;
        }

        private Trigger CreateTrigger()
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

        public static void DeleteTaskSchedule()
        {
            using (TaskService service = new TaskService())
            {
                if (service.FindTask(CogAplexLogManager, false).IsActive)
                {
                    service.RootFolder.DeleteTask(CogAplexLogManager, false);
                }
            }
        }

        public static bool CheckAlreadyRegistered()
        {
            using (TaskService service = new TaskService())
            {
                Task task = service.FindTask(CogAplexLogManager, false);
                if (task == null)
                {
                    return false;
                }
                return true;
            }
        }

        public static string GetRegisteredValues()
        {
            string desc;
            using (TaskService service = new TaskService())
            {
                desc = service.FindTask(CogAplexLogManager, false).Definition.RegistrationInfo.Description;
                if (string.IsNullOrEmpty(desc))
                {
                    return "";
                }
            }
            return desc;
        }
    }
}
