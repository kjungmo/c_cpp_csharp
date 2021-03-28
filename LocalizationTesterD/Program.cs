using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.Runtime.InteropServices;
using System.Text;
using System.Diagnostics;

namespace LocalizationTesterD
{
    static class Program
    {
    /// <summary>
    /// 해당 애플리케이션의 주 진입점입니다.
    /// </summary>
    [STAThread]
        static void Main()
        {

            Application.ThreadException += Application_ThreadException;
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.Automatic);
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            throw new Exception("UI 생성 전 Exception throw.");

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Trace.WriteLine($"CurrentDomain_UnhandledException " +
                $"{((Exception)e.ExceptionObject).Message}");
            MessageBox.Show($"CurrentDomain_UnhandledException " +
                $"{((Exception)e.ExceptionObject).Message} Is Terminating: {e.IsTerminating.ToString()}");
        }

        private static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            Trace.WriteLine($"Application_ThreadException {e.Exception.Message}");
            MessageBox.Show($"Application_ThreadException {e.Exception.Message}");
            Application.Exit(new System.ComponentModel.CancelEventArgs(false));
        }

    }

}
