using Microsoft.Win32.TaskScheduler;
using System;

namespace LogManager
{
    public class TaskSchedulerManager : IDisposable
    {
        private bool isDispose = false;
        private const string TaskScheduleName = "CogAplex Log Management System";
        private string _weekDay = "monday";
        private int _dayInMonth = 1;
        public string Mode { get; set; }
        public string RootPath { get; set; }
        public int ZipLogDaysAfterLogged { get; set; }
        public int DeleteLogDaysAfterLogged { get; set; }
        public int ZipImgDaysAfterLogged { get; set; }
        public int DeleteImgDaysAfterLogged { get; set; }
        public int ZipCsvDaysAfterLogged { get; set; }
        public int DeleteCsvDaysAfterLogged { get; set; }
        public DateTime StartTime { get; set; }
        public string Interval { get; set; }
        public string WeekDay { get { return _weekDay; } private set { _weekDay = value; } }
        public int DayInMonth { get { return _dayInMonth; } private set { _dayInMonth = value; } }

        private readonly Lazy<TaskService> _service = new Lazy<TaskService>(() => new TaskService());
        public TaskService TService
        {
            get
            {
                return _service.Value;
            }
        }

        public TaskSchedulerManager(string[] cLArguments)
        {
            if (!isDispose)
            {
                Dispose();
            }
            Mode = cLArguments[0].ToLower();
            RootPath = cLArguments[1];
            ZipLogDaysAfterLogged = Convert.ToInt32(cLArguments[2]);
            DeleteLogDaysAfterLogged = Convert.ToInt32(cLArguments[3]);
            ZipImgDaysAfterLogged = Convert.ToInt32(cLArguments[4]);
            DeleteImgDaysAfterLogged = Convert.ToInt32(cLArguments[5]);
            ZipCsvDaysAfterLogged = Convert.ToInt32(cLArguments[6]);
            DeleteCsvDaysAfterLogged = Convert.ToInt32(cLArguments[7]);
            if (DateTime.TryParseExact(cLArguments[8], "HH:mm", System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None, out DateTime convertedStartTime))
            {
                StartTime = convertedStartTime;
            }
            Interval = cLArguments[9].ToLower();
            if (Interval == "weekly")
            {
                if (cLArguments.Length == 11)
                {
                    WeekDay = cLArguments[10];
                }
            }
            else if (Interval == "monthly")
            {
                if (cLArguments.Length == 11)
                {
                    string theDay = cLArguments[10];
                    DayInMonth = Convert.ToInt32(theDay);
                }
            }
        }

        public void RegisterTaskScheduler(string exeFileDir)
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

        private void AddTaskSchedule(ExecAction action, Trigger trigger)
        {
            TaskDefinition taskDefinition = TaskService.Instance.NewTask();
            taskDefinition.RegistrationInfo.Author = "cogaplex@cogaplex.com";
            taskDefinition.Triggers.Add(trigger);
            taskDefinition.Actions.Add(action);
            taskDefinition.RegistrationInfo.Description = $"{action.Arguments} {trigger.StartBoundary:HH:mm}";
            TaskService.Instance.RootFolder.RegisterTaskDefinition(TaskScheduleName, taskDefinition);
        }

        private void AddDailyTaskSchedule(string exeFileDir)
        {
            AddTaskSchedule(CreateExeAction(exeFileDir),
                    CreateDailyTrigger());
        }

        private void AddWeeklyTaskSchedule(string exeFileDir)
        {
            AddTaskSchedule(CreateExeAction(exeFileDir),
                    CreateWeeklyTrigger(WeekDay));
        }

        private void AddMonthlyTaskSchedule(string exeFileDir, int selectADayInMonth = 1)
        {
            AddTaskSchedule(CreateExeAction(exeFileDir),
                    CreateMonthlyTrigger(selectADayInMonth));
        }

        public System.Threading.Tasks.Task RegisterTaskSchedulerAsync(string exeFileDir)
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
                        CreateWeeklyTrigger(WeekDay))
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

        public System.Threading.Tasks.Task AddDailyTaskScheduleAsync(string exeFileDir)
        {
            return System.Threading.Tasks.Task.Run(() => AddTaskSchedule(CreateExeAction(exeFileDir),
                    CreateDailyTrigger())
                );
        }

        public System.Threading.Tasks.Task AddWeeklyTaskScheduleAsync(string exeFileDir)
        {
            return System.Threading.Tasks.Task.Run(() => AddTaskSchedule(CreateExeAction(exeFileDir),
                    CreateWeeklyTrigger(WeekDay))
                );
        }

        public System.Threading.Tasks.Task AddMonthlyTaskScheduleAsync(string exeFileDir)
        {
            return System.Threading.Tasks.Task.Run(() => AddTaskSchedule(CreateExeAction(exeFileDir),
                    CreateMonthlyTrigger(DayInMonth))
                );
        }

        private ExecAction CreateExeAction(string exeFileDir)
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

        private DailyTrigger CreateDailyTrigger()
        {
            DailyTrigger dailyTrigger = new DailyTrigger
            {
                StartBoundary = StartTime,
                DaysInterval = 1
            };
            return dailyTrigger;
        }

        private WeeklyTrigger CreateWeeklyTrigger(string weekday)
        {
            WeeklyTrigger weeklyTrigger = new WeeklyTrigger
            {
                StartBoundary = StartTime,
                DaysOfWeek = SelectWeekday(weekday),
                WeeksInterval = 1
            };
            return weeklyTrigger;
        }

        private DaysOfTheWeek SelectWeekday(string weekday)
        {
            return (DaysOfTheWeek)Enum.Parse(typeof(DaysOfTheWeek), weekday, true);
        }

        private MonthlyTrigger CreateMonthlyTrigger(int dayInMonth)
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

        public void DeleteTaskSchedule()
        {
            TService.RootFolder.DeleteTask(TaskScheduleName, false);
        }

        public string CheckAlreadyRegistered()
        {
            var scheduleTask = TService.FindTask(TaskScheduleName, false);
            return scheduleTask?.Definition.RegistrationInfo.Description ?? "";
        }

        public System.Threading.Tasks.Task<string> CheckAlreadyRegisteredAsync()
        {
            return System.Threading.Tasks.Task.Run(() => CheckAlreadyRegistered());
        }

        public void Dispose()
        {
            isDispose = true;
            GC.SuppressFinalize(this);
        }
    }
}
