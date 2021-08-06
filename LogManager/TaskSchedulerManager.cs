using Microsoft.Win32.TaskScheduler;
using System;

namespace LogManager
{
    public static class TaskSchedulerManager
    {
        private const string TaskScheduleName = "CogAplex Log Management System";
        private static readonly Lazy<TaskService> _service = new Lazy<TaskService>(() => new TaskService());
        public static TaskService TService
        {
            get
            {
                return _service.Value;
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

        public static void AddDailyTaskSchedule(string mode, string exeFileDir, string rootPath,
            int zipLogDaysAfterLogged, int deleteLogDaysAfterLogged,
            int zipImgDaysAfterLogged, int deleteImgDaysAfterLogged,
            int zipCsvDaysAfterLogged, int deleteCsvDaysAfterLogged,
            DateTime startTime)
        {
            AddTaskSchedule(CreateExeAction(mode, exeFileDir, rootPath,
                    zipLogDaysAfterLogged, deleteLogDaysAfterLogged,
                    zipImgDaysAfterLogged, deleteImgDaysAfterLogged,
                    zipCsvDaysAfterLogged, deleteCsvDaysAfterLogged),
                    CreateDailyTrigger(startTime));
        }

        public static void AddWeeklyTaskSchedule(string mode, string exeFileDir, string rootPath,
            int zipLogDaysAfterLogged, int deleteLogDaysAfterLogged,
            int zipImgDaysAfterLogged, int deleteImgDaysAfterLogged,
            int zipCsvDaysAfterLogged, int deleteCsvDaysAfterLogged,
            DateTime startTime, string weekday = "monday")
        {
            AddTaskSchedule(CreateExeAction(mode, exeFileDir, rootPath,
                    zipLogDaysAfterLogged, deleteLogDaysAfterLogged,
                    zipImgDaysAfterLogged, deleteImgDaysAfterLogged,
                    zipCsvDaysAfterLogged, deleteCsvDaysAfterLogged),
                    CreateWeeklyTrigger(startTime, weekday));
        }

        public static void AddMonthlyTaskSchedule(string mode, string exeFileDir, string rootPath,
            int zipLogDaysAfterLogged, int deleteLogDaysAfterLogged,
            int zipImgDaysAfterLogged, int deleteImgDaysAfterLogged,
            int zipCsvDaysAfterLogged, int deleteCsvDaysAfterLogged,
            DateTime startTime, int selectADayInMonth = 1)
        {
            AddTaskSchedule(CreateExeAction(mode, exeFileDir, rootPath,
                    zipLogDaysAfterLogged, deleteLogDaysAfterLogged,
                    zipImgDaysAfterLogged, deleteImgDaysAfterLogged,
                    zipCsvDaysAfterLogged, deleteCsvDaysAfterLogged),
                    CreateMonthlyTrigger(startTime, selectADayInMonth));
        }


        public static System.Threading.Tasks.Task AddDailyTaskScheduleAsync(string mode, string exeFileDir, string rootPath,
            int zipLogDaysAfterLogged, int deleteLogDaysAfterLogged,
            int zipImgDaysAfterLogged, int deleteImgDaysAfterLogged,
            int zipCsvDaysAfterLogged, int deleteCsvDaysAfterLogged,
            DateTime startTime)
        {

            return System.Threading.Tasks.Task.Run(() => AddTaskSchedule(CreateExeAction(mode, exeFileDir, rootPath,
                    zipLogDaysAfterLogged, deleteLogDaysAfterLogged,
                    zipImgDaysAfterLogged, deleteImgDaysAfterLogged,
                    zipCsvDaysAfterLogged, deleteCsvDaysAfterLogged),
                    CreateDailyTrigger(startTime))
                );
        }

        public static System.Threading.Tasks.Task AddWeeklyTaskScheduleAsync(string mode, string exeFileDir, string rootPath,
            int zipLogDaysAfterLogged, int deleteLogDaysAfterLogged,
            int zipImgDaysAfterLogged, int deleteImgDaysAfterLogged,
            int zipCsvDaysAfterLogged, int deleteCsvDaysAfterLogged,
            DateTime startTime, string weekday = "monday")
        {
            return System.Threading.Tasks.Task.Run(() => AddTaskSchedule(CreateExeAction(mode, exeFileDir, rootPath,
                    zipLogDaysAfterLogged, deleteLogDaysAfterLogged,
                    zipImgDaysAfterLogged, deleteImgDaysAfterLogged,
                    zipCsvDaysAfterLogged, deleteCsvDaysAfterLogged),
                    CreateWeeklyTrigger(startTime, weekday))
                );
        }

        public static System.Threading.Tasks.Task AddMonthlyTaskScheduleAsync(string mode, string exeFileDir, string rootPath,
            int zipLogDaysAfterLogged, int deleteLogDaysAfterLogged,
            int zipImgDaysAfterLogged, int deleteImgDaysAfterLogged,
            int zipCsvDaysAfterLogged, int deleteCsvDaysAfterLogged,
            DateTime startTime, int selectADayInMonth = 1)
        {
            return System.Threading.Tasks.Task.Run(() => AddTaskSchedule(CreateExeAction(mode, exeFileDir, rootPath,
                    zipLogDaysAfterLogged, deleteLogDaysAfterLogged,
                    zipImgDaysAfterLogged, deleteImgDaysAfterLogged,
                    zipCsvDaysAfterLogged, deleteCsvDaysAfterLogged),
                    CreateMonthlyTrigger(startTime, selectADayInMonth))
                );
        }

        private static ExecAction CreateExeAction(string mode, string exeFileDir, string rootPath,
            int zipLogDaysAfterLogged, int deleteLogDaysAfterLogged,
            int zipImgDaysAfterLogged, int deleteImgDaysAfterLogged,
            int zipCsvDaysAfterLogged, int deleteCsvDaysAfterLogged)
        {
            ExecAction CogAplex = new ExecAction();
            CogAplex.Path = exeFileDir;
            CogAplex.Arguments =
                $"{mode} {rootPath} " +
                $"{zipLogDaysAfterLogged} {deleteLogDaysAfterLogged} " +
                $"{zipImgDaysAfterLogged} {deleteImgDaysAfterLogged} " +
                $"{zipCsvDaysAfterLogged} {deleteCsvDaysAfterLogged}";
            return CogAplex;
        }

        private static DailyTrigger CreateDailyTrigger(DateTime startTime)
        {
            DailyTrigger dailyTrigger = new DailyTrigger
            {
                StartBoundary = startTime,
                DaysInterval = 1
            };
            return dailyTrigger;
        }

        private static WeeklyTrigger CreateWeeklyTrigger(DateTime startTime, string weekday)
        {
            WeeklyTrigger weeklyTrigger = new WeeklyTrigger
            {
                StartBoundary = startTime,
                DaysOfWeek = SelectWeekday(weekday),
                WeeksInterval = 1
            };
            return weeklyTrigger;
        }

        private static DaysOfTheWeek SelectWeekday(string weekday)
        {
            return (DaysOfTheWeek)Enum.Parse(typeof(DaysOfTheWeek), weekday, true);
        }

        private static MonthlyTrigger CreateMonthlyTrigger(DateTime startTime, int dayInMonth)
        {
            MonthlyTrigger monthlyTrigger = new MonthlyTrigger
            {
                StartBoundary = startTime,
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
