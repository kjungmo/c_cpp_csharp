using Microsoft.Win32.TaskScheduler;
using System;

namespace LogManager
{
    public static class TaskSchedulerManager
    {
        private const string TaskScheduleName = "CogAplex Log Management System";
        private static string _weekday = "monday";
        private static int _dayInMonth = 1;
        public static string Mode { get; set; }
        public static string RootPath { get; set; }
        public static int ZipLogDaysAfterLogged { get; set; }
        public static int DeleteLogDaysAfterLogged { get; set; }
        public static int ZipImgDaysAfterLogged { get; set; }
        public static int DeleteImgDaysAfterLogged { get; set; }
        public static int ZipCsvDaysAfterLogged { get; set; }
        public static int DeleteCsvDaysAfterLogged { get; set; }
        public static DateTime StartTime { get; set; }
        public static string Interval { get; set; }
        public static string Weekday { get { return _weekday; } private set { _weekday = value; } }
        public static int DayInMonth { get { return _dayInMonth; } private set { _dayInMonth = value; } }

        private static readonly Lazy<TaskService> _service = new Lazy<TaskService>(() => new TaskService());
        public static TaskService TService
        {
            get
            {
                return _service.Value;
            }
        }

        public static void FillTaskScheduleArgs(string[] arguments)
        {
            Mode = arguments[0];
            RootPath = arguments[1];
            ZipLogDaysAfterLogged = Convert.ToInt32(arguments[2]);
            DeleteLogDaysAfterLogged = Convert.ToInt32(arguments[3]);
            ZipImgDaysAfterLogged = Convert.ToInt32(arguments[4]);
            DeleteImgDaysAfterLogged = Convert.ToInt32(arguments[5]);
            ZipCsvDaysAfterLogged = Convert.ToInt32(arguments[6]);
            DeleteCsvDaysAfterLogged = Convert.ToInt32(arguments[7]);
            if (arguments.Length > 8)
            {
                if (DateTime.TryParseExact(arguments[8], "HH:mm", System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None, out DateTime convertedStartTime))
                {
                    StartTime = convertedStartTime;
                }
                Interval = arguments[9].ToLower();
                if (Interval == "weekly")
                {
                    if (arguments.Length == 11)
                    {
                        Weekday = arguments[10];
                    }
                }
                else if (Interval == "monthly")
                {
                    if (arguments.Length == 11)
                    {
                        string theDay = arguments[10];
                        DayInMonth = Convert.ToInt32(theDay);
                    }
                }
            }
        }

        public static void RegisterTaskScheduler(string exeFileDir)
        {
            switch (Interval)
            {
                case "daily":
                    AddDailyTaskSchedule(exeFileDir);
                    break;
                case "weekly":
                    AddWeeklyTaskSchedule(exeFileDir);
                    break;
                case "monthly":
                    AddMonthlyTaskSchedule(exeFileDir);
                    break;
            }

        }

        private static void AddTaskSchedule(ExecAction action, Trigger trigger)
        {
            TaskDefinition taskDefinition = TaskService.Instance.NewTask();
            taskDefinition.RegistrationInfo.Author = "cogaplex@cogaplex.com";
            taskDefinition.Triggers.Add(trigger);
            taskDefinition.Actions.Add(action);
            taskDefinition.RegistrationInfo.Description = $"{action.Arguments} {trigger.StartBoundary:HH:mm}";
            TaskService.Instance.RootFolder.RegisterTaskDefinition(TaskScheduleName, taskDefinition);
        }

        private static void AddDailyTaskSchedule(string exeFileDir)
        {
            AddTaskSchedule(CreateExeAction(exeFileDir),
                    CreateDailyTrigger());
        }

        private static void AddWeeklyTaskSchedule(string exeFileDir)
        {
            AddTaskSchedule(CreateExeAction(exeFileDir),
                    CreateWeeklyTrigger(Weekday));
        }

        private static void AddMonthlyTaskSchedule(string exeFileDir, int selectADayInMonth = 1)
        {
            AddTaskSchedule(CreateExeAction(exeFileDir),
                    CreateMonthlyTrigger(selectADayInMonth));
        }

        public static System.Threading.Tasks.Task RegisterTaskSchedulerAsync(string exeFileDir)
        {
            return System.Threading.Tasks.Task.Run(() =>
            {
                switch (Interval)
                {
                    case "daily":
                        System.Threading.Tasks.Task.Run(() => AddTaskSchedule(CreateExeAction(exeFileDir),
                        CreateDailyTrigger())
                    );
                        break;
                    case "weekly":
                        System.Threading.Tasks.Task.Run(() => AddTaskSchedule(CreateExeAction(exeFileDir),
                        CreateWeeklyTrigger(Weekday))
                    );
                        break;
                    case "monthly":
                        System.Threading.Tasks.Task.Run(() => AddTaskSchedule(CreateExeAction(exeFileDir),
                        CreateMonthlyTrigger(DayInMonth))
                    );
                        break;
                }
            }
            );
        }

        public static System.Threading.Tasks.Task AddDailyTaskScheduleAsync(string exeFileDir)
        {
            return System.Threading.Tasks.Task.Run(() => AddTaskSchedule(CreateExeAction(exeFileDir),
                    CreateDailyTrigger())
                );
        }

        public static System.Threading.Tasks.Task AddWeeklyTaskScheduleAsync(string exeFileDir)
        {
            return System.Threading.Tasks.Task.Run(() => AddTaskSchedule(CreateExeAction(exeFileDir),
                    CreateWeeklyTrigger(Weekday))
                );
        }

        public static System.Threading.Tasks.Task AddMonthlyTaskScheduleAsync(string exeFileDir)
        {
            return System.Threading.Tasks.Task.Run(() => AddTaskSchedule(CreateExeAction(exeFileDir),
                    CreateMonthlyTrigger(DayInMonth))
                );
        }

        private static ExecAction CreateExeAction(string exeFileDir)
        {
            ExecAction CogAplex = new ExecAction()
            {
                Path = exeFileDir,
                Arguments =
                $"{Mode} {RootPath} " +
                $"{ZipLogDaysAfterLogged} {DeleteLogDaysAfterLogged} " +
                $"{ZipImgDaysAfterLogged} {DeleteImgDaysAfterLogged} " +
                $"{ZipCsvDaysAfterLogged} {DeleteCsvDaysAfterLogged}",
            };
            return CogAplex;
        }

        private static DailyTrigger CreateDailyTrigger()
        {
            DailyTrigger dailyTrigger = new DailyTrigger
            {
                StartBoundary = StartTime,
                DaysInterval = 1
            };
            return dailyTrigger;
        }

        private static WeeklyTrigger CreateWeeklyTrigger(string weekday)
        {
            WeeklyTrigger weeklyTrigger = new WeeklyTrigger
            {
                StartBoundary = StartTime,
                DaysOfWeek = SelectWeekday(weekday),
                WeeksInterval = 1
            };
            return weeklyTrigger;
        }

        private static DaysOfTheWeek SelectWeekday(string weekday)
        {
            return (DaysOfTheWeek)Enum.Parse(typeof(DaysOfTheWeek), weekday, true);
        }

        private static MonthlyTrigger CreateMonthlyTrigger(int dayInMonth)
        {
            MonthlyTrigger monthlyTrigger = new MonthlyTrigger
            {
                StartBoundary = StartTime,
                MonthsOfYear = MonthsOfTheYear.AllMonths
            };

            if (dayInMonth == -1)
            {
                monthlyTrigger.DaysOfMonth = new int[] { };
                monthlyTrigger.RunOnLastDayOfMonth = true;
            }
            else
            {
                monthlyTrigger.DaysOfMonth = new int[] { dayInMonth };
                monthlyTrigger.RunOnLastDayOfMonth = false;
            }

            return monthlyTrigger;
        }

        public static void DeleteTaskSchedule()
        {
            TService.RootFolder.DeleteTask(TaskScheduleName, false);
        }

        public static string CheckAlreadyRegistered()
        {
            var scheduleTask = TService.FindTask(TaskScheduleName, false);
            return scheduleTask?.Definition.RegistrationInfo.Description ?? "";
        }

        public static System.Threading.Tasks.Task<string> CheckAlreadyRegisteredAsync()
        {
            return System.Threading.Tasks.Task.Run(() => CheckAlreadyRegistered());
        }
    }
}
