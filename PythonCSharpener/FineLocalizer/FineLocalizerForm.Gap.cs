using CommonUtils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FineLocalizer
{
    partial class FineLocalizerForm_
    {
        private RefPoses _refGapPoses = new RefPoses()
        {
            Values = new List<RobotPose>()
        };

        private void SetGapButtonsOnOff(int numberOfAvailablePoints)
        {
            for (var i = 0; i < numberOfAvailablePoints; ++i)
            {
                _btnGapPoints[i].Enabled = true;
            }

            for (var i = numberOfAvailablePoints; i < 4; ++i)
            {
                _btnGapPoints[i].Enabled = false;
            }
        }
        
        private void SetGapPointsOnOff(int numberOfPoints)
        {
            for (var i = 0; i < numberOfPoints; ++i)
            {
                _gbGapPoints[i].Visible = true;
            }

            for (var i = numberOfPoints; i < 4; ++i)
            {
                _gbGapPoints[i].Visible = false;
            }
        }

        private void InitGapUI()
        {
            foreach (var gb in _gbGapPoints)
            {
                ClearRobotPoseUi(gb);
            }
            ClearRobotPoseUi(gbGapSV_);

            Invoke(new MethodInvoker(() =>
            {
                btnGapUpdate_.Enabled = false;
                btnGapUpdate_.BackColor = Color.DarkGray;

                btnGapCalculate_.Enabled = false;
                btnGapCalculate_.BackColor = Color.DarkGray;

                btnGapTeach0_.Enabled = false;
                btnGapTeach1_.Enabled = false;
                btnGapTeach2_.Enabled = false;
                btnGapTeach3_.Enabled = false;
                btnGapAdd0_.Enabled = false;
                btnGapAdd1_.Enabled = false;
                btnGapAdd2_.Enabled = false;
                btnGapAdd3_.Enabled = false;

                tbGap1.Text = "";
                tbGap2.Text = "";
                tbGap3.Text = "";
                tbGap4.Text = "";

                tbNumGapExamined1.Text = "";
                tbNumGapExamined2.Text = "";
                tbNumGapExamined3.Text = "";
                tbNumGapExamined4.Text = "";

                pictureBoxGap1.Image = null;
                pictureBoxGap2.Image = null;
                pictureBoxGap3.Image = null;
                pictureBoxGap4.Image = null;

                DisplayGapVisionStatus(VisionStatus.NONE);

                if (_mode == OperationMode.Set)
                {
                    foreach (var btnPoint in _btnGapPoints)
                    {
                        btnPoint.Text = Lang.FineLo.btnGapSavePoints;
                        btnPoint.BackColor = Color.FromArgb(255, 75, 75);
                        btnPoint.ForeColor = Color.White;
                    }

                    SetGapButtonsOnOff(1);
                    SetGapPointsOnOff(4);
                }
                else
                {
                    foreach (var btnPoint in _btnGapPoints)
                    {
                        btnPoint.Text = Lang.FineLo.btnGapScanPoints;
                        btnPoint.BackColor = Color.FromArgb(80, 215, 73);
                        btnPoint.ForeColor = Color.White;
                    }

                    SetGapPointsOnOff(_refGapPoses.Values?.Count ?? 0);
                    if (_mode == OperationMode.Auto)
                    {
                        SetGapButtonsOnOff(0);
                    }
                    else
                    {
                        SetGapButtonsOnOff(_refGapPoses.Values?.Count ?? 0);
                    }
                }
            }));
        }

        private bool LoadRefGapPosesFile()
        {
            if (!_config[-1].UseGapChecker)
            {
                return false;
            }

            _refGapPoses = RefPosesFileManager.LoadFromFile($"{_config[-1].GapRefDataPath}/ref_pose.dat", _config[-1].GapCamera);
            if (_refGapPoses.Values == null)
            {
                Logger.Warning(Lang.LogsFineLo.LoadingGapCheckRefPoseFailed);
                return false;
            }
            else
            {
                Logger.Info(Lang.LogsFineLo.LoadingGapCheckRefPoseDone);
                return true;
            }
        }

        private async void btnGapPointn_Click(object sender, EventArgs e)
        {
            var btnSender = sender as Button;
            btnSender.Enabled = false;

            if (!_isCameraConnected)
            {
                Logger.Warning(Lang.LogsFineLo.CameraConnectionNeeded);
                btnSender.Enabled = true;
                return;
            }

            if (!FineLocalizerVehicleEngineAPI.ReadyGapCheck())
            {
                Logger.Warning(Lang.LogsFineLo.ReadyGapError);
                return;
            }

            var name = btnSender.Name;
            var pointNum = Convert.ToInt32(name.Substring(name.Length - 2, 1));

            try
            {
                if (_mode == OperationMode.Set)
                {
                    await SaveCurrentRobotPoseAndImageForGapCheck(pointNum);

                    btnGapAdd0_.Enabled = true;
                    btnGapAdd1_.Enabled = true;
                    btnGapAdd2_.Enabled = true;
                    btnGapAdd3_.Enabled = true;

                    btnGapTeach0_.Enabled = true;
                    btnGapTeach1_.Enabled = true;
                    btnGapTeach2_.Enabled = true;
                    btnGapTeach3_.Enabled = true;
                }
                else
                {
                    await ScanGapPoint(pointNum);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                btnSender.Enabled = true;
            }
        }

        private async Task ScanGapPoint(int pointNum)
        {
            var curPose = await _gapChecker.Robot.ReadRobotCurrentPoseAsync();
            if (curPose == null)
            {
                Logger.Error(Lang.LogsFineLo.CantReadRobotCurrentPose);
                return;
            }
            UpdateRobotPoseToUI(curPose, _gbGapPoints[pointNum - 1]);

            var retCap = await _gapChecker.TriggerScanGapAsync(false, false);
            if (retCap)
            {
                Logger.Info($"{Lang.LogsFineLo.GapCheckPointScanDone} (point= {pointNum})");
                Invoke(new MethodInvoker(() =>
                {
                    btnGapCalculate_.Enabled = _mode != OperationMode.Auto;
                    btnGapCalculate_.BackColor = Color.Lime;
                }));
            }
            else
            {
                Logger.Warning($"{Lang.LogsFineLo.GapCheckPointScanFailed} (point= {pointNum})");
            }
        }

        private async Task SaveCurrentRobotPoseAndImageForGapCheck(int pointNum)
        {
            bool saveImg = MessageBox.Show($"{Lang.MsgBoxFineLo.WantSaveRefImg}", "", MessageBoxButtons.YesNo) == DialogResult.Yes;
            bool savePose = MessageBox.Show($"{Lang.MsgBoxFineLo.WantSaveRefPose}", "", MessageBoxButtons.YesNo) == DialogResult.Yes;

            var curPose = await _gapChecker.Robot.ReadRobotCurrentPoseAsync();
            UpdateRobotPoseToUI(curPose, _gbGapPoints[pointNum - 1]);

            if (await _gapChecker.TriggerScanGapAsync(true, saveImg))
            {
                Logger.Info($"{Lang.LogsFineLo.ScanningPointDone} (point= {pointNum})");
                Invoke(new MethodInvoker(() => SetGapButtonsOnOff(pointNum + 1)));
            }
            else
            {
                Logger.Error($"{Lang.LogsFineLo.ScanningPointFailed} (point= {pointNum})");
                return;
            }

            if (savePose)
            {
                try
                {
                    if (!Directory.Exists(_config[-1].GapRefDataPath))
                    {
                        Directory.CreateDirectory(_config[-1].GapRefDataPath);
                    }

                    if (_refGapPoses.Values == null) _refGapPoses.Values = new List<RobotPose> { curPose };
                    else if (_refGapPoses.Values.Count < pointNum) _refGapPoses.Values.Add(curPose);
                    else _refGapPoses.Values[pointNum - 1] = curPose;

                    RefPosesFileManager.SaveToFile(_refGapPoses, $"{_config[-1].GapRefDataPath}/ref_pose.dat", _config[-1].GapCamera);

                    Logger.Info($"{Lang.LogsFineLo.SavingRefPoseDone} (point= {pointNum})");
                }
                catch (Exception ex)
                {
                    Logger.Error($"{Lang.LogsFineLo.SavingRefPoseFailed} (point= {pointNum}) <{ex.Message}>");
                }
            }
        }

        private async void btnGapCalculate_Click(object sender, EventArgs e)
        {
            btnGapCalculate_.Enabled = false;
            (bool isSuccess, double[] gapAverages) = await CalculateGapAsync();

            if (isSuccess)
            {
                Invoke(new MethodInvoker(() => DisplayGapVisionStatus(VisionStatus.OK)));
                Logger.CaptureFineLocalizer(VisionStatus.OK, "gap", _config[-1].CarName, $"{_currentCarSeq}");
            }
            else
            {
                Invoke(new MethodInvoker(() => DisplayGapVisionStatus(VisionStatus.NG)));
                Logger.CaptureFineLocalizer(VisionStatus.NG, "gap", _config[-1].CarName, $"{_currentCarSeq}");
            }
        }

        private void btnGapUpdate_Click(object sender, EventArgs e)
        {
            btnGapUpdate_.Enabled = false;
        }

        private async Task<(bool isSuccess, double[] gapValues)> CalculateGapAsync()
        {
            var ret = await _gapChecker.MeasureGapAsync();

            Invoke(new MethodInvoker(() =>
            {
                pictureBoxGap1.Image = ret.resultImages[0];
                pictureBoxGap2.Image = ret.resultImages[1];
                pictureBoxGap3.Image = ret.resultImages[2];
                pictureBoxGap4.Image = ret.resultImages[3];

                tbGap1.Text = $"{ret.gapAverages[0]:F3}";
                tbGap2.Text = $"{ret.gapAverages[1]:F3}";
                tbGap3.Text = $"{ret.gapAverages[2]:F3}";
                tbGap4.Text = $"{ret.gapAverages[3]:F3}";

                tbNumGapExamined1.Text = $"{ret.numsGapExamined[0]}";
                tbNumGapExamined2.Text = $"{ret.numsGapExamined[1]}";
                tbNumGapExamined3.Text = $"{ret.numsGapExamined[2]}";
                tbNumGapExamined4.Text = $"{ret.numsGapExamined[3]}";
            }));

            if (ret.isSuccess)
            {
                Logger.Info(Lang.LogsFineLo.GapCheckSuccess);
                Invoke(new MethodInvoker(() => btnGapUpdate_.Enabled = _mode != OperationMode.Auto));
                Logger.WriteGapValues(_currentCar, _config[-1].CarName, _currentCarSeq, -1, ret.gapAverages);
            }
            else
            {
                Logger.Warning(Lang.LogsFineLo.GapCheckFailed);
            }

            return (ret.isSuccess, ret.gapAverages);
        }
    }
}
