using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using CommonUtils;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using CoPick.Logging;
using System.Text.RegularExpressions;

namespace FineLocalizer
{
    public partial class FineLocalizerSettingManagerForm_ : Form
    {
        private Config<FineLocalizerConfig> _config;
        private bool _modified;
        private string _factorySettingsFilePath = "./gifnoc";
        private System.Windows.Forms.Timer _factorySettingTimer = new System.Windows.Forms.Timer();
        private Action<Config<FineLocalizerConfig>> _setConfigDelegate;
        private Dictionary<string, bool> _isDataSourceChanging = new Dictionary<string, bool>();
        private int _mainFormCarType;
        private bool _needCheckLogPath;
        private OperationMode _mode;
        private readonly string[] CameraReadOnlysInAutoMode = new string[] { "Serial" };

        public string ScheduleValues { get; private set; }

        public FineLocalizerSettingManagerForm_(Config<FineLocalizerConfig> config, OperationMode mode,
                                                Action<Config<FineLocalizerConfig>> setConfigDelegate = null)
        {
            _config = config;
            _mode = mode;
            _mainFormCarType = _config.RecentlyUsedCar;
            _setConfigDelegate = setConfigDelegate;
            InitializeComponent();

            this.Translate(Lang.SetMgr.ResourceManager);
            this.ApplyFont();
        }

        #region - VerticalTab Design
        private void tabControl1_DrawItem(Object sender, System.Windows.Forms.DrawItemEventArgs e)
        {
            Graphics g = e.Graphics;
            SolidBrush b = new SolidBrush(Color.White);
            Brush _textBrush;
            Brush _selectBrush;
            int count = SettingManagerTabControl.TabCount;

            // Get the item from the collection.
            TabPage _tabPage = SettingManagerTabControl.TabPages[e.Index];

            // Get the real bounds for the tab rectangle.
            Rectangle _tabBounds = SettingManagerTabControl.GetTabRect(e.Index);
            if (e.State == DrawItemState.Selected)
            {
                _selectBrush = new SolidBrush(Color.FromArgb(255, 208, 65));

                // Draw a different background color, and don't paint a focus rectangle.
                _textBrush = new SolidBrush(Color.Black);
                g.FillRectangle(_selectBrush, e.Bounds);
            }
            else
            {
                _selectBrush = new SolidBrush(Color.FromArgb(243, 243, 243));
                _textBrush = new SolidBrush(Color.FromArgb(90, 90, 90));
                e.Graphics.FillRectangle(_selectBrush, e.Bounds);
            }

            // Use our own font.
            var _tabFont = new Font(FontManager.CustomFont ?? new FontFamily("Consolas"), 13.0f, FontStyle.Bold, GraphicsUnit.Pixel);

            // Draw string. Center the text.
            StringFormat _stringFlags = new StringFormat();
            _stringFlags.Alignment = StringAlignment.Center;
            _stringFlags.LineAlignment = StringAlignment.Center;
            g.DrawString(_tabPage.Text, _tabFont, _textBrush, _tabBounds, new StringFormat(_stringFlags));

            g.FillRectangle(b, new Rectangle(SettingManagerTabControl.ItemSize.Height + 2, 0, this.Size.Width, this.Size.Height));
            g.FillRectangle(b, new Rectangle(0, 0, SettingManagerTabControl.ItemSize.Height + 1, 2));
            g.FillRectangle(b, new Rectangle(0, SettingManagerTabControl.ItemSize.Width * count + 2, SettingManagerTabControl.ItemSize.Height + 2, Size.Height - SettingManagerTabControl.ItemSize.Width * count));
        }
        #endregion

        private void FineLocalizerSettingManagerForm_Load(object sender, EventArgs e)
        {
            SettingManagerTabControl.DrawItem += new DrawItemEventHandler(tabControl1_DrawItem);
            btnFactoryReset_.Enabled = File.Exists(_factorySettingsFilePath) && _mode == OperationMode.Set;
            SetFactorySettingsTimer();

            _isDataSourceChanging["cbPoseVarRobotMaker"] = false;
            cbPoseVarRobotMaker.DataSource = DefaultSettingLoader.Robots.Keys.ToList();
            SetRobotPoseVarGrid();
            InitUIFromConfig();

            FieldInfo fi = typeof(PropertyGrid).GetField("gridView", BindingFlags.Instance | BindingFlags.NonPublic);
            object propertyGridView = fi.GetValue(TaskGrid);
            MethodInfo mi = propertyGridView.GetType().GetMethod("MoveSplitterTo", BindingFlags.NonPublic | BindingFlags.Instance);
            mi.Invoke(propertyGridView, new object[] { 240 });

            if (_mode == OperationMode.Auto)
            {
                groupBox1_.Enabled = false;
                groupBox2_.Enabled = false;
                btnAddCarType_.Enabled = false;
                btnRemoveCarType_.Enabled = false;
                btnCopyCarType_.Enabled = false;

                btnAddRobotConfig_.Enabled = false;
                btnCopyRobot_.Enabled = false;
                btnRemoveRobotConfig_.Enabled = false;

                btnCamAdd_.Enabled = false;
                btnCopyCamera_.Enabled = false;
                btnCamRemove_.Enabled = false;

                gbLogMgrScheduler_.Enabled = false;
                tBoxLogPath.Enabled = false;
                btnLogPath.Enabled = false;
            }

            this.Activate();
        }

        private void InitUIFromConfig()
        {
            SetComboBoxDataSource(cmbCarType, _config.GetCarTypeList());
            SetComboBoxDataSource(cmbPlcConfig, _config.PlcConfigs.Keys.ToList());

            SetCamerasDataSources();
            SetRobotsDataSources();
            numericCamCount.Enabled = _config.CameraConfigs.Count > 0;

            cmbCarType.SelectItemAndFireEventUnconditionally(_config.RecentlyUsedCar, cmbCarType_SelectedIndexChanged);
            cmbPlcConfig.SelectItemAndFireEventUnconditionally(_config.Plc, cmbPlcConfig_SelectedIndexChanged);

            SetComboBoxDataSource(cmbFileLogLev, Enum.GetValues(typeof(LogLevel)));
            SetComboBoxDataSource(cmbUILogLev, Enum.GetValues(typeof(LogLevel)));
            cmbFileLogLev.SelectItemAndFireEventUnconditionally(_config.MinimumFileLogLevel, cmbFileLogLev_SelectedIndexChanged);
            cmbUILogLev.SelectItemAndFireEventUnconditionally(_config.MinimumUiLogLevel, cmbUILogLev_SelectedIndexChanged);

            btnLogPath.Click += (s, e) => OnBtnBrowseFolderPath(tBoxLogPath);
            tBoxLogPath.Text = _config.LogPath;

            TaskGrid.Refresh();
        }

        private void SetComboBoxDataSource(ComboBox cb, object dataSource)
        {
            _isDataSourceChanging[cb.Name] = true;
            cb.DataSource = dataSource;
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

        [Conditional("DEVEL")]
        private void SetFactorySettingsTimer()
        {
            _factorySettingTimer.Interval = 3000;
            _factorySettingTimer.Tick += (s, e) =>
            {
                _factorySettingTimer.Stop();
                var res = MessageBox.Show(Lang.MsgBoxFineLo.WantSaveFactoryDefault,
                                          Lang.MsgBoxFineLo.SettingManagerTitle, MessageBoxButtons.YesNo);
                if (res == DialogResult.Yes)
                {
                    ConfigFileManager.SaveToBinaryFile(_config, _factorySettingsFilePath);
                }
            };

            btnSaveFactorySettings.MouseDown += (s, e) => _factorySettingTimer.Start();
            btnSaveFactorySettings.MouseUp += (s, e) => _factorySettingTimer.Stop();
        }

        private void cmbRobotConfig_SelectedIndexChanged(object sender, EventArgs e)
        {
            RobotConfigGrid.SelectedObject = new DictionaryPropertyGridAdapter<RobotAttribute, string>(_config.RobotConfigs[Convert.ToString(cmbRobotConfig.SelectedItem)]);
            if (_mode == OperationMode.Auto)
            {
                RobotConfigGrid.Enabled = false;
            }
            TaskGrid.Refresh();
        }

        private void cmbCam_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetCameraPropertyGrid();
            TaskGrid.Refresh();
        }

        private void cmbPlcConfig_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (CheckIfDataSourceIsChanging(sender)) return;

            _modified |= _config.Plc != (PlcModel)cmbPlcConfig.SelectedItem;
            plcConfigGrid.SelectedObject = new DictionaryPropertyGridAdapter<PlcAttribute, string>(_config.PlcConfigs[(PlcModel)cmbPlcConfig.SelectedItem]);
            if (_mode == OperationMode.Auto)
            {
                plcConfigGrid.Enabled = false;
            }
        }

        private void cmbCarType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (CheckIfDataSourceIsChanging(sender)) return;

            int selectedCarType = Convert.ToInt32(cmbCarType.SelectedItem);
            _config.RecentlyUsedCar = selectedCarType;
            TaskGrid.SelectedObject = _config[-1];

            SelectRobots();
            SelectCameras();
        }

        private void configPropertyGrid_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            var validatorAttr = (ValidatorTypeAttribute)e.ChangedItem.PropertyDescriptor.Attributes[typeof(ValidatorTypeAttribute)];
            if (validatorAttr != null)
            {
                (bool ret, string msg) = validatorAttr.ValidatorFunc(e.ChangedItem.Value);
                if (!ret)
                {
                    MessageBox.Show(msg, Lang.MsgBoxFineLo.WarningTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    e.ChangedItem.PropertyDescriptor.SetValue(e.ChangedItem.Parent.Value ?? (s as PropertyGrid).SelectedObject, e.OldValue);
                    SendKeys.Send("{TAB}");
                    return;
                }
            }

            _modified = true;
            (s as PropertyGrid).Refresh();
        }

        private void FineLocalizerSettingManagerForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_needCheckLogPath)
            {
                tbFolderPathConf_Leave(tBoxLogPath, null);
            }

            if (_modified)
            {
                DialogResult result = MessageBox.Show(Lang.MsgBoxFineLo.WantSaveSettings, Lang.MsgBoxFineLo.SettingManagerTitle,
                                                      MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
                if (result == DialogResult.OK)
                {
                    _config.Plc = (cmbPlcConfig.SelectedItem == null) ? PlcModel.MELSEC : (PlcModel)cmbPlcConfig.SelectedItem;

                    this.DialogResult = DialogResult.OK;
                }
                else
                {
                    this.DialogResult = DialogResult.Cancel;
                }
            }
            else
            {
                this.DialogResult = DialogResult.Ignore;
            }

            _config.RecentlyUsedCar = _mainFormCarType;
        }

        private void btnFactoryReset_Click(object sender, EventArgs e)
        {
            var res = MessageBox.Show(Lang.MsgBoxFineLo.WantResetFactorySetting,
                                      Lang.MsgBoxFineLo.SettingManagerTitle, MessageBoxButtons.YesNo);

            if (res == DialogResult.Yes)
            {
                _config = ConfigFileManager.LoadFromBinaryFile<Config<FineLocalizerConfig>>(_factorySettingsFilePath);
                _modified = true;
                _setConfigDelegate?.Invoke(_config);
                InitUIFromConfig();
            }
        }

        private void btnCamAdd_Click(object sender, EventArgs e)
        {
            using (AddCamConfigForm_ form = new AddCamConfigForm_())
            {
                form.CameraNames = _config.CameraConfigs.Keys.ToList();
                if (form.ShowDialog() == DialogResult.OK)
                {
                    _config.CameraConfigs[form.CameraName] = new List<Dictionary<CameraAttribute, string>>()
                    {
                        DefaultSettingLoader.Cameras[form.CamModel]()
                    };

                    SetCamerasDataSources();
                    cmbCamConfig.SelectedItem = form.CameraName;

                    SelectCameras();
                    _modified = true;

                    if (_config.CameraConfigs.Count == 1)
                    {
                        numericCamCount.Enabled = true;
                    }
                }
            }
        }

        private void btnCamRemove_Click(object sender, EventArgs e)
        {
            if (_config.CameraConfigs.Count == 0) return;

            DialogResult result = MessageBox.Show(Lang.MsgBoxFineLo.WantRemove, Lang.MsgBoxFineLo.SettingManagerTitle,
                                                  MessageBoxButtons.OKCancel, MessageBoxIcon.Information);

            if (result == DialogResult.OK)
            {
                _config.CameraConfigs.Remove(cmbCamConfig.SelectedItem.ToString());

                SetCamerasDataSources();

                if (_config.CameraConfigs.Count == 0)
                {
                    _config[-1].GapCamera = null;
                    _config[-1].GlassCamera = null;
                    _config[-1].VehicleCamera = null;
                    CamConfigGrid.SelectedObject = null;

                    numericCamCount.Value = 1;
                    numericCamCount.Enabled = false;
                }
                else
                {
                    SelectCameras();
                }

                _modified = true;
            }
        }

        private void btnAddRobotConfig_Click(object sender, EventArgs e)
        {
            using (AddRobotConfigForm_ form = new AddRobotConfigForm_())
            {
                form.RobotNames = _config.RobotConfigs.Keys.ToList();
                if (form.ShowDialog() == DialogResult.OK)
                {
                    _config.RobotConfigs[form.RobotName] = DefaultSettingLoader.Robots[form.RobotMaker]();

                    SetRobotsDataSources();
                    cmbRobotConfig.SelectedItem = form.RobotName;

                    SelectRobots();
                    _modified = true;
                }
            }
        }

        private void btnRemoveRobotConfig_Click(object sender, EventArgs e)
        {
            if (_config.RobotConfigs.Count == 0) return;

            DialogResult result = MessageBox.Show(Lang.MsgBoxFineLo.WantRemove, Lang.MsgBoxFineLo.SettingManagerTitle,
                                                  MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
            if (result == DialogResult.OK)
            {
                _config.RobotConfigs.Remove(cmbRobotConfig.SelectedItem.ToString());

                SetRobotsDataSources();

                if (_config.RobotConfigs.Count == 0)
                {
                    _config[-1].InstallRobot = null;
                    _config[-1].ScanRobot = null;
                    RobotConfigGrid.SelectedObject = null;
                }
                else
                {
                    SelectRobots();
                }

                _modified = true;
            }
        }

        private void SetCamerasDataSources()
        {
            cmbCamConfig.DataSource = _config.CameraConfigs.Keys.ToList();
            SetComboBoxDataSource(cmbGapCamera, _config.CameraConfigs.Keys.ToList());
            SetComboBoxDataSource(cmbGlassCamera, _config.CameraConfigs.Keys.ToList());
            SetComboBoxDataSource(cmbVehicleCamera, _config.CameraConfigs.Keys.ToList());
        }

        private void SetRobotsDataSources()
        {
            cmbRobotConfig.DataSource = _config.RobotConfigs.Keys.ToList();
            SetComboBoxDataSource(cmbInstallRobot, _config.RobotConfigs.Keys.ToList());
            SetComboBoxDataSource(cmbScanRobot, _config.RobotConfigs.Keys.ToList());
        }

        private void btnAddCarType_Click(object sender, EventArgs e)
        {
            using (var form = new AddNewCarTypeForm_())
            {
                form.ExistingCarTypes = _config.GetCarTypeList();
                if (form.ShowDialog() == DialogResult.OK)
                {
                    _config[form.CarType] = new FineLocalizerConfig { };
                    _config[form.CarType].CarName = form.CarName;
                    cmbCarType.DataSource = _config.GetCarTypeList();
                    cmbCarType.SelectedItem = form.CarType;
                    _modified = true;
                }
            }
        }

        private void btnRemoveCarType_Click(object sender, EventArgs e)
        {
            if (_config.GetCarTypeList().Count < 2)
            {
                MessageBox.Show(Lang.MsgBoxFineLo.CantRemoveLastCar, Lang.MsgBoxFineLo.SettingManagerTitle);
                return;
            }

            if (MessageBox.Show(Lang.MsgBoxFineLo.WantRemove, Lang.MsgBoxFineLo.SettingManagerTitle, MessageBoxButtons.YesNo)
                == DialogResult.No)
            {
                return;
            }

            if (_config.Delete(Convert.ToInt32(cmbCarType.SelectedItem)))
            {
                cmbCarType.DataSource = _config.GetCarTypeList();
            }
            _modified = true;
        }

        private void SetRobotPoseVarGrid()
        {
            if (_config.RobotPoseVariables.Count > 0)
            {
                robotPoseVariableGrid.SelectedObject = new DictionaryPropertyGridAdapter<string, RobotPoseVariable>(_config.RobotPoseVariables);
                robotPoseVariableGrid_SelectedGridItemChanged(null, null);
            }
            else
            {
                robotPoseVariableGrid.SelectedObject = null;
            }
        }

        private void btnAddPoseVar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tbPoseVarName.Text))
            {
                MessageBox.Show(Lang.MsgBoxFineLo.NeedVarName, Lang.MsgBoxFineLo.WarningTitle);
                return;
            }

            if (_config.RobotPoseVariables.ContainsKey(tbPoseVarName.Text))
            {
                MessageBox.Show(Lang.MsgBoxFineLo.UsedVarName, Lang.MsgBoxFineLo.WarningTitle);
                return;
            }

            DialogResult result = MessageBox.Show(Lang.MsgBoxFineLo.WantAddRobotPoseVar, Lang.MsgBoxFineLo.SettingManagerTitle,
                                                  MessageBoxButtons.OKCancel, MessageBoxIcon.Information);

            if (result == DialogResult.OK)
            {
                _config.RobotPoseVariables[tbPoseVarName.Text] = new RobotPoseVariable(cbPoseVarCoordinateSystem.SelectedItem as string,
                                                                                       (RobotMaker)cbPoseVarRobotMaker.SelectedItem);
                SetRobotPoseVarGrid();
                _modified = true;
            }
        }

        private void btnRemovePoseVar_Click(object sender, EventArgs e)
        {
            if (_config.RobotPoseVariables.Count > 0)
            {
                DialogResult result = MessageBox.Show(Lang.MsgBoxFineLo.WantRemove, Lang.MsgBoxFineLo.SettingManagerTitle,
                                                      MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
                if (result == DialogResult.OK)
                {
                    if (robotPoseVariableGrid.SelectedGridItem.GridItemType == GridItemType.Property)
                    {
                        string selectedLabel = GetSelectedVariableName();

                        _config.RobotPoseVariables.Remove(selectedLabel);
                        SetRobotPoseVarGrid();
                        _modified = true;
                    }
                }
            }
        }

        private void robotPoseVariableGrid_SelectedGridItemChanged(object sender, SelectedGridItemChangedEventArgs e)
        {
            tbPoseVarName.Text = GetSelectedVariableName();
            cbPoseVarRobotMaker.SelectItemAndFireEventUnconditionally(_config.RobotPoseVariables[tbPoseVarName.Text].Maker,
                                                                      cbPoseVarRobotMaker_SelectedIndexChanged);
        }

        private void btnModifyPoseVar_Click(object sender, EventArgs e)
        {
            string oldValue = GetSelectedVariableName();
            if (string.IsNullOrEmpty(oldValue) || tbPoseVarName.Text == oldValue) return;

            if (string.IsNullOrWhiteSpace(tbPoseVarName.Text))
            {
                MessageBox.Show(Lang.MsgBoxFineLo.NeedVarName, Lang.MsgBoxFineLo.WarningTitle);
                return;
            }

            if (_config.RobotPoseVariables.ContainsKey(tbPoseVarName.Text))
            {
                MessageBox.Show(Lang.MsgBoxFineLo.UsedVarName, Lang.MsgBoxFineLo.WarningTitle);
                return;
            }

            DialogResult result = MessageBox.Show(Lang.MsgBoxFineLo.WantModifyRobotPosevar,
                                                  Lang.MsgBoxFineLo.SettingManagerTitle,
                                                  MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
            if (result == DialogResult.OK)
            {
                _config.RobotPoseVariables[tbPoseVarName.Text] = _config.RobotPoseVariables[oldValue];
                _config.RobotPoseVariables.Remove(oldValue);
                SetRobotPoseVarGrid();
                _modified = true;
            }
        }

        private void cbPoseVarRobotMaker_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (CheckIfDataSourceIsChanging(sender)) return;

            string[] crdSysList = null;
            switch ((RobotMaker)cbPoseVarRobotMaker.SelectedItem)
            {
                case RobotMaker.YASKAWA:
                    crdSysList = Enum.GetNames(typeof(YrcCoordinateSystem));
                    break;

                case RobotMaker.FANUC:
                    crdSysList = Enum.GetNames(typeof(FanucUserFrame));
                    break;

                case RobotMaker.HYUNDAI:
                    crdSysList = Enum.GetNames(typeof(HrCoordinateSystem));
                    break;
            }
            SetComboBoxDataSource(cbPoseVarCoordinateSystem, crdSysList);

            string crdSys = null;
            if (_config.RobotPoseVariables.TryGetValue(tbPoseVarName.Text, out var pose))
            {
                RobotMaker newMaker = (RobotMaker)cbPoseVarRobotMaker.SelectedItem;
                if (pose.Maker != newMaker)
                {
                    pose.Maker = newMaker;
                    _modified = true;
                }

                crdSys = pose.CoordinateSystem;
            }

            if (crdSysList == null)
            {
                pose.CoordinateSystem = "";
                return;
            }

            if (string.IsNullOrEmpty(crdSys) || !crdSysList.Contains(crdSys))
            {
                crdSys = crdSysList[0];
            }

            cbPoseVarCoordinateSystem.SelectItemAndFireEventUnconditionally(crdSys,
                                                                            cbPoseVarCoordinateSystem_SelectedIndexChanged);
        }

        private void cbPoseVarCoordinateSystem_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (CheckIfDataSourceIsChanging(sender)) return;

            if (_config.RobotPoseVariables.TryGetValue(tbPoseVarName.Text, out var pose))
            {
                string newCrdSys = cbPoseVarCoordinateSystem.SelectedItem as string;
                if (pose.CoordinateSystem != newCrdSys)
                {
                    pose.CoordinateSystem = cbPoseVarCoordinateSystem.SelectedItem as string;
                    _modified = true;
                }
            }
        }

        private string GetSelectedVariableName()
        {
            string selectedLabel;
            if (robotPoseVariableGrid.SelectedGridItem == null)
            {
                selectedLabel = null;
            }
            else if (robotPoseVariableGrid.SelectedGridItem.Expandable == false)
            {
                selectedLabel = robotPoseVariableGrid.SelectedGridItem.Parent.Label;
            }
            else
            {
                selectedLabel = robotPoseVariableGrid.SelectedGridItem.Label;
            }
            return selectedLabel;
        }

        private void SetCameraPropertyGrid()
        {
            CamConfigGrid.SelectedObject = _config.CameraConfigs[cmbCamConfig.SelectedItem.ToString()].Select(c =>
            {
                return new DictionaryPropertyGridAdapter<CameraAttribute, string>(c, _mode == OperationMode.Auto ? CameraReadOnlysInAutoMode : null);
            }).ToArray();
            CamConfigGrid.ExpandAllGridItems();
            numericCamCount.Value = _config.CameraConfigs[cmbCamConfig.SelectedItem.ToString()].Count;
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (CamConfigGrid.SelectedGridItem == null) return;

            DialogResult result = MessageBox.Show(Lang.MsgBoxFineLo.WantRemove, Lang.MsgBoxFineLo.SettingManagerTitle,
                                                  MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
            if (result == DialogResult.OK)
            {
                if (CamConfigGrid.SelectedGridItem.GridItemType == GridItemType.Property)
                {
                    string selectedLabel;
                    if (CamConfigGrid.SelectedGridItem.Expandable == false)
                    {
                        selectedLabel = CamConfigGrid.SelectedGridItem.Parent.Label;
                    }
                    else
                    {
                        selectedLabel = CamConfigGrid.SelectedGridItem.Label;
                    }

                    int index = Convert.ToInt32(selectedLabel.Substring(1, selectedLabel.Length - 2));
                    if (_config.CameraConfigs[cmbCamConfig.SelectedItem.ToString()].Count > 1)
                    {
                        _config.CameraConfigs[cmbCamConfig.SelectedItem.ToString()].RemoveAt(index);
                    }
                    else
                    {
                        MessageBox.Show(Lang.MsgBoxFineLo.CantRemoveLastCam, Lang.MsgBoxFineLo.SettingManagerTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    _modified = true;
                    SetCameraPropertyGrid();
                }
            }
        }

        private void numericCamCount_ValueChanged(object sender, EventArgs e)
        {
            if (cmbCamConfig.SelectedItem == null) return;

            string camName = cmbCamConfig.SelectedItem.ToString();
            int count = _config.CameraConfigs[camName].Count - Convert.ToInt32(numericCamCount.Value);

            if (count == 0)
            {
                return;
            }
            else if (count < 0)
            {
                count *= -1;
                var camModel = _config.CameraConfigs[camName][0][CameraAttribute.Model].ToEnum<CameraModel>();

                for (var i = 0; i < count; ++i)
                {
                    _config.CameraConfigs[camName].Add(DefaultSettingLoader.Cameras[camModel]());
                }
            }
            else
            {
                DialogResult result = MessageBox.Show(Lang.MsgBoxFineLo.WantRemoveCamItem, Lang.MsgBoxFineLo.WarningTitle,
                                                      MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
                if (result == DialogResult.OK)
                {
                    _config.CameraConfigs[camName].RemoveRange(Convert.ToInt32(numericCamCount.Value), count);
                }
            }
            _modified = true;
            SetCameraPropertyGrid();
        }

        private void addToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (CamConfigGrid.SelectedGridItem == null) return;

            var selCams = _config.CameraConfigs[cmbCamConfig.SelectedItem.ToString()];
            if (selCams.Count < numericCamCount.Maximum)
            {
                selCams.Add(DefaultSettingLoader.Cameras[selCams[0][CameraAttribute.Model].ToEnum<CameraModel>()]());
                _modified = true;
                SetCameraPropertyGrid();
            }
        }

        private void cmbInstallRobot_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (CheckIfDataSourceIsChanging(sender)) return;

            string selectedRobot = cmbInstallRobot.SelectedItem?.ToString();
            _modified |= _config[-1].InstallRobot != selectedRobot;
            _config[-1].InstallRobot = selectedRobot;
        }

        private void cmbScanRobot_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (CheckIfDataSourceIsChanging(sender)) return;

            string selectedRobot = cmbScanRobot.SelectedItem?.ToString();
            _modified |= _config[-1].ScanRobot != selectedRobot;
            _config[-1].ScanRobot = selectedRobot;
        }

        private void cmbGapCamera_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (CheckIfDataSourceIsChanging(sender)) return;

            string selectedCam = cmbGapCamera.SelectedItem?.ToString();
            _modified |= _config[-1].GapCamera != selectedCam;
            _config[-1].GapCamera = selectedCam;
        }

        private void cmbGlassCamera_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (CheckIfDataSourceIsChanging(sender)) return;

            string selectedCam = cmbGlassCamera.SelectedItem?.ToString();
            _modified |= _config[-1].GlassCamera != selectedCam;
            _config[-1].GlassCamera = selectedCam;
        }

        private void cmbVehicleCamera_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (CheckIfDataSourceIsChanging(sender)) return;

            string selectedCam = cmbVehicleCamera.SelectedItem?.ToString();
            _modified |= _config[-1].VehicleCamera != selectedCam;
            _config[-1].VehicleCamera = selectedCam;
        }

        private void SelectRobots()
        {
            if (!_config.RobotConfigs.ContainsKey(_config[-1].InstallRobot ?? ""))
            {
                _config[-1].InstallRobot = cmbInstallRobot.SelectedItem?.ToString();
                _modified |= _config[-1].InstallRobot != null;
            }
            else
            {
                cmbInstallRobot.SelectItemAndFireEventUnconditionally(_config[-1].InstallRobot ?? "", cmbInstallRobot_SelectedIndexChanged);
            }

            if (!_config.RobotConfigs.ContainsKey(_config[-1].ScanRobot ?? ""))
            {
                _config[-1].ScanRobot = cmbScanRobot.SelectedItem?.ToString();
                _modified |= _config[-1].ScanRobot != null;
            }
            else
            {
                cmbScanRobot.SelectItemAndFireEventUnconditionally(_config[-1].ScanRobot ?? "", cmbScanRobot_SelectedIndexChanged);
            }
        }

        private void SelectCameras()
        {
            if (!_config.CameraConfigs.ContainsKey(_config[-1].GapCamera ?? ""))
            {
                _config[-1].GapCamera = cmbGapCamera.SelectedItem?.ToString();
                _modified |= _config[-1].GapCamera != null;
            }
            else
            {
                cmbGapCamera.SelectItemAndFireEventUnconditionally(_config[-1].GapCamera, cmbGapCamera_SelectedIndexChanged);
            }

            if (!_config.CameraConfigs.ContainsKey(_config[-1].GlassCamera ?? ""))
            {
                _config[-1].GlassCamera = cmbGlassCamera.SelectedItem?.ToString();
                _modified |= _config[-1].GlassCamera != null;
            }
            else
            {
                cmbGlassCamera.SelectItemAndFireEventUnconditionally(_config[-1].GlassCamera, cmbGlassCamera_SelectedIndexChanged);
            }

            if (!_config.CameraConfigs.ContainsKey(_config[-1].VehicleCamera ?? ""))
            {
                _config[-1].VehicleCamera = cmbVehicleCamera.SelectedItem?.ToString();
                _modified |= _config[-1].VehicleCamera != null;
            }
            else
            {
                cmbVehicleCamera.SelectItemAndFireEventUnconditionally(_config[-1].VehicleCamera, cmbVehicleCamera_SelectedIndexChanged);
            }
        }

        private void btnCopyRobot_Click(object sender, EventArgs e)
        {
            if (RobotConfigGrid.SelectedGridItem == null) return;

            using (CopyConfigsForm_ form = new CopyConfigsForm_())
            {
                form.StartPosition = FormStartPosition.CenterScreen;
                form.ConfigList = _config.RobotConfigs.Keys.ToList();
                if (form.ShowDialog() == DialogResult.OK)
                {
                    _config.RobotConfigs[form.CopiedName] = new Dictionary<RobotAttribute, string>(_config.RobotConfigs[cmbRobotConfig.SelectedItem.ToString()]);

                    SetRobotsDataSources();
                    cmbRobotConfig.SelectedItem = form.CopiedName;

                    SelectRobots();
                    _modified = true;
                }
            }
        }

        private void btnCopyCamera_Click(object sender, EventArgs e)
        {
            if (CamConfigGrid.SelectedGridItem == null) return;

            using (CopyConfigsForm_ form = new CopyConfigsForm_())
            {
                form.StartPosition = FormStartPosition.CenterScreen;
                form.ConfigList = _config.CameraConfigs.Keys.ToList();
                if (form.ShowDialog() == DialogResult.OK)
                {
                    _config.CameraConfigs[form.CopiedName] =
                        _config.CameraConfigs[cmbCamConfig.SelectedItem.ToString()].ConvertAll(o => new Dictionary<CameraAttribute, string>(o));

                    SetCamerasDataSources();
                    cmbCamConfig.SelectedItem = form.CopiedName;

                    SelectCameras();
                    _modified = true;
                }
            }
        }

        private void btnCopyCarType_Click(object sender, EventArgs e)
        {
            using (CopyConfigsForm_ form = new CopyConfigsForm_())
            {
                form.StartPosition = FormStartPosition.CenterScreen;
                form.ConfigList = _config.GetCarTypeStringList();
                if (form.ShowDialog() == DialogResult.OK)
                {
                    if (!int.TryParse(form.CopiedName, out var carType))
                    {
                        MessageBox.Show(Lang.MsgBoxFineLo.InvalidCarType, Lang.MsgBoxFineLo.WarningTitle);
                        btnCopyCarType_Click(null, null);
                        return;
                    }

                    _config[carType] = _config[(int)cmbCarType.SelectedItem].DeepClone();
                    _config[carType].CarName = $"{_config[(int)cmbCarType.SelectedItem].CarName}_copy";
                    cmbCarType.DataSource = _config.GetCarTypeList();
                    cmbCarType.SelectedItem = carType;
                    _modified = true;
                }
            }
        }

        private void btnChangePassword__Click(object sender, EventArgs e)
        {
            using (var f = new ChangePasswordForm_())
            {
                f.ShowDialog();
            }
        }

        private void OnBtnBrowseFolderPath(TextBox tBoxLogPath)
        {
            try
            {
                folderBrowserDialoglogPath.SelectedPath = Path.GetFullPath(tBoxLogPath.Text);
            }
            catch
            {
            }

            if (folderBrowserDialoglogPath.ShowDialog() == DialogResult.OK)
            {
                tBoxLogPath.Text = folderBrowserDialoglogPath.SelectedPath;
                tbFolderPathConf_Leave(tBoxLogPath, null);
            }
        }

        private void tbFolderPathConf_Leave(object sender, EventArgs e)
        {
            _needCheckLogPath = false;
            TextBox tb = sender as TextBox;

            try
            {
                if (!Directory.Exists(tb.Text))
                {
                    Directory.CreateDirectory(tb.Text);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{Lang.MsgBoxFineLo.InvalidPath} {ex}", Lang.MsgBoxFineLo.WarningTitle);
                tb.Text = _config.LogPath;
                return;
            }

            if (_config.LogPath != tb.Text)
            {
                _config.LogPath = tb.Text;
                _modified = true;
            }
        }

        private void cmbFileLogLev_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (CheckIfDataSourceIsChanging(sender)) return;

            var fileLogLev = (LogLevel)cmbFileLogLev.SelectedItem;

            _modified |= _config.MinimumFileLogLevel != fileLogLev;
            _config.MinimumFileLogLevel = fileLogLev;
        }

        private void cmbUILogLev_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (CheckIfDataSourceIsChanging(sender)) return;

            var uiLogLev = (LogLevel)cmbUILogLev.SelectedItem;

            _modified |= _config.MinimumUiLogLevel != uiLogLev;
            _config.MinimumUiLogLevel = uiLogLev;
        }

        private void tBoxLogPath_Enter(object sender, EventArgs e)
        {
            _needCheckLogPath = true;
        }

        private async void Logging__Enter(object sender, EventArgs e)
        {
            ScheduleValues = await TaskSchedulerManager.CheckAlreadyRegisteredAsync();
            SetScheduleValues();
        }

        private void SetScheduleValues()
        {
            Invoke(new MethodInvoker(() =>
            {
                if (!string.IsNullOrEmpty(ScheduleValues))
                {
                    btnDeleteSchedule_.Enabled = true;
                    string[] values = new Regex(@"[ ](?=(?:[^""]*""[^""]*"")*[^""]*$)").Split(ScheduleValues);
                    numericLogPeriodCount.Text = values[3];
                    numericImgPeriodCount.Text = values[5];
                    numericCsvPeriodCount.Text = values[7];
                    dtpScheduleStartTime.Value = DateTime.ParseExact(values[8], "HH:mm", Thread.CurrentThread.CurrentCulture);
                }
                else
                {
                    btnDeleteSchedule_.Enabled = false;
                    numericLogPeriodCount.Text = "0";
                    numericImgPeriodCount.Text = "0";
                    numericCsvPeriodCount.Text = "0";
                    dtpScheduleStartTime.Value = DateTime.Today;
                }
            }));
        }

        private async void btnRegisterSchedule__Click(object sender, EventArgs e)
        {
            btnDeleteSchedule_.Enabled = false;
            btnRegisterSchedule_.Enabled = false;

            string editedLogPath = tBoxLogPath.Text.Contains(" ") ? $"\"{tBoxLogPath.Text}\"" : tBoxLogPath.Text;
            
            ScheduleValues = $"no_zip {Path.GetFullPath(editedLogPath)} " +
                $"0 {numericLogPeriodCount.Value} " +
                $"0 {numericImgPeriodCount.Value} " +
                $"0 {numericCsvPeriodCount.Value} " +
                $"{dtpScheduleStartTime.Value:HH:mm}";

            await TaskSchedulerManager.AddDailyTaskScheduleAsync("no_zip", Path.GetFullPath(@".\LogManager.exe"), Path.GetFullPath(editedLogPath),
                0, Convert.ToInt32(numericLogPeriodCount.Value),
                0, Convert.ToInt32(numericImgPeriodCount.Value),
                0, Convert.ToInt32(numericCsvPeriodCount.Value),
                dtpScheduleStartTime.Value);

            btnDeleteSchedule_.Enabled = true;
            btnRegisterSchedule_.Enabled = true;
        }
        
        private void btnDeleteSchedule__Click(object sender, EventArgs e)
        {
            ScheduleValues = "";
            TaskSchedulerManager.DeleteTaskSchedule();
            SetScheduleValues();
        }
    }
}