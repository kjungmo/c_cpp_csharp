using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Linq;

using CommonUtils;

namespace FineLocalizer
{
    partial class FineLocalizerForm_
    {
        private List<RobotPose> _glassShiftValues;
        private RefPoses _refGlassPoses = new RefPoses()
        {
            Values = new List<RobotPose>()
        };

        private void InitGlassUI()
        {
            ClearRobotPoseUi(gbGlassPoint_);
            ClearGlassShiftValueUi(gbGlassShiftValue_);

            _glManagerGlass1?.Clear();
            _glManagerGlass2?.Clear();

            Invoke(new MethodInvoker(() =>
            {
                btnGlassUpdate_.Enabled = false;
                btnGlassUpdate_.BackColor = Color.DarkGray;

                btnGlassCalculate_.Enabled = false;
                btnGlassCalculate_.BackColor = Color.DarkGray;

                DisplayGlassVisionStatus(VisionStatus.NONE);

                if (_mode == OperationMode.Set)
                {
                    btnGlassPoint_.Text = Lang.FineLo.btnGlassSavePoints;
                    btnGlassPoint_.BackColor = Color.FromArgb(255, 75, 75);
                    btnGlassPoint_.ForeColor = Color.White;
                    btnGlassPoint_.Enabled = true;
                }
                else
                {
                    btnGlassPoint_.Text = Lang.FineLo.btnGlassScanPoints;
                    btnGlassPoint_.BackColor = Color.FromArgb(80, 215, 73);
                    btnGlassPoint_.ForeColor = Color.White;

                    if (_mode == OperationMode.Auto)
                    {
                        btnGlassPoint_.Enabled = false;
                    }
                    else
                    {
                        btnGlassPoint_.Enabled = (_refGlassPoses.Values?.Count ?? 0) > 0;
                    }
                }
            }));
        }

        private bool LoadRefGlassPosesFile()
        {
            if (!_config[-1].UseGlassChecker)
            {
                return false;
            }

            _refGlassPoses = RefPosesFileManager.LoadFromFile($"{_config[-1].GlassRefDataPath}/ref_pose.dat", _config[-1].GlassCamera);
            if (_refGlassPoses.Values == null)
            {
                Logger.Warning(Lang.LogsFineLo.LoadingGlassCheckRefPoseFailed);
                return false;
            }
            else
            {
                Logger.Info(Lang.LogsFineLo.LoadingGlassCheckRefPoseDone);
                return true;
            }
        }

        private async void btnGlassPoint_Click(object sender, EventArgs e)
        {
            var btnSender = sender as Button;
            btnSender.Enabled = false;

            if (!_isCameraConnected)
            {
                Logger.Warning(Lang.LogsFineLo.CameraConnectionNeeded);
                btnSender.Enabled = true;
                return;
            }

            if (!FineLocalizerVehicleEngineAPI.ReadyGlassCheck())
            {
                Logger.Warning(Lang.LogsFineLo.ReadyGlassError);
                return;
            }

            try
            {
                if (_mode == OperationMode.Set)
                {
                    await SaveCurrentRobotPoseAndImageForGlassCheck();
                }
                else
                {
                    await ScanGlassPoint();
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

        private async Task<bool> ScanGlassPoint()
        {
            try
            {
                var curPose = await _glassChecker.Robot.ReadRobotCurrentPoseAsync();
                if (curPose == null)
                {
                    Logger.Error(Lang.LogsFineLo.CantReadRobotCurrentPose);
                    return false;
                }
                UpdateRobotPoseToUI(curPose, gbGlassPoint_);

                bool retInfoCurr = _glassChecker.InfoGlassScanPoseToEngine(curPose);
                bool retInfoRef = _glassChecker.InfoGlassScanRefPoseToEngine(_refGlassPoses.Values[0]);

                var ret = await _glassChecker.TriggerScanGlassAsync(false, false);
                if (ret)
                {
                    Logger.Info(Lang.LogsFineLo.GlassCheckScanCompleted);
                    Invoke(new MethodInvoker(() =>
                    {
                        btnGlassCalculate_.Enabled = _mode != OperationMode.Auto;
                        btnGlassCalculate_.BackColor = Color.Lime;
                    }));
                }
                else
                {
                    Logger.Warning(Lang.LogsFineLo.GlassCheckScanFailed);
                }

                return ret;
            }
            catch (Exception ex)
            {
                Logger.Error($"{Lang.LogsFineLo.GlassCheckScanFailed} ({ex})");
                return false;
            }
        }

        private async Task SaveCurrentRobotPoseAndImageForGlassCheck()
        {
            bool saveImg = MessageBox.Show($"{Lang.MsgBoxFineLo.WantSaveRefImg}", "", MessageBoxButtons.YesNo) == DialogResult.Yes;
            bool savePose = MessageBox.Show($"{Lang.MsgBoxFineLo.WantSaveRefPose}", "", MessageBoxButtons.YesNo) == DialogResult.Yes;

            var curPose = await _glassChecker.Robot.ReadRobotCurrentPoseAsync();
            UpdateRobotPoseToUI(curPose, gbGlassPoint_);

            bool retInfoCurr = _glassChecker.InfoGlassScanPoseToEngine(curPose);
            
            if (await _glassChecker.TriggerScanGlassAsync(true, saveImg))
            {
                Logger.Info($"{Lang.LogsFineLo.ScanningPointDone}");
            }
            else
            {
                Logger.Error($"{Lang.LogsFineLo.ScanningPointFailed}");
                return;
            }

            if (savePose)
            {
                try
                {
                    if (!Directory.Exists(_config[-1].GlassRefDataPath))
                    {
                        Directory.CreateDirectory(_config[-1].GlassRefDataPath);
                    }

                    if (_refGlassPoses.Values == null) _refGlassPoses.Values = new List<RobotPose> { curPose };
                    else if (_refGlassPoses.Values.Count < 1) _refGlassPoses.Values.Add(curPose);
                    else _refGlassPoses.Values[0] = curPose;

                    RefPosesFileManager.SaveToFile(_refGlassPoses, $"{_config[-1].GlassRefDataPath}/ref_pose.dat", _config[-1].GlassCamera);

                    Logger.Info($"{Lang.LogsFineLo.SavingRefPoseDone}");
                }
                catch (Exception ex)
                {
                    Logger.Error($"{Lang.LogsFineLo.SavingRefPoseFailed} <{ex.Message}>");
                }
            }
        }

        private async void btnGlassCalculate_Click(object sender, EventArgs e)
        {
            btnGlassCalculate_.Enabled = false;
            if (await CalculateGlassAsync())
            {
                Invoke(new MethodInvoker(() => DisplayGlassVisionStatus(VisionStatus.OK)));
                Logger.CaptureFineLocalizer(VisionStatus.OK, "glass", _config[-1].CarName, $"{_currentCarSeq}");
            }
            else
            {
                Invoke(new MethodInvoker(() => DisplayGlassVisionStatus(VisionStatus.NG)));
                Logger.CaptureFineLocalizer(VisionStatus.NG, "glass", _config[-1].CarName, $"{_currentCarSeq}");
            }
        }

        private async void btnGlassUpdate_Click(object sender, EventArgs e)
        {
            btnGlassUpdate_.Enabled = false;
            await UpdateGlassPoseAsync();
        }

        private async Task<bool> CalculateGlassAsync()
        {
            try
            {
                var ret = await _glassChecker.EstimateGlassGripPoseAsync();

                if (ret.target1.Length > 0 && ret.source1.Length > 0 && ret.sourceAligned1.Length > 0)
                {
                    _glManagerGlass1.UpdatePointCloud(ret.target1, ret.source1, ret.sourceAligned1,
                                                      ret.zAvg, ret.zMin, ret.zMax);
                }

                if (ret.target2.Length > 0 && ret.source2.Length > 0 && ret.sourceAligned2.Length > 0)
                {
                    _glManagerGlass2.UpdatePointCloud(ret.target2, ret.source2, ret.sourceAligned2,
                                                      ret.zAvg, ret.zMin, ret.zMax);
                }

                _glassShiftValues = ret.updatingPoses;
                UpdateGlassShiftValueToUi(_glassShiftValues[0], gbGlassShiftValue_);
                Logger.WriteGlassShiftValue(_currentCar, _config[-1].CarName, _currentCarSeq, -1, ret.isSuccess, _glassShiftValues[0],
                                            _config.RobotPoseVariables.TryGetValue($"{_currentCar}_glass_offset", out var offset) ? offset : new RobotPoseVariable());

                if (ret.isSuccess)
                {
                    Logger.Info(Lang.LogsFineLo.GlassGripPoseShiftValueCalculated);

                    Invoke(new MethodInvoker(() =>
                    {
                        btnGlassUpdate_.Enabled = _mode != OperationMode.Auto;
                        btnGlassUpdate_.BackColor = Color.Lime;
                    }));
                }
                else
                {
                    Logger.Error(Lang.LogsFineLo.GlassGripPoseShiftValueFailed);
                }

                return ret.isSuccess;
            }
            catch (Exception ex)
            {
                Logger.Error($"{Lang.LogsFineLo.GlassGripPoseShiftValueFailed} ({ex})");
                return false;
            }
        }

        private async Task<bool> UpdateGlassPoseAsync()
        {
            try
            {
                if (!_config.RobotPoseVariables.TryGetValue($"{_config.RecentlyUsedCar}_glass_offset", out var offset))
                {
                    offset = new RobotPoseVariable();
                }

                List<RobotPose> glassShiftValuesToUpdate;
                if (_config[-1].UpdateToVehicleShift)
                {
                    glassShiftValuesToUpdate = _glassShiftValues.Select(s => new RobotPose()).ToList();
                }
                else
                {
                    glassShiftValuesToUpdate = _glassShiftValues.Select(s => s + offset).ToList();
                }
                bool res = await _glassChecker.UpdateGlassGripPoseAsync(glassShiftValuesToUpdate);

                Logger.Info($"{Lang.LogsFineLo.CalculatedGlassShiftValue}: {_glassShiftValues[0]}");
                Logger.Info($"{Lang.LogsFineLo.GlassOffsetValue}: {offset}");
                Logger.Info($"{Lang.LogsFineLo.ModifiedGlassShiftValue}: {glassShiftValuesToUpdate[0]}");

                if (res)
                {
                    Logger.Info(Lang.LogsFineLo.GlassGripPoseShiftValueUpdateCompleted);
                }
                else
                { 
                    Logger.Error(Lang.LogsFineLo.GlassGripPoseShiftValueUpdateFailed);
                }

                return res;
            }
            catch (Exception ex)
            {
                Logger.Error($"{Lang.LogsFineLo.GlassGripPoseShiftValueUpdateFailed} ({ex})");
                return false;
            }
        }
    }
}
