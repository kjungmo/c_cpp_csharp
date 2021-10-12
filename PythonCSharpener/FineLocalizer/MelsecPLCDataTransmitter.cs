using CommonUtils;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Configuration;


namespace FineLocalizer
{
    class MelsecPLCDataTransmitter
    {
        private static readonly LogHelper Logger = LogHelper.Logger;
        private string _melsecPlcDataProcName;
        private Process _melsecPlcDataProc;

        public MelsecPLCDataTransmitter(GlassInsertionPart insertionPart)
        {
            SetMelsecPlcDataExeName(insertionPart);
        }

        public Task InitMelsecPlcDataProcessAsync()
        {
            return Task.Run(() => 
            { 
                if (!string.IsNullOrEmpty(_melsecPlcDataProcName) && Process.GetProcessesByName(_melsecPlcDataProcName).Length < 1)
                {
                    try
                    {
                        var procPath = Path.Combine(Application.StartupPath, $"{_melsecPlcDataProcName}.exe");
                        if (!File.Exists(procPath))
                        {
                            Logger.Debug($"We need {_melsecPlcDataProcName} in the same directory");
                            return;
                        }

                        _melsecPlcDataProc = new Process();
                        _melsecPlcDataProc.StartInfo.FileName = procPath;
                        _melsecPlcDataProc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;

                        _melsecPlcDataProc.Start();
                    }
                    catch (Exception ex)
                    {
                        Logger.Debug($"MelsecPLCDataProcess initialization error...{ex.Message}");
                    }
                }
            });
        }

        public Task TerminateMelsecPlcDataProcAsync()
        {
            return Task.Run(() =>
            {
                if (!string.IsNullOrEmpty(_melsecPlcDataProcName))
                {
                    foreach (Process process in Process.GetProcessesByName(_melsecPlcDataProcName))
                    {
                        process.Kill();
                    }
                }
            });
        }

        public void SetMelsecPlcDataExeName(GlassInsertionPart insertionPart)
        {
            if (insertionPart == GlassInsertionPart.LOW)
            {
                _melsecPlcDataProcName = "MelsecPLCDataReceiver";
            }
            else if(insertionPart.ToString() == ConfigurationManager.AppSettings["MelsecPLCDataSender"].ToUpper())
            {
                _melsecPlcDataProcName = "MelsecPLCDataSender";
            }
            else
            {
                _melsecPlcDataProcName = "";
            }
        }
    }
}
