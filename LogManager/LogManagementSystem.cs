using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace LogManager
{
    public class LogManagementSystem
    {
        static void Main(string[] args)
        {
            // |||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||
            // |                                                                                                                       |
            // |                                                                                                                       |
            // |      [SCHEDULER MODE] : args ->  Mode, RootPath,                                                                      |
            // |                                   zipLogDaysAfterLogged, deleteLogDaysAfterLogged,                                    |
            // |                                   zipImgDaysAfterLogged, deleteImgDaysAfterLogged,                                    |
            // |                                   zipCsvDaysAfterLogged, deleteCsvDaysAfterLogged,                                    |
            // |                                   startTime, interval                                                                 |
            // |                                                                                                                       |
            // |                                                                                                                       |
            // |                                                                                                                       |
            // |                                                                                                                       |
            // |                                                                                                                       |
            // |     [ZIP/NO_ZIP MODE] : args ->  Mode, RootPath,                                                                      |
            // |                                   zipLogDaysAfterLogged, deleteLogDaysAfterLogged,                                    |
            // |                                   zipImgDaysAfterLogged, deleteImgDaysAfterLogged,                                    |
            // |                                   zipCsvDaysAfterLogged, deleteCsvDaysAfterLogged,                                    |
            // |                                                                                                                       |
            // |                                                                                                                       |
            // |                                                                                                                       |
            // |                                                                                                                       |
            // |                                                                                                                       |
            // |||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||


            if (CheckArgsValidity(args))
            {
                string mode = args[0].ToLower();
                switch (mode)
                {
                    case "schedule_zip":
                    case "schedule_no_zip":
                        TaskSchedulerManager.FillTaskScheduleArgs(args);

                        if (mode == "schedule_zip")
                            TaskSchedulerManager.Mode = "zip";

                        else 
                            TaskSchedulerManager.Mode = "no_zip";

                        TaskSchedulerManager.RegisterTaskScheduler(System.Reflection.Assembly.GetExecutingAssembly().Location);
                        break;

                    case "zip":
                        List<string> temp = new List<string>();
                        ZipHelper.FillLogManagerArgs(args);
                        using (ZipArchive archive = ZipFile.Open(CreateZipFile(ZipHelper.RootPath, "LOG"), ZipArchiveMode.Update))
                        {
                            ZipHelper.UpdateZipFileEntries(archive);
                            ZipHelper.SortLoggedFiles(temp, archive);
                        }
                        Console.WriteLine("Management Success.");
                        break;

                    case "no_zip":
                        ZipHelper.FillLogManagerArgs(args);
                        ZipHelper.SortLoggedFiles();
                        break;

                    case "schedule_delete":
                        if (!string.IsNullOrEmpty(TaskSchedulerManager.CheckAlreadyRegistered()))
                        {
                            TaskSchedulerManager.DeleteTaskSchedule();
                            Console.WriteLine("Successfully deleted!");
                        }
                        Console.WriteLine("No Scheduled LogManager to delete.");
                        break;

                    case "schedule_load":
                        Console.WriteLine($"\nFound LogManager's Description: {TaskSchedulerManager.CheckAlreadyRegistered() ?? "None"}");
                        break;
                }
            }
            else
            {
                Console.WriteLine("The Program Cannot be executed");
            }
        }

        private static bool CheckArgsValidity(string[] modeArgs)
        {
            if (modeArgs.Length < 2)
            {
                List<string> modeSelection = new List<string> { "schedule_delete", "schedule_load", };
                if (!modeSelection.Contains(modeArgs[0].ToLower()))
                {
                    Console.WriteLine("Correct Mode required. [ schedule_delete / schedule_load ]");
                    return false;
                }
            }
            else if (modeArgs.Length < 9)
            {
                if (!CheckZipNoZipArgs(modeArgs))
                {
                    return false;
                }
            }
            else
            {
                if (!CheckScheduleArgs(modeArgs))
                {
                    return false;
                }
            }
            return true;
        }

        private static bool CheckZipNoZipArgs(string[] arguments)
        {
            List<string> modeSelection = new List<string> { "zip", "no_zip", };
            if (!modeSelection.Contains(arguments[0].ToLower()))
            {
                Console.WriteLine("Correct Mode required. [ zip / no_zip ]");
                return false;
            }

            if (!CheckLogPath(arguments[1]))
            {
                Console.WriteLine("Root path does not exist");
                return false;
            }

            if (!CheckSixInputDays(
                arguments[2], arguments[3],
                arguments[4], arguments[5],
                arguments[6], arguments[7]))
            {
                return false;
            }
            return true;
        }

        private static bool CheckLogPath(string rootPath)
        {
            bool isPath = true;
            if (!Directory.Exists(rootPath))
            {
                Console.WriteLine("Root path does not exist");
                isPath = false;
            }
            return isPath;
        }

        private static bool CheckSixInputDays(
            string day1, string day2,
            string day3, string day4,
            string day5, string day6)
        {

            Dictionary<string, string> days = new Dictionary<string, string>()
            {
                [day1] = "zipLog",
                [day2] = "delLog",
                [day3] = "zipImg",
                [day4] = "delImg",
                [day5] = "zipCsv",
                [day6] = "delCsv",
            };

            foreach (KeyValuePair<string, string> item in days)
            {
                if (!int.TryParse(item.Key, out _))
                {
                    Console.WriteLine($"Correct number of {item.Value} required");
                    return false;
                }
            }

            return true;
        }

        private static bool CheckScheduleArgs(string[] arguments)
        {
            List<string> modeSelection = new List<string> { "schedule_zip", "schedule_no_zip", };
            if (!modeSelection.Contains(arguments[0].ToLower()))
            {
                Console.WriteLine("Correct Mode required. [ schedule_zip / schedule_no_zip ]");
                return false;
            }

            if (!CheckLogPath(arguments[1]))
            {
                Console.WriteLine("Root path does not exist");
                return false;
            }

            if (!CheckSixInputDays(
                arguments[2], arguments[3],
                arguments[4], arguments[5],
                arguments[6], arguments[7]))
            {
                return false;
            }

            if (!CheckStartTimeArgs(arguments[8]))
            {
                return false;
            }

            List<string> intervalSelection = new List<string> { "daily", "weekly", "monthly", };
            if (!intervalSelection.Contains(arguments[9].ToLower()))
            {
                Console.WriteLine("Correct Interval required. [ daily / weekly / monthly ]");
                return false;
            }

            if (arguments[9] == "weekly" && arguments.Length == 11)
            {
                if (!CheckWeekTrigArgs(arguments[10]))
                {
                    return false;
                }
            }

            if (arguments[9] == "monthly" && arguments.Length == 11)
            {
                if (!CheckMonTrigArgs(arguments[10]))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool CheckStartTimeArgs(string startTime)
        {
            string format = "HH:mm";
            bool rightTime = false;
            try
            {
                System.Globalization.CultureInfo provider = System.Globalization.CultureInfo.InvariantCulture;
                DateTime dt = DateTime.ParseExact(startTime, format, provider);
                rightTime = true;
            }
            catch (Exception)
            {
                Console.WriteLine("Wrong StartTime input. Time format is HH:mm(24H)");
            }
            return rightTime;
        }

        private static bool CheckWeekTrigArgs(string weekday)
        {
            bool isWeekday = true;
            List<string> weekdays = new List<string>
            {
                "monday", "tuesday", "wednesday", "thursday", "friday", "saturday", "sunday",
            };

            if (!weekdays.Contains(weekday.ToLower()))
            {
                Console.WriteLine("Correct weekday required. [  monday / tuesday / wednesday / thursday / friday / saturday / sunday ]");
                isWeekday = false;
            }

            return isWeekday;
        }

        private static bool CheckMonTrigArgs(string dayInMonth)
        {
            bool isMonth = true;
            int day;
            if (!int.TryParse(dayInMonth, out day))
            {
                isMonth = false;
                Console.WriteLine($"Correct number of Day required");
            }

            else if (day != -1 || (day < 1 || day > 31))
            {
                Console.WriteLine($"Correct number of Day required : -1 for Last day, 1 <= day <= 31");
            }

            return isMonth;
        }

        private static string CreateZipFile(string rootPath, string zipfileName)
        {
            string zipFilePath = Path.Combine(rootPath, string.Concat(zipfileName, ".zip"));

            if (!File.Exists(zipFilePath))
            {
                try
                {
                    var myfile = File.Create(zipFilePath);
                    myfile.Close();
                }
                catch (Exception e)
                {
                    Console.WriteLine($"{e.Message} \nUnable to create zipfile");
                }
            }
            return zipFilePath;
        }
    }
}