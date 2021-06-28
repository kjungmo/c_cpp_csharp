using Microsoft.Win32.TaskScheduler;
using System;

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

        private enum ExeInterval
        {
            DAILY,
            WEEKLY,
            MONTHLY,
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

        //TODO-switch method
        public Trigger SelectTrigger(bool stopFlag = true)
        {
            ExeInterval triggerInterval;
            if (!Enum.TryParse<ExeInterval>(ExecutionInterval.ToUpper(), out triggerInterval))
            {
                return null;
            }

            switch (triggerInterval) // enum말고 string 으로 switch할 것.
            {
                case ExeInterval.DAILY:
                    return CreateDailyTrigger(stopFlag);

                case ExeInterval.WEEKLY:
                    return CreateWeeklyTrigger(stopFlag);

                case ExeInterval.MONTHLY:
                    return CreateMonthlyTrigger(stopFlag);
                //return CreateMonthlyTrigger2(stopFlag);

                default:
                    return CreateDailyTrigger(stopFlag);
            }

        }

        private DailyTrigger CreateDailyTrigger(bool stopFlag = true)
        {
            // [DAILY] - StartBoundary(DateTime), DaysInterval(int), stopFlag(bool)
            DailyTrigger dailyTrigger = new DailyTrigger();
            dailyTrigger.StartBoundary = DateTime.Today.AddDays(1);
            dailyTrigger.DaysInterval = 1;
            if (!stopFlag)
            {
                dailyTrigger.Enabled = false;
            }
            return dailyTrigger;
        }

        private WeeklyTrigger CreateWeeklyTrigger(bool stopFlag = true)
        {
            // [WEEKLY] - StartBoundray(DateTime), DaysOfWeek(DaysOfTheWeek), WeeksInterval(int), stopFlag(bool)
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

        private MonthlyTrigger CreateMonthlyTrigger(bool stopFlag = true)
        {
            // [MONTHLY] - StartBoundary(DateTime), DaysOfWeek(DaysOfTheWeek), MonthsOfYear(MonthsOfTheYear), stopFlag(bool)
            // Type : Day
            // starts tomorrow, triggers on the first day of every month, doesn't run on the last day of the month

            MonthlyTrigger monthlyTrigger = new MonthlyTrigger();
            monthlyTrigger.StartBoundary = DateTime.Today.AddDays(1);
            monthlyTrigger.DaysOfMonth = new int[] { 1 };
            monthlyTrigger.MonthsOfYear = MonthsOfTheYear.AllMonths;
            monthlyTrigger.RunOnLastDayOfMonth = false; // V2 only
            if (!stopFlag) // 그냥 delete하는 걸로 
            {
                monthlyTrigger.Enabled = false;
            }
            return monthlyTrigger;
        }

        private MonthlyDOWTrigger CreateMonthlyTrigger2(bool stopFlag = true)
        {
            // [MONTHLY] - StartBoundary(DateTime), DaysOfWeek(DaysOfTheWeek), MonthsOfYear(MonthsOfTheYear), stopFlag(bool)
            //// Type : Weekday
            ////OR starts tomorrow, triggers on the first week's monday of every month

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
