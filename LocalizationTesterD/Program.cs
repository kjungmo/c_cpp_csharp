using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.Runtime.InteropServices;
using System.Text;
using System.Diagnostics;
using System.Security.Permissions;
using LocalizationTesterD.Tools;
using System.Windows;
using System.Runtime.ExceptionServices;

namespace LocalizationTesterD
{
    static class Program
    {
        /// <summary>
        /// 해당 애플리케이션의 주 진입점입니다.
        /// </summary>
        /// 
        [SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.ControlAppDomain)]
        static void Main()
        {
            LogManager logger = new LogManager();
            Application.ThreadException += (s, e) =>
            {
                Console.WriteLine("Application ThreadException here");
                HandleException(e.Exception);
                logger.WriteLine("Caught by ThreadException");
            };
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.Automatic, false);
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Console.WriteLine($"Application Thread: {Thread.CurrentThread.ManagedThreadId}");
            //throw new Exception("Before Run( )");
            //new Thread(new ThreadStart((() =>
            //{
            //    Console.WriteLine($"NEW THREAD: {Thread.CurrentThread.ManagedThreadId}");
            //    throw new Exception("New Thread in Main( )");
            //}))).Start();
            Form1 fo = new Form1(logger);
            Application.Run(fo);
            
        }


        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Console.WriteLine("Unhandled");
            HandleException(e.ExceptionObject as Exception);
            LogManager log = new LogManager();
            log.WriteLine("Caught by UnhandledException");

        }

        private static void HandleException(Exception ex)
        {
            LogManager logger = new LogManager();
            Console.WriteLine($"Exception occurred : {ex.Message}");
            Console.WriteLine($"Location : {ex.StackTrace}");
            logger.WriteLine($"\noccurred exception: {ex.Message} \nwhere: {ex.StackTrace}");

        }
    }
}
