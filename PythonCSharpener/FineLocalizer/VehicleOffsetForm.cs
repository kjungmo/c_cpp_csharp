using CommonUtils;
using System;
using System.Windows.Forms;

namespace FineLocalizer
{
    public partial class VehicleOffsetForm_ : Form
    {
        private static readonly LogHelper Logger = LogHelper.Logger;

        private Config<FineLocalizerConfig> _config;
        private string _varName;

        public VehicleOffsetForm_(Config<FineLocalizerConfig> config)
        {
            InitializeComponent();
            this.Translate(Lang.VehOff.ResourceManager);
            _config = config;

            cbCarType.DataSource = _config.GetCarTypeList();
            cbCarType.SelectedItem = _config.RecentlyUsedCar;

            pictureBoxVehicleOffset.ImageLocation = $"./Resources/vehicleoffset_{_config.InsertionPart.ToString().ToLower()}.png";
            this.ApplyFont();
        }

        private void cbCarType__SelectedIndexChanged(object sender, EventArgs e)
        {
            _varName = $"{cbCarType.SelectedItem}_vehicle_offset";
            if (!_config.RobotPoseVariables.ContainsKey(_varName))
            {
                _config.RobotPoseVariables[_varName] = new RobotPoseVariable("", _config.RobotConfigs[_config[(int)cbCarType.SelectedItem].InstallRobot][RobotAttribute.Maker].ToEnum<RobotMaker>());
            }

            tbCarName.Text = _config[(int)cbCarType.SelectedItem].CarName;
            DisplayOffsetValue(_config.RobotPoseVariables[_varName]);
        }

        private void btnApply__Click(object sender, EventArgs e)
        {
            if (double.TryParse(tbOffsetTx.Text, out double tx) &&
                double.TryParse(tbOffsetTy.Text, out double ty) &&
                double.TryParse(tbOffsetTz.Text, out double tz) &&
                double.TryParse(tbOffsetRx.Text, out double rx) &&
                double.TryParse(tbOffsetRy.Text, out double ry) &&
                double.TryParse(tbOffsetRz.Text, out double rz))
            {
                string carType = _config[(int)cbCarType.SelectedItem].CarName;
                string prevPose = _config.RobotPoseVariables[_varName].ToString();

                _config.RobotPoseVariables[_varName].Tx = tx;
                _config.RobotPoseVariables[_varName].Ty = ty;
                _config.RobotPoseVariables[_varName].Tz = tz;
                _config.RobotPoseVariables[_varName].Rx = rx;
                _config.RobotPoseVariables[_varName].Ry = ry;
                _config.RobotPoseVariables[_varName].Rz = rz;
                DialogResult = DialogResult.OK;

                Logger.Info($"{Lang.LogsFineLo.VehicleOffsetChanged}[{carType}] ({prevPose}) -> ({_config.RobotPoseVariables[_varName]})");

                Close();
            }
            else
            {
                MessageBox.Show(Lang.MsgBoxFineLo.OffsetValueNotValid, Lang.MsgBoxFineLo.WarningTitle);
            }
        }

        private void DisplayOffsetValue(RobotPose pose)
        {
            tbOffsetTx.Text = $"{pose.Tx:F3}";
            tbOffsetTy.Text = $"{pose.Ty:F3}";
            tbOffsetTz.Text = $"{pose.Tz:F3}";
            tbOffsetRx.Text = $"{pose.Rx:F3}";
            tbOffsetRy.Text = $"{pose.Ry:F3}";
            tbOffsetRz.Text = $"{pose.Rz:F3}";
        }

        private void btnCancel__Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
