using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Windows.Forms;
using CommonUtils;
using System.Configuration;
using OpenGL;
using System.Threading;
using System.Globalization;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Configuration;

namespace FineLocalizer
{
    public partial class FineLocalizerForm_ : Form
    {
        private static readonly LogHelper Logger = LogHelper.Logger;

        private OperationMode _mode = OperationMode.Set;
        private bool _isJustModeSelection;
        private Config<FineLocalizerConfig> _config = new Config<FineLocalizerConfig>();
        private PropertyAccessor<Config<FineLocalizerConfig>> _bConfAcc = new PropertyAccessor<Config<FineLocalizerConfig>>();
        private PropertyAccessor<FineLocalizerConfig> _sConfAcc = new PropertyAccessor<FineLocalizerConfig>();

        private int _currentCar;
        private int _currentCarSeq;

        private bool _isCameraConnected = false;
        private bool _isCameraConnecting;

        private IPLCComm<PlcSignalForLocalizer> _plcComm;
        private IPlcForLocalizer _plcForThisTask;
        public bool IsPLCConnected { get; set; } = false;
        private bool _isPlcEvtHandlersRegistered = false;
        private PlcStatusPainter _plcStatusPainter;
        private Dictionary<PlcSignalForLocalizer, Label> _plcSignalLabelDict;
        private delegate void UpdatePlcSignalStatusDelegate();
        private UpdatePlcSignalStatusDelegate _updatePlcSignalStatus;

        private Button[] _btnPoints;
        private GroupBox[] _gbPoints;

        private Button[] _btnGapPoints;
        private GroupBox[] _gbGapPoints;

        private List<RobotPose> _calPositions = new List<RobotPose>();

        private RefPoses _refPoses = new RefPoses()
        {
            Values = new List<RobotPose>()
        };

        private GlDisplayManager _glManagerGlass1;
        private GlDisplayManager _glManagerGlass2;
        private GlDisplayManager _glManagerVehicle1;
        private GlDisplayManager _glManagerVehicle2;
        private bool _isVehicleGlMax = false;

        private Dictionary<PlcModel, Action> _setPlcFuncDict;

        private FineLocalizerVehicle _fineLocalizerVehicle;
        private GapChecker _gapChecker;
        private GlassChecker _glassChecker;

        private Dictionary<string, bool> _isDataSourceChanging = new Dictionary<string, bool>();

        private TextBox[] _tbGapCameraSerials;
        private TextBox[] _tbGlassCameraSerials;
        private TextBox[] _tbVehicleCameraSerials;

        private bool _isGlassCheckReady;
        private bool _isVehicleCheckReady;
        private bool _isGapCheckReady;

        private MelsecPLCDataTransmitter _melsecPlcDataTransmitter;
        private NgListViewManager _ngListView;

        private FineLocalizerDashboard_ _dashBoard;
        private Screen[] _screenArray = Screen.AllScreens;
        private bool _isDashBoardDisplay;

        private bool _isConfigModified;

        // in order to prevent to move this form
        protected override void WndProc(ref Message message)
        {
            const int WM_SYSCOMMAND = 0x0112;
            const int SC_MOVE = 0xF010;

            switch (message.Msg)
            {
                case WM_SYSCOMMAND:
                    int command = message.WParam.ToInt32() & 0xfff0;
                    if (command == SC_MOVE)
                        return;
                    break;
            }

            base.WndProc(ref message);
        }


        public FineLocalizerForm_(Config<FineLocalizerConfig> config)
        {
            InitializeComponent();

            WindowsUtils.PreventChangingHangulMode(Handle);

            Size desiredSize = new Size(Math.Min(Screen.FromHandle(Handle).WorkingArea.Width, 1920),
                                        Math.Min(Screen.FromHandle(Handle).WorkingArea.Height, 1080));
            Size = desiredSize;
            int margin = (desiredSize.Width - ClientSize.Width) / 2 - 1;
            Location = new Point(-margin, 0);
            Size = new Size(desiredSize.Width + 2 * margin, desiredSize.Height + margin);

            Logger.ResMgr = Lang.EngineLogFineLo.ResourceManager;
            SelectLanguage(new CultureInfo("ko-kr"));
            FontManager.SetCustomFont("./Resources/NanumSquareRoundB.ttf");
            this.ApplyFont();

            _ngListView = new NgListViewManager(this.ltvNG,12,12);

            glControlVehicle1.MouseWheel += glControl1_MouseWheel;
            glControlVehicle2.MouseWheel += glControlVehicle2_MouseWheel;
            glControlGlass1.MouseWheel += glControlGlass1_MouseWheel;
            glControlGlass2.MouseWheel += glControlGlass2_MouseWheel;

            btnVehicleRefDataPath.Click += (s, e) => OnBtnBrowseFolderPath(tbVehicleRefDataPath);
            btnVehicleHandEyeCalFileBrowser.Click += (s, e) => OnBtnBrowseFolderPath(tbVehicleHandEyeCalibFilePath);
            btnVehicleBirdEyeCaliFileBrowser.Click += (s, e) => btnBirdEyeCaliFileBrowser_Click(s, e);
            btnGapRefDataPath.Click += (s, e) => OnBtnBrowseFolderPath(tbGapRefDataPath);
            btnGapCalibPath.Click += (s, e) => OnBtnBrowseFolderPath(tbGapHandEyeCalibFilePath);
            btnGlassRefDataPath.Click += (s, e) => OnBtnBrowseFolderPath(tbGlassRefDataPath);
            btnGlassHandEyeCalibFilePath.Click += (s, e) => OnBtnBrowseFolderPath(tbGlassHandEyeCalibFilePath);

            _btnPoints = new Button[] { btnPoint1_, btnPoint2_, btnPoint3_, btnPoint4_ };
            _gbPoints = new GroupBox[] { gbP1_, gbP2_, gbP3_, gbP4_ };
            _btnGapPoints = new Button[] { btnGapPoint1_, btnGapPoint2_, btnGapPoint3_, btnGapPoint4_ };
            _gbGapPoints = new GroupBox[] { gbGapP1_, gbGapP2_, gbGapP3_, gbGapP4_ };
            _tbGapCameraSerials = new TextBox[] { tbGapCameraSerial1, tbGapCameraSerial2, tbGapCameraSerial3, tbGapCameraSerial4 };
            _tbGlassCameraSerials = new TextBox[] { tbGlassCameraSerial1, tbGlassCameraSerial2, tbGlassCameraSerial3, tbGlassCameraSerial4 };
            _tbVehicleCameraSerials = new TextBox[] { tbVehicleCameraSerial1, tbVehicleCameraSerial2, tbVehicleCameraSerial3, tbVehicleCameraSerial4 };

            _setPlcFuncDict = new Dictionary<PlcModel, Action>()
            {
                [PlcModel.SIEMENS] = SetS7PlcToUse,
                [PlcModel.MELSEC] = SetMelsecPlcToUse
            };

            if (!LicenseValidator.ValidateLicenseKey(false))
            {
                MessageBox.Show("!@InvalidLicense", "!@WarningTitle");
                Environment.Exit(0);
            }

            try
            {
                _config = config;

                Logger.RtbLog = rtbLog;
                Logger.MaxLine = 1000;
                FineLocalizerVehicleEngineAPI.LogCallback = Logger.WriteLog;

                Logger.Info("!@ProgramStart");

                InfoSettingsToEngine();
                RefPosesFileManager.LoadCamSettings();
                SetComboBoxDataSource(comboBoxCarType, _config.GetCarTypeList());
                comboBoxCarType.SelectItemAndFireEventUnconditionally(_config.RecentlyUsedCar, comboBoxCarType_SelectedIndexChanged);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{"!@LoadingConfigError"} {ex.Message}");
            }

            _plcStatusPainter = new PlcStatusPainter(groupBoxPLC.CreateGraphics(), 20, 22);
            LoadPlcSignalLabelDict();

            _isDashBoardDisplay = Convert.ToBoolean(ConfigurationManager.AppSettings["DashBoard"]);
            DisplayDashBoard(_isDashBoardDisplay);
            if (Convert.ToBoolean(ConfigurationManager.AppSettings["MelsecPLCDataTransmitter"]))
            {
                _melsecPlcDataTransmitter = new MelsecPLCDataTransmitter(_config.InsertionPart);
            }
        }

        private void SetComboBoxDataSource(ComboBox cb, object dataSource)
        {
            _isDataSourceChanging[cb.Name] = true;
            cb.DataSource = dataSource;
        }
        
        private void UpdateUIFromConfig()
        {
            tbCarName.Text = _config[-1].CarName;

            if (_config.CameraConfigs.TryGetValue(_config[-1].GapCamera ?? "", out var gapConf))
            {
                tbGapCamera.Text = _config[-1].GapCamera;
                tbGapCameraModel.Text = gapConf[0][CameraAttribute.Model];
                for (var i = 0; i < gapConf.Count; ++i)
                {
                    _tbGapCameraSerials[i].Text = gapConf[i][CameraAttribute.Serial];
                }
                for (var i = gapConf.Count; i < _tbGapCameraSerials.Length; ++i)
                {
                    _tbGapCameraSerials[i].Text = "-";
                }
            }
            else 
            {
                _config[-1].GapCamera = "";
                tbGapCamera.Text = "";
                tbGapCameraModel.Text = "";
                for (var i = 0; i < 4; ++i)
                {
                    _tbGapCameraSerials[i].Text = "";
                }
            }

            if (_config.CameraConfigs.TryGetValue(_config[-1].GlassCamera ?? "", out var glassConf))
            {
                tbGlassCamera.Text = _config[-1].GlassCamera;
                tbGlassCameraModel.Text = glassConf[0][CameraAttribute.Model];
                for (var i = 0; i < glassConf.Count; ++i)
                {
                    _tbGlassCameraSerials[i].Text = glassConf[i][CameraAttribute.Serial];
                }
                for (var i = glassConf.Count; i < _tbGlassCameraSerials.Length; ++i)
                {
                    _tbGlassCameraSerials[i].Text = "-";
                }
            }
            else 
            {
                _config[-1].GlassCamera = "";
                tbVehicleCamera.Text = "";
                tbVehicleCameraModel.Text = "";
                for (var i = 0; i < 4; ++i)
                {
                    _tbVehicleCameraSerials[i].Text = "";
                }
            }

            if (_config.CameraConfigs.TryGetValue(_config[-1].VehicleCamera ?? "", out var vehicleConf))
            {
                tbVehicleCamera.Text = _config[-1].VehicleCamera;
                tbVehicleCameraModel.Text = vehicleConf[0][CameraAttribute.Model];
                for (var i = 0; i < vehicleConf.Count; ++i)
                {
                    _tbVehicleCameraSerials[i].Text = vehicleConf[i][CameraAttribute.Serial];
                }
                for (var i = vehicleConf.Count; i < _tbVehicleCameraSerials.Length; ++i)
                {
                    _tbVehicleCameraSerials[i].Text = "-";
                }
            }
            else 
            {
                _config[-1].VehicleCamera = "";
                tbGlassCamera.Text = "";
                tbGlassCameraModel.Text = "";
                for (var i = 0; i < 4; ++i)
                {
                    _tbGlassCameraSerials[i].Text = "";
                }
            }

            tbVehicleRefDataPath.Text = _config[-1].VehicleRefDataPath;
            tbVehicleHandEyeCalibFilePath.Text = _config[-1].VehicleHandEyeCalibFilePath;
            tbVehicleBirdEyeCalibFilePath.Text = _config[-1].VehicleBirdEyeCalibFilePath;

            tbGapRefDataPath.Text = _config[-1].GapRefDataPath;
            tbGapHandEyeCalibFilePath.Text = _config[-1].GapHandEyeCalibFilePath;

            tbGlassRefDataPath.Text = _config[-1].GlassRefDataPath;
            tbGlassHandEyeCalibFilePath.Text = _config[-1].GlassHandEyeCalibFilePath;

            tbPlc.Text = _config.Plc.ToPlcInfoString(_config.PlcConfigs[_config.Plc]);

            if (_config.RobotPoseVariables.TryGetValue($"{_config.RecentlyUsedCar}_glass_offset", out var glassOffset))
            {
                UpdateGlassShiftValueToUi(glassOffset, gbGlassOffset_);
            }
            else
            {
                ClearGlassShiftValueUi(gbGlassOffset_);
            }

            if (_config.RobotPoseVariables.TryGetValue($"{_config.RecentlyUsedCar}_vehicle_offset", out var vehicleOffset))
            {
                UpdateRobotPoseToUI(vehicleOffset, gbVehicleOffset_);
            }
            else
            {
                ClearRobotPoseUi(gbVehicleOffset_);
            }

            bool isBirdEyePathNeeded = _config[-1].ScanInstallMode == ScanInstallMode.DIFF_SCAN_INSTALL;
            label32_.Visible = isBirdEyePathNeeded;
            tbVehicleBirdEyeCalibFilePath.Visible = isBirdEyePathNeeded;
            btnVehicleBirdEyeCaliFileBrowser.Visible = isBirdEyePathNeeded;
        }

        private void DisplayDashBoard(bool isDisplay) 
        {
            if (isDisplay && _screenArray.Length > 1)
            {
                _dashBoard = new FineLocalizerDashboard_(_currentCar.ToString(), _config[-1].CarName, _currentCarSeq.ToString());
                Screen screen = (_screenArray[0] == Screen.PrimaryScreen) ? _screenArray[1] : _screenArray[0];
                Size sdesiredSize = (screen.WorkingArea.Width < screen.WorkingArea.Height)
                    ? new Size(Math.Min(screen.WorkingArea.Width, 1080), Math.Min(screen.WorkingArea.Height, 1920))
                    : new Size(Math.Min(screen.WorkingArea.Width, 1920), Math.Min(screen.WorkingArea.Height, 1080));
                _dashBoard.Size = sdesiredSize;
                int smargin = (sdesiredSize.Width - _dashBoard.ClientSize.Width) / 2 - 1;
                _dashBoard.Location = new Point(screen.WorkingArea.X - smargin, screen.WorkingArea.Y);
                _dashBoard.Size = new Size(sdesiredSize.Width + 2 * smargin, sdesiredSize.Height + smargin);
            }
        }

        private void ToggleComponentActivation()
        {
            bool editable = _mode == OperationMode.Set;

            tbVehicleRefDataPath.Enabled = editable;
            tbVehicleHandEyeCalibFilePath.Enabled = editable;
            btnVehicleRefDataPath.Enabled = editable;
            btnVehicleHandEyeCalFileBrowser.Enabled = editable;
            tbGlassRefDataPath.Enabled = editable;
            tbGlassHandEyeCalibFilePath.Enabled = editable;
            btnGlassRefDataPath.Enabled = editable;
            btnGlassHandEyeCalibFilePath.Enabled = editable;
            tbGapRefDataPath.Enabled = editable;
            tbGapHandEyeCalibFilePath.Enabled = editable;
            btnGapRefDataPath.Enabled = editable;
            btnGapCalibPath.Enabled = editable;
            gbCarType_.Enabled = editable;
            btnGapManage0_.Enabled = editable;
            btnGapManage1_.Enabled = editable;
            btnGapManage2_.Enabled = editable;
            btnGapManage3_.Enabled = editable;

            btnSettingManager_.Enabled = _mode != OperationMode.Manual;
            btnCameraConnect_.Enabled = _mode != OperationMode.Auto;

            if (_config[-1].ScanInstallMode == ScanInstallMode.SAME_SCAN_INSTALL)
            {
                tbVehicleBirdEyeCalibFilePath.Enabled = false;
                btnVehicleBirdEyeCaliFileBrowser.Enabled = false;
            }
            else
            {
                tbVehicleBirdEyeCalibFilePath.Enabled = editable;
                btnVehicleBirdEyeCaliFileBrowser.Enabled = editable;
            }
        }

        private void FineLocalizerForm_Load(object sender, EventArgs e)
        {
            switch (_config.StartMode)
            {
                case OperationMode.Auto:
                    rbAuto_.Checked = true;
                    break;

                case OperationMode.Manual:
                    rbManual_.Checked = true;
                    break;

                case OperationMode.Set:
                    radioButtonMode_CheckedChanged(rbSet_, null);
                    break;
            }

            DisplayGlassVisionStatus(VisionStatus.NONE);
            DisplayVehicleVisionStatus(VisionStatus.NONE);
            DisplayGapVisionStatus(VisionStatus.NONE);

            pictureBoxGapCheckerGuide.ImageLocation = $"./Resources/gapCheckimg_{_config.InsertionPart.ToString().ToLower()}.png";

            if (_config.InsertionPart == GlassInsertionPart.FRONT)
            {
                lblGlassHandDirection1.Text = "RH";
                lblGlassHandDirection2.Text = "LH";
                lblVehicleHandDirection1.Text = "RH";
                lblVehicleHandDirection2.Text = "LH";
            }

            tabControl1_.SelectedTab = tabControl1_.TabPages["tabPageGlass_"];
            tabControl1_.SelectedTab = tabControl1_.TabPages["tabPageVehicle_"];

            _dashBoard?.Show();
        }
        
        private bool LoadRefPosesFile()
        {
            _refPoses = RefPosesFileManager.LoadFromFile($"{_config[-1].VehicleRefDataPath}/ref_pose.dat", _config[-1].VehicleCamera);
            if (_refPoses.Values == null)
            {
                Logger.Warning("!@LoadingVehicleRefPoseFailed");
                return false;
            }
            else
            {
                Logger.Info("!@LoadingVehicleRefPoseDone");
                return true;
            }
        }

        private void SetButtonsOnOff(int numberOfAvailablePoints)
        {
            for (var i = 0; i < numberOfAvailablePoints; ++i)
            {
                _btnPoints[i].Enabled = true;
            }

            for (var i = numberOfAvailablePoints; i < 4; ++i)
            {
                _btnPoints[i].Enabled = false;
            }
        }

        private void SetVehiclePointsOnOff(int numberOfPoints)
        {
            for (var i = 0; i < numberOfPoints; ++i)
            {
                _gbPoints[i].Visible = true;
            }

            for (var i = numberOfPoints; i < 4; ++i)
            {
                _gbPoints[i].Visible = false;
            }
        }

        private void InitVehicleUI()
        {
            foreach (var gb in _gbPoints)
            {
                ClearRobotPoseUi(gb);
            }
            ClearRobotPoseUi(gbSV_);

            _glManagerVehicle1?.Clear();
            _glManagerVehicle2?.Clear();

            Invoke(new MethodInvoker(() =>
            {
                btnUpdate_.Enabled = false;
                btnUpdate_.BackColor = Color.DarkGray;

                btnCalculate_.Enabled = false;
                btnCalculate_.BackColor = Color.DarkGray;

                DisplayVehicleVisionStatus(VisionStatus.NONE);

                if (_mode == OperationMode.Set)
                {
                    foreach (var btnPoint in _btnPoints)
                    {
                        btnPoint.Text = Lang.FineLo.btnVehicleSavePoints;
                        btnPoint.BackColor = Color.FromArgb(255,75,75);
                        btnPoint.ForeColor = Color.White;
                    }

                    SetButtonsOnOff(1);
                    SetVehiclePointsOnOff(4);
                }
                else
                {
                    foreach (var btnPoint in _btnPoints)
                    {
                        btnPoint.Text = Lang.FineLo.btnVehicleScanPoints;
                        btnPoint.BackColor = Color.FromArgb(80,215,73);
                        btnPoint.ForeColor = Color.White;
                    }

                    SetVehiclePointsOnOff(_refPoses.Values?.Count ?? 0);
                    if (_mode == OperationMode.Auto)
                    {
                        SetButtonsOnOff(0);
                    }
                    else
                    {
                        SetButtonsOnOff(_refPoses.Values?.Count ?? 0);
                    }
                }
            }));
        }

        private void FineLocalizerForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (MessageBox.Show("!@MainFormClosingCheck", "!@WarningTitle", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                _plcComm?.StopMonitoring();
                _plcComm?.StopHeartbeat();
                _melsecPlcDataTransmitter?.TerminateMelsecPlcDataProcAsync();
            }
            else
            {
                e.Cancel = true;
            }
        }

        private async void btnPointn_Click(object sender, EventArgs e)
        {
            var btnSender = sender as Button;
            btnSender.Enabled = false;

            if (!_isCameraConnected)
            {
                Logger.Warning("!@CameraConnectionNeeded");
                btnSender.Enabled = true;
                return;
            }

            if (!await _fineLocalizerVehicle.ReadyVehicleCheckerAsync())
            {
                Logger.Warning("!@ReadyVehicleError");
                return;
            }

            var name = btnSender.Name;
            var pointNum = Convert.ToInt32(name.Substring(name.Length - 2, 1));

            try
            {
                if (_mode == OperationMode.Set)
                {
                    await SaveCurrentRobotPoseAndImage(pointNum);
                }
                else
                {
                    await ScanPoint(pointNum);
                }

                if (_config.RobotConfigs[_config[-1].ScanRobot][RobotAttribute.Maker].ToEnum<RobotMaker>() != RobotMaker.HYUNDAI)
                {
                    await _fineLocalizerVehicle.ScanRobot.DisconnectAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{"!@PointScanError"} {ex.Message}", "!@WarningTitle");
            }
            finally
            {
                btnSender.Enabled = true;
            }
        }

        private async Task<bool> ScanPoint(int pointNum)
        {
            try
            {
                var curPose = await _fineLocalizerVehicle.ScanRobot.ReadRobotCurrentPoseAsync();
                if (curPose == null)
                {
                    Logger.Error("!@CantReadRobotCurrentPose");
                    return false;
                }
                UpdateRobotPoseToUI(curPose, _gbPoints[pointNum - 1]);

                bool retInfoCurr = _fineLocalizerVehicle.InfoVehicleScanPoseToEngine(pointNum, curPose);
                bool retInfoRef = _fineLocalizerVehicle.InfoVehicleScanRefPoseToEngine(pointNum, _refPoses.Values[pointNum - 1]);

                bool retCap = await _fineLocalizerVehicle.TriggerScanVehicleAsync(pointNum, false, false);

                if (retInfoCurr && retCap && retInfoRef)
                {
                    Invoke(new MethodInvoker(() =>
                    {
                        btnCalculate_.Enabled = _mode != OperationMode.Auto;
                        btnCalculate_.BackColor = Color.Lime;
                    }));
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"{"!@VehicleScanPointError"} ({ex})");
                return false;
            }
        }

        private async Task SaveCurrentRobotPoseAndImage(int pointNum)
        {
            bool saveImg = MessageBox.Show($"{"!@WantSaveRefImg"}", "", MessageBoxButtons.YesNo) == DialogResult.Yes;
            bool savePose = MessageBox.Show($"{"!@WantSaveRefPose"}", "", MessageBoxButtons.YesNo) == DialogResult.Yes;

            var curPose = await _fineLocalizerVehicle.ScanRobot.ReadRobotCurrentPoseAsync();
            UpdateRobotPoseToUI(curPose, _gbPoints[pointNum - 1]);

            if (await _fineLocalizerVehicle.TriggerScanVehicleAsync(pointNum, true, saveImg))
            {
                Logger.Info($"{"!@ScanningPointDone"} (point= {pointNum})");
                Invoke(new MethodInvoker(() => SetButtonsOnOff(pointNum + 1)));
            }
            else
            {
                Logger.Error($"{"!@ScanningPointFailed"} (point= {pointNum})");
                return;
            }

            if (savePose)
            {
                try
                {
                    if (!Directory.Exists(_config[-1].VehicleRefDataPath))
                    {
                        Directory.CreateDirectory(_config[-1].VehicleRefDataPath);
                    }

                    if (_refPoses.Values == null) _refPoses.Values = new List<RobotPose> { curPose };
                    else if (_refPoses.Values.Count < pointNum) _refPoses.Values.Add(curPose);
                    else _refPoses.Values[pointNum - 1] = curPose;

                    RefPosesFileManager.SaveToFile(_refPoses, $"{_config[-1].VehicleRefDataPath}/ref_pose.dat", _config[-1].VehicleCamera);

                    Logger.Info($"{"!@SavingRefPoseDone"} (point= {pointNum})");
                }
                catch (Exception ex)
                {
                    Logger.Error($"{"!@SavingRefPoseFailed"} (point= {pointNum}) <{ex.Message}>");
                }
            }
        }

        private async void btnUpdate_Click(object sender, EventArgs e)
        {
            btnUpdate_.Enabled = false;
            await UpdateInstallPosesToRobotAsync();
        }

        private async Task<bool> UpdateInstallPosesToRobotAsync()
        {
            RobotPoseVariable offset;
            if (!_config.RobotPoseVariables.TryGetValue($"{_config.RecentlyUsedCar}_vehicle_offset", out offset))
            {
                offset = new RobotPoseVariable();
            }

            bool ret = await _fineLocalizerVehicle.UpdateInstallPosesToRobot(_calPositions.Select(p => p + offset).ToList());

            Logger.Info($"{"!@CalculatedVehicleShiftValue"}: {_calPositions[0]}");
            Logger.Info($"{"!@VehicleOffsetValue"}: {offset}");
            Logger.Info($"{"!@ModifiedVehicleShiftValue"}: {_calPositions[0] + offset}");

            if (ret)
            {
                Logger.Info("!@UpdateInstallPosesToRobotDone");
            }
            else
            {
                Logger.Error("!@UpdateInstallPosesToRobotError");
            }

            return ret;
        }

        [Conditional("WITH_PLC")]
        private void ConnectPlc()
        {
            _setPlcFuncDict[_config.Plc]();
            RegisterPlcEventHandlers();
            if (_plcComm.Connect())
            {
                Logger.Info("!@PlcConnectDone");
            }
            else
            {
                Logger.Info("!@PlcConnectFailed");
            }
        }

        private Task ConnectPlcAsync()
        {
            return Task.Run(() => ConnectPlc());
        }

        [Conditional("WITH_PLC")]
        private void DisconnectPlc()
        {
            _plcComm.StopMonitoring();
            _plcComm.StopHeartbeat();

            if (_plcComm.Disconnect())
            {
                IsPLCConnected = false;
                ResetAllPlcSignalStatus();
                Logger.Info("!@PlcDisconnectDone");
            }
            else
            {
                Logger.Warning("!@PlcDisconnectFailed");
            }
        }

        private bool ValidateModeChange()
        {
            using (var f = new PasswordForm_())
            {
                var res = f.ShowDialog();
                if (res == DialogResult.Cancel)
                {
                    _isJustModeSelection = true;
                    rbAuto_.Checked = true;
                    return false;
                }
                else if (res == DialogResult.No)
                {
                    MessageBox.Show("!@ChangeModePasswordFailed", "!@WarningTitle");
                    _isJustModeSelection = true;
                    rbAuto_.Checked = true;
                    return false;
                }
            }

            return true;
        }

        private async void radioButtonMode_CheckedChanged(object sender, EventArgs e)
        {
            var rb = sender as RadioButton;
            if (!rb.Checked) return;

            if (_isJustModeSelection)
            {
                _isJustModeSelection = false;
                return;
            }

            if (_mode == OperationMode.Auto && ModifierKeys != Keys.Shift && !ValidateModeChange()) return;

            if (rb.Name == "rbSet_")
            {
                _mode = OperationMode.Set;

                _refPoses.Values = new List<RobotPose>();
                _refGlassPoses.Values = new List<RobotPose>();
                _refGapPoses.Values = new List<RobotPose>();

#if WITH_CAMERA
                if (_isCameraConnected)
                {
                    btnCameraConnect_Click(null, null);
                }
#endif

                if (_plcComm != null)
                {
                    DisconnectPlc();
                }

                if (_melsecPlcDataTransmitter != null)
                {
                    await _melsecPlcDataTransmitter.TerminateMelsecPlcDataProcAsync();
                }
            }
            else
            {
                LoadRefPosesFile();
                LoadRefGapPosesFile();
                LoadRefGlassPosesFile();

#if WITH_CAMERA
                if (!_isCameraConnected)
                {
                    btnCameraConnect_Click(null, null);
                }
#endif

                if (rb.Name == "rbAuto_")
                {
                    _mode = OperationMode.Auto;
                    
                    if (!IsPLCConnected)
                    {
                        await ConnectPlcAsync();
                    }

                    if (_melsecPlcDataTransmitter != null)
                    {
                        await _melsecPlcDataTransmitter.InitMelsecPlcDataProcessAsync();
                    }
                    
                }
                else
                {
                    _mode = OperationMode.Manual;

                    if (_plcComm != null)
                    {
                        DisconnectPlc();
                    }

                    if (_melsecPlcDataTransmitter != null)
                    {
                        await _melsecPlcDataTransmitter.TerminateMelsecPlcDataProcAsync();
                    }
                }
            }

            ToggleComponentActivation();
            InitVehicleUI();
            InitGapUI();
            InitGlassUI();
        }

        private void UpdateRobotPoseToUI(RobotPose pose, GroupBox gb, bool displayPositiveAngle = false)
        {
            if (gb.InvokeRequired)
            {
                Invoke(new MethodInvoker(() =>
                {
                    UpdateRobotPoseToUI_(pose, gb, displayPositiveAngle);
                }));
            }
            else
            {
                UpdateRobotPoseToUI_(pose, gb, displayPositiveAngle);
            }
        }

        private void UpdateRobotPoseToUI_(RobotPose pose, GroupBox gb, bool displayPositiveAngle)
        {
            // group box name == gbXX
            // names of text boxes in a group box == tbXXTx,tbXXTy,tbXXTz, tbXXRx,...
            var tbPrefix = $"tb{gb.Name.Substring(2, gb.Name.Length - 3)}";
            gb.Controls[$"{tbPrefix}Tx"].Text = $"{pose.Tx:F3}";
            gb.Controls[$"{tbPrefix}Ty"].Text = $"{pose.Ty:F3}";
            gb.Controls[$"{tbPrefix}Tz"].Text = $"{pose.Tz:F3}";

            double rx = pose.Rx;
            double ry = pose.Ry;
            double rz = pose.Rz;

            if (displayPositiveAngle)
            {
                rx = (rx > 179) ? rx - 180 : rx;
                ry = (ry > 179) ? ry - 180 : ry;
                rz = (rz > 179) ? rz - 180 : rz;
            }

            gb.Controls[$"{tbPrefix}Rx"].Text = $"{rx:F3}";
            gb.Controls[$"{tbPrefix}Ry"].Text = $"{ry:F3}";
            gb.Controls[$"{tbPrefix}Rz"].Text = $"{rz:F3}";
        }

        private void UpdateGlassShiftValueToUi(RobotPose pose, GroupBox gb)
        {
            if (gb.InvokeRequired)
            {
                Invoke(new MethodInvoker(() =>
                {
                    UpdateGlassShiftValueToUi_(pose, gb);
                }));
            }
            else
            {
                UpdateGlassShiftValueToUi_(pose, gb);
            }
        }

        private void UpdateGlassShiftValueToUi_(RobotPose pose, GroupBox gb)
        {
            var textboxPrefix = $"tb{gb.Name.Substring(2, gb.Name.Length - 3)}";
            TextBox tbTx = gb.Controls[$"{textboxPrefix}Tx"] as TextBox;
            TextBox tbTy = gb.Controls[$"{textboxPrefix}Ty"] as TextBox;

            tbTx.BackColor = DefaultBackColor;
            tbTy.BackColor = DefaultBackColor;

            if (_config[-1].UpdateToVehicleShift)
            {
                tbTx.ForeColor = Color.Blue;
                tbTy.ForeColor = Color.Blue;
            }
            else
            {
                tbTx.ForeColor = Color.Black;
                tbTy.ForeColor = Color.Black;
            }

            tbTx.Text = $"{pose.Tx:F3}";
            tbTy.Text = $"{pose.Ty:F3}";
        }

        private void ClearRobotPoseUi(GroupBox gb)
        {
            if (gb.InvokeRequired)
            {
                Invoke(new MethodInvoker(() =>
                {
                    ClearRobotPoseUi_(gb);
                }));
            }
            else
            {
                ClearRobotPoseUi_(gb);
            }
        }

        private void ClearRobotPoseUi_(GroupBox gb)
        {
            var tbPrefix = $"tb{gb.Name.Substring(2, gb.Name.Length - 3)}";
            gb.Controls[$"{tbPrefix}Tx"].Text = "";
            gb.Controls[$"{tbPrefix}Ty"].Text = "";
            gb.Controls[$"{tbPrefix}Tz"].Text = "";
            gb.Controls[$"{tbPrefix}Rx"].Text = "";
            gb.Controls[$"{tbPrefix}Ry"].Text = "";
            gb.Controls[$"{tbPrefix}Rz"].Text = "";
        }

        private void ClearGlassShiftValueUi(GroupBox gb)
        {
            if (gb.InvokeRequired)
            {
                Invoke(new MethodInvoker(() =>
                {
                    ClearGlassShiftValueUi_(gb);
                }));
            }
            else
            {
                ClearGlassShiftValueUi_(gb);
            }
        }

        private void ClearGlassShiftValueUi_(GroupBox gb)
        {
            var tbPrefix = $"tb{gb.Name.Substring(2, gb.Name.Length - 3)}";
            gb.Controls[$"{tbPrefix}Tx"].Text = "";
            gb.Controls[$"{tbPrefix}Ty"].Text = "";
        }

        [Conditional("WITH_DLL")]
        private void InfoSettingsToEngine()
        {
            Logger.Debug("InfoSettingsToEngine() is called!!!");
            try
            {
                foreach (int carType in _config.GetCarTypeList())
                {
                    var scanRobotMaker = _config.RobotConfigs[_config[carType].ScanRobot][RobotAttribute.Maker].ToEnum<RobotMaker>();
                    var installRobotMaker = _config.RobotConfigs[_config[carType].InstallRobot][RobotAttribute.Maker].ToEnum<RobotMaker>();

                    if (_config[carType].UseGlassChecker)
                    {
                        var glassCamConfigs = _config.CameraConfigs[_config[carType].GlassCamera];
                        FineLocalizerVehicleEngineAPI.InfoGlassScanCamSettings(carType, glassCamConfigs.Count,
                                                                               glassCamConfigs.Select(c => new CamSettings(c)).ToArray());
                        FineLocalizerVehicleEngineAPI.InfoGlassCheckerSettings(carType, new GlassCheckerSettings(_config[carType], installRobotMaker));
                    }

                    var vehicleCamConfigs = _config.CameraConfigs[_config[carType].VehicleCamera];
                    FineLocalizerVehicleEngineAPI.InfoVehicleScanCamSettings(carType, vehicleCamConfigs.Count,
                                                                             vehicleCamConfigs.Select(c => new CamSettings(c)).ToArray());
                    FineLocalizerVehicleEngineAPI.InfoVehicleAlignerSettings(carType, new VehicleAlignerSettings(_config[carType],
                                                                                                                 scanRobotMaker, installRobotMaker));

                    if (_config[carType].UseGapChecker)
                    {
                        var gapCamConfigs = _config.CameraConfigs[_config[carType].GapCamera];
                        FineLocalizerVehicleEngineAPI.InfoGapScanCamSettings(carType, gapCamConfigs.Count,
                                                                             gapCamConfigs.Select(c => new CamSettings(c)).ToArray());
                        FineLocalizerVehicleEngineAPI.InfoGapCheckerSettings(carType, new GapCheckerSettings(_config[carType], installRobotMaker));
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Warning($"{"!@InfoSettingsFailed"} ({ex})");
            }
        }

        private async Task<bool> CalculatePoses()
        {
            try
            {
                var ret = await _fineLocalizerVehicle.CalculatePosesAsync(_config[-1].VisualizationXAverage,
                                                                          _config[-1].VisualizationYAverage,
                                                                          _config[-1].VisualizationZAverage,
                                                                          _config[-1].VisualizationZMax);

                if (ret.target1.Length > 0 && ret.source1.Length > 0 && ret.sourceAligned1.Length > 0)
                {
                    _glManagerVehicle1.UpdatePointCloud(ret.target1, ret.source1, ret.sourceAligned1,
                                                        _config[-1].VisualizationZAverage,
                                                        _config[-1].VisualizationZMin,
                                                        _config[-1].VisualizationZMax);
                }

                if (ret.target2.Length > 0 && ret.source2.Length > 0 && ret.sourceAligned2.Length > 0)
                {
                    _glManagerVehicle2.UpdatePointCloud(ret.target2, ret.source2, ret.sourceAligned2,
                                                        _config[-1].VisualizationZAverage,
                                                        _config[-1].VisualizationZMin,
                                                        _config[-1].VisualizationZMax);
                }

                _calPositions = ret.calculatedPoses;

                for (var i = 0; i < _calPositions.Count; ++i)
                {
                    Logger.Info($"{"!@CalculatedInstallPose"} : [{i + 1}] {_calPositions[i]}");
                }

                UpdateRobotPoseToUI(_calPositions[0], gbSV_);
                Logger.WriteVehicleShiftValue(_currentCar, _config[-1].CarName, _currentCarSeq, -1, ret.isOK, _calPositions[0],
                                              _config.RobotPoseVariables.TryGetValue($"{_currentCar}_vehicle_offset", out var offset) ? offset : new RobotPoseVariable());

                if (ret.isOK)
                {
                    btnUpdate_.Enabled = _mode != OperationMode.Auto;
                    btnUpdate_.BackColor = Color.Lime;

                    Logger.Info("!@VehicleCalculationDone");
                }
                else
                {
                    Logger.Error("!@VehicleCalculationFailed");
                }

                return ret.isOK;
            }
            catch (Exception ex)
            {
                Logger.Error($"{"!@VehicleCalculationFailed"} ({ex})");
                return false;
            }
        }

        private async void btnCalculate_Click(object sender, EventArgs e)
        {
            btnCalculate_.Enabled = false;

            if (await CalculatePoses())
            {
                Invoke(new MethodInvoker(() => DisplayVehicleVisionStatus(VisionStatus.OK)));
                Logger.CaptureFineLocalizer(VisionStatus.OK, "vehicle", _config[-1].CarName, $"{_currentCarSeq}");
            }
            else
            {
                Logger.Error("!@VehicleCalculationFailed");
                Invoke(new MethodInvoker(() => DisplayVehicleVisionStatus(VisionStatus.NG)));
                Logger.CaptureFineLocalizer(VisionStatus.NG, "vehicle", _config[-1].CarName, $"{_currentCarSeq}");
                return;
            }

            if (_config[-1].UseGapChecker)
            {
                if ((_refGapPoses.Values?.Count ?? 0) < 1)
                {
                    Logger.Error("!@ReadingGapCheckRefPosesFailed");
                    return;
                }

                var ret = await _gapChecker.EstimateAndUpdateGapScanPosesAsync();
                if (!ret.isSuccess)
                {
                    Logger.Error("!@GapCheckFailed");
                }
                UpdateRobotPoseToUI(ret.shiftValue, gbGapSV_);
            }
        }

        private void groupBoxPLC_Paint(object sender, PaintEventArgs e)
        {
            _plcStatusPainter.DrawDefaultSketch();

            if (IsPLCConnected)
            {
                _plcStatusPainter.DrawConnStatus(PlcStatus.ON);
            }
        }

        private void SaveConfig()
        {
            try
            {
                ConfigFileManager.SaveToFile(_config, ConfigFileManager.GetConfigFilePath());
            }
            catch (Exception ex)
            {
                Logger.WriteLog(ex.Message);
            }
        }

        private void OnBtnBrowseFolderPath(TextBox tb)
        {
            try
            {
                folderBrowserDialog1.SelectedPath = Path.GetFullPath(tb.Text);
            }
            catch
            {
            }

            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                tb.Text = folderBrowserDialog1.SelectedPath;
                tbFolderPathConf_Leave(tb, null);
            }
        }

        private void btnBirdEyeCaliFileBrowser_Click(object sender, EventArgs e)
        {
            try
            {
                openFileDialog1.FileName = Path.GetFullPath(tbVehicleBirdEyeCalibFilePath.Text);
            }
            catch
            {
            }

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                tbVehicleBirdEyeCalibFilePath.Text = openFileDialog1.FileName;
                tbStringSpecificConf_Leave(tbVehicleBirdEyeCalibFilePath, null);
            }
        }

        private bool CheckIfDataSourceIsChanging(object control)
        {
            string name = (control as Control).Name;
            if (_isDataSourceChanging[name])
            {
                _isDataSourceChanging[name] = false;
                return true;
            }
            else
            {
                return false;
            }
        }

        private void comboBoxCarType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (CheckIfDataSourceIsChanging(sender)) return;

            _currentCar = Convert.ToInt32(comboBoxCarType.SelectedItem);
            _config.RecentlyUsedCar = _currentCar;
            Invoke(new MethodInvoker(() => _dashBoard?.LoadCarInfo(_currentCar.ToString(), _config[-1].CarName)));

            SaveConfig();
            UpdateUIFromConfig();

            if (_config.RobotConfigs.ContainsKey(_config[-1].InstallRobot ?? ""))
            {
                if (_config.RobotConfigs.ContainsKey(_config[-1].ScanRobot ?? ""))
                {
                    if (_fineLocalizerVehicle == null)
                    {
                        _fineLocalizerVehicle = new FineLocalizerVehicle(_config.RobotConfigs[_config[-1].InstallRobot],
                                                                         _config.RobotConfigs[_config[-1].ScanRobot]);
                    }
                    else
                    {
                        _fineLocalizerVehicle.ConfigureInstallRobot(_config.RobotConfigs[_config[-1].InstallRobot]);
                        _fineLocalizerVehicle.ConfigureScanRobot(_config.RobotConfigs[_config[-1].ScanRobot]);
                    }
                }

                if (_gapChecker == null)
                {
                    _gapChecker = new GapChecker(_config.RobotConfigs[_config[-1].InstallRobot]);
                }
                else
                {
                    _gapChecker.ConfigureRobot(_config.RobotConfigs[_config[-1].InstallRobot]);
                }

                if (_glassChecker == null)
                {
                    _glassChecker = new GlassChecker(_config.RobotConfigs[_config[-1].InstallRobot]);
                }
                else
                {
                    _glassChecker.ConfigureRobot(_config.RobotConfigs[_config[-1].InstallRobot]);
                }
            }

            if (_mode == OperationMode.Auto)
            {
                LoadRefGlassPosesFile();
                bool isVehicleRefPosesLoaded = LoadRefPosesFile();
                bool isGapRefPosesLoaded = LoadRefGapPosesFile();

                SetVehiclePointsOnOff(isVehicleRefPosesLoaded ? _refPoses.Values.Count : 0);
                SetGapPointsOnOff(isGapRefPosesLoaded ? _refGapPoses.Values.Count : 0);

                SetButtonsOnOff(0);
                SetGapButtonsOnOff(0);
                btnGlassPoint_.Enabled = false;
            }
            else
            {
                SetButtonsOnOff(1);
                SetGapButtonsOnOff(1);
                btnGlassPoint_.Enabled = true;
            }

            FineLocalizerVehicleEngineAPI.InfoCarType(_currentCar);
        }

        private async void btnCameraConnect_Click(object sender, EventArgs e)
        {
#if WITH_CAMERA
            if (_isCameraConnecting) return;
            _isCameraConnecting = true;
            btnCameraConnect_.Enabled = false;
            if (_isCameraConnected)
            {
                try
                {
                    if (FineLocalizerVehicleEngineAPI.DisconnectCameras())
                    {
                        _isCameraConnected = false;
                        btnCameraConnect_.Text = Lang.FineLo.btnCameraConnect_;
                        Logger.Info("!@CameraDisconnectionDone");
                    }
                    else
                    {
                        Logger.Warning("!@CameraDisconnectionFailed");
                    }
                }
                catch (Exception ex)
                {
                    Logger.Warning($"{"!@CameraDisconnectionFailed"} ({ex.Message})");
                }
            }
            else
            {
                Logger.Info("!@WaitingForCameraConnection");
                if ((_config[-1].UseGapChecker && !_config.CameraConfigs.ContainsKey(_config[-1].GapCamera ?? "")) ||
                    !_config.CameraConfigs.ContainsKey(_config[-1].VehicleCamera ?? "") ||
                    (_config[-1].UseGlassChecker && !_config.CameraConfigs.ContainsKey(_config[-1].GlassCamera ?? "")))
                {
                    Logger.Warning("!@CameraInfoNotSet");
                    btnCameraConnect_.Enabled = _mode != OperationMode.Auto;
                    _isCameraConnecting = false;
                    return;
                }

                try
                {
                    if (await Task.Run(() => FineLocalizerVehicleEngineAPI.ConnectCameras()))
                    {
                        _isCameraConnected = true;
                        btnCameraConnect_.Text = Lang.FineLo.btnCameraDisconnect_;
                        Logger.Info("!@CameraConnectionDone");
                    }
                    else
                    {
                        Logger.Warning("!@CameraConnectionFailed");
                    }
                }
                catch (Exception ex)
                {
                    Logger.Warning($"{"!@CameraConnectionFailed"} ({ex.Message})");
                }
            }

            btnCameraConnect_.Enabled = _mode != OperationMode.Auto;
            _isCameraConnecting = false;
#endif
        }

        #region OpenGL Control handlers
        private void glControl1_ContextCreated(object sender, GlControlEventArgs e)
        {
            _glManagerVehicle1 = new GlDisplayManager(glControlVehicle1);
        }

        private void glControl1_Render(object sender, GlControlEventArgs e)
        {
            _glManagerVehicle1.Render();
        }

        private void glControl1_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.R:
                _glManagerVehicle1.Reset();
                break;

                case Keys.O:
                _glManagerVehicle1.AutoRotate = false;
                _glManagerVehicle1.Scale -= 0.1;
                break;

                case Keys.I:
                _glManagerVehicle1.AutoRotate = false;
                _glManagerVehicle1.Scale += 0.1;
                break;

                case Keys.A:
                _glManagerVehicle1.AutoRotate = !_glManagerVehicle1.AutoRotate;
                break;

                case Keys.C:
                _glManagerVehicle1.DisplayColor = !_glManagerVehicle1.DisplayColor;
                break;

                default:
                return;
            }

            glControlVehicle1.Invalidate();
        }

        private void glControl1_MouseDown(object sender, MouseEventArgs e)
        {
            _glManagerVehicle1.PrevMousePos = e.Location;
        }

        private void glControl1_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _glManagerVehicle1.Rotate(e.X, e.Y);
            }
            else if (e.Button == MouseButtons.Right)
            {
                _glManagerVehicle1.Move(e.X, e.Y);
            }
            else
            {
                return;
            }

            glControlVehicle1.Invalidate();
        }

        private void glControl1_MouseWheel(object sender, MouseEventArgs e)
        {
            if (e.Delta > 0)
            {
                _glManagerVehicle1.AutoRotate = false;
                _glManagerVehicle1.Scale += 0.5;
            }
            else
            {
                _glManagerVehicle1.Scale -= 0.5;
            }

            glControlVehicle1.Invalidate();
        }

        private void glControl1_ContextDestroying(object sender, GlControlEventArgs e)
        {
        }

        private void glControl1_DoubleClick(object sender, EventArgs e)
        {
            glControlVehicle1.BringToFront();
            glControlVehicle1.Dock = _isVehicleGlMax ? DockStyle.None : DockStyle.Fill;
            _isVehicleGlMax = !_isVehicleGlMax;
        }

        private void glControlVehicle2_ContextCreated(object sender, GlControlEventArgs e)
        {
            _glManagerVehicle2 = new GlDisplayManager(glControlVehicle2);
        }

        private void glControlVehicle2_ContextDestroying(object sender, GlControlEventArgs e)
        {
        }

        private void glControlVehicle2_Render(object sender, GlControlEventArgs e)
        {
            _glManagerVehicle2.Render();
        }

        private void glControlVehicle2_MouseDown(object sender, MouseEventArgs e)
        {
            _glManagerVehicle2.PrevMousePos = e.Location;
        }

        private void glControlVehicle2_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _glManagerVehicle2.Rotate(e.X, e.Y);
            }
            else if (e.Button == MouseButtons.Right)
            {
                _glManagerVehicle2.Move(e.X, e.Y);
            }
            else
            {
                return;
            }

            glControlVehicle2.Invalidate();
        }

        private void glControlVehicle2_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.R:
                _glManagerVehicle2.Reset();
                break;

                case Keys.O:
                _glManagerVehicle2.AutoRotate = false;
                _glManagerVehicle2.Scale -= 0.1;
                break;

                case Keys.I:
                _glManagerVehicle2.AutoRotate = false;
                _glManagerVehicle2.Scale += 0.1;
                break;

                case Keys.A:
                _glManagerVehicle2.AutoRotate = !_glManagerVehicle2.AutoRotate;
                break;

                case Keys.C:
                _glManagerVehicle2.DisplayColor = !_glManagerVehicle2.DisplayColor;
                break;

                default:
                return;
            }

            glControlVehicle2.Invalidate();
        }

        private void glControlVehicle2_DoubleClick(object sender, EventArgs e)
        {
            glControlVehicle2.BringToFront();
            glControlVehicle2.Dock = _isVehicleGlMax ? DockStyle.None : DockStyle.Fill;
            _isVehicleGlMax = !_isVehicleGlMax;
        }

        private void glControlVehicle2_MouseWheel(object sender, MouseEventArgs e)
        {
            if (e.Delta > 0)
            {
                _glManagerVehicle2.AutoRotate = false;
                _glManagerVehicle2.Scale += 0.5;
            }
            else
            {
                _glManagerVehicle2.Scale -= 0.5;
            }

            glControlVehicle2.Invalidate();
        }

        private void glControlGlass1_ContextCreated(object sender, GlControlEventArgs e)
        {
            _glManagerGlass1 = new GlDisplayManager(glControlGlass1);
        }

        private void glControlGlass1_ContextDestroying(object sender, GlControlEventArgs e)
        {
        }

        private void glControlGlass1_Render(object sender, GlControlEventArgs e)
        {
            _glManagerGlass1.Render();
        }

        private void glControlGlass1_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.R:
                _glManagerGlass1.Reset();
                break;

                case Keys.O:
                _glManagerGlass1.AutoRotate = false;
                _glManagerGlass1.Scale -= 0.1;
                break;

                case Keys.I:
                _glManagerGlass1.AutoRotate = false;
                _glManagerGlass1.Scale += 0.1;
                break;

                case Keys.A:
                _glManagerGlass1.AutoRotate = !_glManagerGlass1.AutoRotate;
                break;

                case Keys.C:
                _glManagerGlass1.DisplayColor = !_glManagerGlass1.DisplayColor;
                break;

                default:
                return;
            }

            glControlGlass1.Invalidate();
        }

        private void glControlGlass1_MouseDown(object sender, MouseEventArgs e)
        {
            _glManagerGlass1.PrevMousePos = e.Location;
        }

        private void glControlGlass1_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _glManagerGlass1.Rotate(e.X, e.Y);
            }
            else if (e.Button == MouseButtons.Right)
            {
                _glManagerGlass1.Move(e.X, e.Y);
            }
            else
            {
                return;
            }

            glControlGlass1.Invalidate();
        }

        private void glControlGlass1_MouseWheel(object sender, MouseEventArgs e)
        {
            if (e.Delta > 0)
            {
                _glManagerGlass1.AutoRotate = false;
                _glManagerGlass1.Scale += 0.5;
            }
            else
            {
                _glManagerGlass1.Scale -= 0.5;
            }

            glControlGlass1.Invalidate();
        }

        private void glControlGlass2_ContextCreated(object sender, GlControlEventArgs e)
        {
            _glManagerGlass2 = new GlDisplayManager(glControlGlass2);
        }

        private void glControlGlass2_ContextDestroying(object sender, GlControlEventArgs e)
        {
        }

        private void glControlGlass2_Render(object sender, GlControlEventArgs e)
        {
            _glManagerGlass2.Render();
        }

        private void glControlGlass2_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.R:
                _glManagerGlass2.Reset();
                break;

                case Keys.O:
                _glManagerGlass2.AutoRotate = false;
                _glManagerGlass2.Scale -= 0.1;
                break;

                case Keys.I:
                _glManagerGlass2.AutoRotate = false;
                _glManagerGlass2.Scale += 0.1;
                break;

                case Keys.A:
                _glManagerGlass2.AutoRotate = !_glManagerGlass2.AutoRotate;
                break;

                case Keys.C:
                _glManagerGlass2.DisplayColor = !_glManagerGlass2.DisplayColor;
                break;

                default:
                return;
            }

            glControlGlass2.Invalidate();
        }

        private void glControlGlass2_MouseDown(object sender, MouseEventArgs e)
        {
            _glManagerGlass2.PrevMousePos = e.Location;
        }

        private void glControlGlass2_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _glManagerGlass2.Rotate(e.X, e.Y);
            }
            else if (e.Button == MouseButtons.Right)
            {
                _glManagerGlass2.Move(e.X, e.Y);
            }
            else
            {
                return;
            }

            glControlGlass2.Invalidate();
        }

        private void glControlGlass2_MouseWheel(object sender, MouseEventArgs e)
        {
            if (e.Delta > 0)
            {
                _glManagerGlass2.AutoRotate = false;
                _glManagerGlass2.Scale += 0.5;
            }
            else
            {
                _glManagerGlass2.Scale -= 0.5;
            }

            glControlGlass2.Invalidate();
        }
#endregion

#region UI Components about configuration event handlers

        private void tbFolderPathConf_Leave(object sender, EventArgs e)
        {
            TextBox tb = sender as TextBox;
            var magicRule = new FileSystemAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null),
                                                     FileSystemRights.FullControl,
                                                     InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
                                                     PropagationFlags.None,
                                                     AccessControlType.Allow);

            try
            {
                if (!Directory.Exists(tb.Text))
                {
                    var ds = new DirectorySecurity();
                    ds.AddAccessRule(magicRule);
                    Directory.CreateDirectory(tb.Text, ds);
                }
                else
                {
                    var secu = new DirectoryInfo(tb.Text).GetAccessControl();
                    bool hasPermission = false;
                    foreach (FileSystemAccessRule rule in secu.GetAccessRules(true, true, typeof(SecurityIdentifier)))
                    {
                        if (rule.IdentityReference == new SecurityIdentifier(WellKnownSidType.WorldSid, null))
                        {
                            hasPermission = true;
                            break;
                        }
                    }
                    if (!hasPermission)
                    {
                        secu.AddAccessRule(magicRule);
                        Directory.SetAccessControl(tb.Text, secu);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{Lang.MsgBoxFineLo.InvalidPath} {ex}", Lang.MsgBoxFineLo.WarningTitle);
                tb.Text = _sConfAcc.Get(_config[-1], tb.Name.Substring(2)).ToString();
                return;
            }

            tbStringSpecificConf_Leave(sender, e);
        }

        private void tbStringSpecificConf_Leave(object sender, EventArgs e)
        {
            TextBox tb = sender as TextBox;
            var propertyName = tb.Name.Substring(2);
            if (_sConfAcc.Get(_config[-1], propertyName) as string != tb.Text)
            {
                _sConfAcc.Set(_config[-1], propertyName, tb.Text);
                SaveConfig();
                InfoSettingsToEngine();
            }
        }
#endregion

        private void SelectLanguage(CultureInfo culture)
        {
            Thread.CurrentThread.CurrentUICulture = culture;
            Logger.ProcessName = Lang.FineLo.ResourceManager.GetString("FineLocalizerForm_");
            this.Translate(Lang.FineLo.ResourceManager);
        }

        private async void btnOption_click(object sender, EventArgs e)
        {
            var openForm = Application.OpenForms["FineLocalizerSettingManagerForm_"];
            if (openForm != null)
            {
                Invoke(new MethodInvoker(() => openForm.Activate()));
                return;
            }

            var configCandidate = ConfigFileManager.LoadFromFile<Config<FineLocalizerConfig>>(ConfigFileManager.GetConfigFilePath());
            var form = new FineLocalizerSettingManagerForm_(configCandidate, _mode, c => configCandidate = c);

            if (_mode == OperationMode.Auto)
            {
                await Task.Run(() => form.ShowDialog());
            }
            else
            {
                form.ShowDialog();
            }

            if (form.DialogResult == DialogResult.OK) 
            {
                _config = configCandidate;
                SaveConfig();

                if (_mode == OperationMode.Set)
                {
                    SetComboBoxDataSource(comboBoxCarType, _config.GetCarTypeList());
                    if (!_config.GetCarTypeList().Contains(_config.RecentlyUsedCar))
                    {
                        _config.RecentlyUsedCar = Convert.ToInt32(comboBoxCarType.SelectedItem);
                    }
                    comboBoxCarType.SelectItemAndFireEventUnconditionally(_config.RecentlyUsedCar, comboBoxCarType_SelectedIndexChanged);

                    InfoSettingsToEngine();
                    RefPosesFileManager.LoadCamSettings();
                    Logger.LogPath = _config.LogPath;
                }
                else
                {
                    _isConfigModified = true;
                    Logger.Debug($"Settings are saved. {_isConfigModified}");
                }

                Logger.FileLoglevelFrom = _config.MinimumFileLogLevel;
                Logger.GuiLoglevelFrom = _config.MinimumUiLogLevel;
            }

            if (_mode == OperationMode.Set && !string.IsNullOrEmpty(form.ScheduleValues))
            {
                string[] values = new Regex(@"[ ](?=(?:[^""]*""[^""]*"")*[^""]*$)").Split(form.ScheduleValues);
                if (values[1] != Path.GetFullPath(_config.LogPath))
                {
                    await CoPick.Logging.TaskSchedulerManager.AddDailyTaskScheduleAsync(
                        values[0], Path.GetFullPath(@".\LogManager.exe"), _config.LogPath,
                        0, Convert.ToInt32(values[3]),
                        0, Convert.ToInt32(values[5]),
                        0, Convert.ToInt32(values[7]),
                        DateTime.ParseExact(values[8], "HH:mm", Thread.CurrentThread.CurrentCulture));
                }
            }
        }

        private void DisplayGlassVisionStatus(VisionStatus status)
        {
            switch (status)
            {
                case VisionStatus.OK:
                    pBoxOKGlass.Image = Properties.Resources.visionOKOn;
                    pBoxNGGlass.Image = Properties.Resources.visionNGOff;
                    break;

                case VisionStatus.NG:
                    pBoxOKGlass.Image = Properties.Resources.visionOKOff;
                    pBoxNGGlass.Image = Properties.Resources.visionNGOn;
                    _ngListView.ReportAndSaveNgInfo(_config[-1].CarName, _currentCarSeq, TaskStage.Glass);
                    _dashBoard?.AddNgListViewItem(_config[-1].CarName, _currentCarSeq, TaskStage.Glass);
                    break;
                    
                case VisionStatus.NONE:
                    pBoxOKGlass.Image = Properties.Resources.visionOKOff;
                    pBoxNGGlass.Image = Properties.Resources.visionNGOff;
                    break;
            }
            _dashBoard?.DisplayGlassVisionDashboard(status);
        }

        private void DisplayVehicleVisionStatus(VisionStatus status)
        {
            switch (status)
            {
                case VisionStatus.OK:
                    pBoxOKVehicle.Image = Properties.Resources.visionOKOn;
                    pBoxNGVehicle.Image = Properties.Resources.visionNGOff;
                    break;

                case VisionStatus.NG:
                    pBoxOKVehicle.Image = Properties.Resources.visionOKOff;
                    pBoxNGVehicle.Image = Properties.Resources.visionNGOn;
                    _ngListView.ReportAndSaveNgInfo(_config[-1].CarName, _currentCarSeq, TaskStage.Vehicle);
                    _dashBoard?.AddNgListViewItem(_config[-1].CarName, _currentCarSeq, TaskStage.Vehicle);
                    break;
                    
                case VisionStatus.NONE:
                    pBoxOKVehicle.Image = Properties.Resources.visionOKOff;
                    pBoxNGVehicle.Image = Properties.Resources.visionNGOff;
                    break;
            }
            _dashBoard?.DisplayVehicleVisionDashboard(status);
        }

        private void DisplayGapVisionStatus(VisionStatus status)
        {
            switch (status)
            {
                case VisionStatus.OK:
                    pBoxOKGap.Image = Properties.Resources.visionOKOn;
                    pBoxNGGap.Image = Properties.Resources.visionNGOff;
                    break;

                case VisionStatus.NG:
                    pBoxOKGap.Image = Properties.Resources.visionOKOff;
                    pBoxNGGap.Image = Properties.Resources.visionNGOn;
                    _ngListView.ReportAndSaveNgInfo(_config[-1].CarName, _currentCarSeq, TaskStage.Gap);
                    _dashBoard?.AddNgListViewItem(_config[-1].CarName, _currentCarSeq, TaskStage.Gap);
                    break;
                    
                case VisionStatus.NONE:
                    pBoxOKGap.Image = Properties.Resources.visionOKOff;
                    pBoxNGGap.Image = Properties.Resources.visionNGOff;
                    break;
            }
            _dashBoard?.DisplayGapVisionDashboard(status);
        }

        private void pictureBoxGap1_Paint(object sender, PaintEventArgs e)
        {
            Color c = Color.Green;
            int w = 1;
            ControlPaint.DrawBorder(e.Graphics, pictureBoxGap1.ClientRectangle, c, w, ButtonBorderStyle.Solid,
                                    c, w, ButtonBorderStyle.Solid, c, w, ButtonBorderStyle.Solid,
                                    c, w, ButtonBorderStyle.Solid);
        }

        private void pictureBoxGap2_Paint(object sender, PaintEventArgs e)
        {
            Color c = Color.Green;
            int w = 1;
            ControlPaint.DrawBorder(e.Graphics, pictureBoxGap2.ClientRectangle, c, w, ButtonBorderStyle.Solid,
                                    c, w, ButtonBorderStyle.Solid, c, w, ButtonBorderStyle.Solid,
                                    c, w, ButtonBorderStyle.Solid);
        }

        private void pictureBoxGap3_Paint(object sender, PaintEventArgs e)
        {
            Color c = Color.Green;
            int w = 1;
            ControlPaint.DrawBorder(e.Graphics, pictureBoxGap3.ClientRectangle, c, w, ButtonBorderStyle.Solid,
                                    c, w, ButtonBorderStyle.Solid, c, w, ButtonBorderStyle.Solid,
                                    c, w, ButtonBorderStyle.Solid);
        }

        private void pictureBoxGap4_Paint(object sender, PaintEventArgs e)
        {
            Color c = Color.Green;
            int w = 1;
            ControlPaint.DrawBorder(e.Graphics, pictureBoxGap4.ClientRectangle, c, w, ButtonBorderStyle.Solid,
                                    c, w, ButtonBorderStyle.Solid, c, w, ButtonBorderStyle.Solid,
                                    c, w, ButtonBorderStyle.Solid);
        }

        private void btnSetGlassOffset__Click(object sender, EventArgs e)
        {
            var openForm = Application.OpenForms["GlassOffsetForm_"];
            if (openForm != null)
            {
                Invoke(new MethodInvoker(()=>openForm.Activate()));
                return;
            }

            Task.Run(() =>
            {
                using (var f = new GlassOffsetForm_(_config))
                {
                    if (f.ShowDialog() == DialogResult.OK)
                    {
                        UpdateGlassShiftValueToUi(_config.RobotPoseVariables[$"{_config.RecentlyUsedCar}_glass_offset"], gbGlassOffset_);
                        SaveConfig();
                    }
                }
            });
        }

        private void btnSetVehicleOffset__Click(object sender, EventArgs e)
        {
            var openForm = Application.OpenForms["VehicleOffsetForm_"];
            if (openForm != null) 
            {
                Invoke(new MethodInvoker(() => openForm.Activate()));
                return;
            }

            Task.Run(() =>
            {
                using (var f = new VehicleOffsetForm_(_config))
                {
                    if (f.ShowDialog() == DialogResult.OK)
                    {
                        UpdateRobotPoseToUI(_config.RobotPoseVariables[$"{_config.RecentlyUsedCar}_vehicle_offset"], gbVehicleOffset_);
                        SaveConfig();
                    }
                }
            });
        }

        private void btnGapTeach_Click(object sender, EventArgs e)
        {
            var btn = sender as Button;
            int idx = Convert.ToInt32(btn.Name[11]) - 48;
            if (FineLocalizerVehicleEngineAPI.TeachGap(idx))
            {
                Logger.Info($"{"!@GapTeachCompleted"} (idx= {idx})");
            }
            else
            {
                Logger.Warning($"{"!@GapTeachFailed"} (idx= {idx})");
            }
        }

        private void btnGapAdd_Click(object sender, EventArgs e)
        {
            var btn = sender as Button;
            int idx = Convert.ToInt32(btn.Name[9]) - 48;
            if (FineLocalizerVehicleEngineAPI.AddGapData(idx))
            {
                Logger.Info($"{"!@GapAddCompleted"} (idx= {idx})");
            }
            else
            {
                Logger.Warning($"{"!@GapAddFailed"} (idx= {idx})");
            }
        }

        private void btnGapManage_Click(object sender, EventArgs e)
        {
            var btn = sender as Button;
            int idx = Convert.ToInt32(btn.Name[12]) - 48;
            if (FineLocalizerVehicleEngineAPI.ManageGapData(idx))
            {
                Logger.Info($"{"!@GapManageCompleted"} (idx= {idx})");
            }
            else
            {
                Logger.Warning($"{"!@GapManageFailed"} (idx= {idx})");
            }
        }
    }
}
