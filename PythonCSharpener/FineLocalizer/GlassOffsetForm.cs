using CommonUtils;
using System;
using System.Windows.Forms;

namespace FineLocalizer
{
    public partial class GlassOffsetForm_ : Form
    {
        private static readonly LogHelper Logger = LogHelper.Logger;

        private Config<FineLocalizerConfig> _config;
        private string _varName;

        public GlassOffsetForm_(Config<FineLocalizerConfig> config)
        {
            InitializeComponent();
            this.Translate(Lang.GlaOff.ResourceManager);
            _config = config;

            cbCarType.DataSource = _config.GetCarTypeList();
            cbCarType.SelectedItem = _config.RecentlyUsedCar;
            this.ApplyFont();
        }

        private void cbCarType__SelectedIndexChanged(object sender, EventArgs e)
        {
            _varName = $"{cbCarType.SelectedItem}_glass_offset";
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
                double.TryParse(tbOffsetTy.Text, out double ty))
            {
                string carType = _config[(int)cbCarType.SelectedItem].CarName;
                double prevTx = _config.RobotPoseVariables[_varName].Tx;
                double prevTy = _config.RobotPoseVariables[_varName].Ty;

                _config.RobotPoseVariables[_varName].Tx = tx;
                _config.RobotPoseVariables[_varName].Ty = ty;
                DialogResult = DialogResult.OK;

                Logger.Info($"{Lang.LogsFineLo.GlassOffsetChanged}[{carType}] ({prevTx}, {prevTy}) -> ({tx}, {ty})");

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
        }

        private void btnCancel__Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
