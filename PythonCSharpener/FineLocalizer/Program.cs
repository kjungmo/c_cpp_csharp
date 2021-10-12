using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Security.Permissions;
using CommonUtils;

namespace FineLocalizer
{
    static class Program
    {
        private static readonly LogHelper Logger = LogHelper.Logger;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.ControlAppDomain)]
		[STAThread]
		static void Main(string[] args)
        {
            if (System.Diagnostics.Process.GetProcessesByName("FineLocalizer").Length > 1)
            {
                MessageBox.Show(Lang.MsgBoxFineLo.ProgramAlreadyRunning, Lang.MsgBoxFineLo.WarningTitle);
                return;
            }

            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.ThrowException, false);
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                Exception ex = (Exception)e.ExceptionObject;
                Logger.Fatal($"{Lang.LogsFineLo.CaughtUnhandledException} ({ex.Message}){Environment.NewLine}{ex.StackTrace}");
            };

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

#if !DEVEL
            SplashScreen screen = new SplashScreen(Properties.Resources.SplashScreen_Finelocalizer);
            Task splashScreen = screen.ShowSplashScreen(2500);

#if TRIAL_VERSION
            if (!LicenseValidator.ValidateByExpiryDate(new DateTime(2021, 7, 27, 13, 1, 1)))
            {
                MessageBox.Show(Lang.MsgBoxFineLo.LicenseExpired, Lang.MsgBoxFineLo.WarningTitle);
                return;
            }
#endif
#endif

            Config<FineLocalizerConfig> config;
            try
            {
                config = ConfigFileManager.LoadFromFile<Config<FineLocalizerConfig>>(ConfigFileManager.GetConfigFilePath());
            }
            catch (Exception)
            {
                MessageBox.Show(Lang.MsgBoxFineLo.LoadingConfigError, Lang.MsgBoxFineLo.WarningTitle);
                return;
            }

            if (args.Length >= 2 && args[0] == "--clientIp")
            {
                foreach (var robotConfig in config.RobotConfigs.Values)
                {
                    if (robotConfig[RobotAttribute.Maker].ToEnum<RobotMaker>() == RobotMaker.HYUNDAI)
                    {
                        robotConfig[RobotAttribute.ClientIp] = args[1];
                    }
                }
            }

            Logger.Configure(config.LogPath, config.MinimumUiLogLevel, config.MinimumFileLogLevel, writeCsv: true);
            FineLocalizerForm_ f = new FineLocalizerForm_(config);

#if !DEVEL
            splashScreen.Wait();
#endif

            Application.Run(f);
            Logger.Info(Lang.LogsFineLo.ProgramEnd);
        }
    }
}
