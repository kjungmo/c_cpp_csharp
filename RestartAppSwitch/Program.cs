using System;
using System.Diagnostics;
using System.IO;

namespace RestartAppSwitch
{
    internal class Program
    {
        private static readonly string s_fineLocalizer = "FineLocalizer";

        private static void Main(string[] args)
        {
            if (args.Length > 2 && args[1] == "restart")
            {
                RestartProcess(s_fineLocalizer);
            }
            else
            {
                StartProcess(s_fineLocalizer);
            }
        }

        private static void RestartProcess(string processName)
        {
            KillProcess(processName);
            StartProcess(processName);
        }

        private static void KillProcess(string processName)
        {
            Process[] fineLocalizer = Process.GetProcessesByName(processName);
            try
            {
                if (fineLocalizer.Length > 0)
                {
                    fineLocalizer[0].Kill();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message}");
            }
        }

        private static void StartProcess(string processName)
        {
            Process[] fineLocalizer = Process.GetProcessesByName(processName);
            try
            {
                if (fineLocalizer.Length < 1)
                {
                    string procPath = Path.Combine(Environment.CurrentDirectory, $"{processName}.exe");
                    if (!File.Exists(procPath))
                    {
                        Console.WriteLine($"We need {processName}.exe in the same directory");
                        return;
                    }

                    Process process = new Process();
                    process.StartInfo.FileName = procPath;
                    process.StartInfo.WindowStyle = ProcessWindowStyle.Normal;

                    process.Start();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message}");
            }
        }
    }
}
