using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CommonUtils;

namespace FineLocalizer
{
    public partial class FineLocalizerForm_
    {
        private async Task<bool> WaitForKawaCompletedAsync(string varName, int waitingDelay,
                                                           Dictionary<RobotAttribute, string> robotConf)
        {
            using (var kawa = new KawaComm(robotConf))
            {
                while (_mode == OperationMode.Auto)
                {
                    int? value = await kawa.ReadIntVarAsync(varName);
                    if ((value ?? 0) == 1)
                    {
                        return await kawa.WriteIntVarAsync(0, varName);
                    }
                    else
                    {
                        await Task.Delay(waitingDelay);
                    }
                }

                Logger.Info(Lang.LogsFineLo.StopForSetMode);
                return false;
            }
        }

        private async void RunAutoWithRobot()
        {
            //btnAutoWithRobot.Enabled = false;

            if (_mode == OperationMode.Set)
            {
                Logger.Info(Lang.LogsFineLo.StopForSetMode);
                //btnAutoWithRobot.Enabled = true;
                return;
            }

            if (!_isCameraConnected)
            {
                Logger.Warning(Lang.LogsFineLo.CameraConnectionNeeded);
                //btnAutoWithRobot.Enabled = true;
                return;
            }

            Logger.Info(Lang.LogsFineLo.RobotVisionAutoModeStart);

            string glassRcVar = "glass_rc";
            string glassVcVar = "glass_vc";
            string gapRcVar = "gap_rc";
            string gapVcVar = "gap_vc";
            string vehicleRcVar = "qx_rc";
            string vehicleVcVar = "qx_vc";
            var installRobotConf = _config.RobotConfigs[_config[-1].InstallRobot];

            using (var kawa = new KawaComm(installRobotConf))
            {
                Logger.Info(Lang.LogsFineLo.VariableInitStart);
                bool retGlassRc = await kawa.WriteIntVarAsync(0, glassRcVar);
                bool retGlassVc = await kawa.WriteIntVarAsync(0, glassVcVar);
                bool retGapRc = await kawa.WriteIntVarAsync(0, gapRcVar);
                bool retGapVc = await kawa.WriteIntVarAsync(0, gapVcVar);
                bool retVehicleRc = await kawa.WriteIntVarAsync(0, vehicleRcVar);
                bool retVehicleVc = await kawa.WriteIntVarAsync(0, vehicleVcVar);

                if (retGlassRc && retGlassVc && retGapRc && retGapVc && retVehicleRc && retVehicleVc)
                {
                    Logger.Info(Lang.LogsFineLo.VariableInitDone);
                }
                else
                {
                    Logger.Error(Lang.LogsFineLo.VariableInitFailed);
                }
            }

            if (await WaitForKawaCompletedAsync(glassRcVar, 500, installRobotConf))
            {
                Logger.Info(Lang.LogsFineLo.SignalGlassRcReceived);
            }
            else
            {
                Logger.Error(Lang.LogsFineLo.SignalGlassRcInitFailed);
            }

            if (_mode == OperationMode.Set)
            {
                //btnAutoWithRobot.Enabled = true;
                return;
            }

            tabControl1_.SelectedTab = tabControl1_.TabPages["tabPageGlass_"];
            
            try
            {
                await Task.Delay(1000);
                await ScanGlassPoint();
                var ret = await _glassChecker.EstimateGlassGripPoseAsync();
                if (ret.isSuccess)
                {
                    _glassShiftValues = ret.updatingPoses;
                    UpdateGlassShiftValueToUi(_glassShiftValues[0], gbGlassShiftValue_);

                    _glManagerGlass1.UpdatePointCloud(ret.target1, ret.source1, ret.sourceAligned1,
                                                      ret.zAvg, ret.zMin, ret.zMax);

                    _glManagerGlass2.UpdatePointCloud(ret.target2, ret.source2, ret.sourceAligned2,
                                                      ret.zAvg, ret.zMin, ret.zMax);
                }
                else
                {
                    Logger.Warning(Lang.LogsFineLo.GlassGripPoseShiftValueFailed);
                    //btnAutoWithRobot.Enabled = true;
                    return;
                }

                if (!await _glassChecker.UpdateGlassGripPoseAsync(_glassShiftValues))
                {
                    Logger.Warning(Lang.LogsFineLo.GlassGripPoseShiftValueUpdateFailed);
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"{Lang.LogsFineLo.ExceptionInGlassCheck} ({ex})");
                //btnAutoWithRobot.Enabled = true;
                return;
            }

            using (var kawa = new KawaComm(installRobotConf))
            {
                await kawa.WriteIntVarAsync(1, glassVcVar);
            }

            if (await WaitForKawaCompletedAsync(vehicleRcVar, 500, installRobotConf))
            {
                Logger.Info(Lang.LogsFineLo.SignalQxRcReceived);
            }
            else
            {
                Logger.Warning(Lang.LogsFineLo.SignalQxRcInitFailed);
            }

            if (_mode == OperationMode.Set)
            {
                //btnAutoWithRobot.Enabled = true;
                return;
            }

            tabControl1_.SelectedTab = tabControl1_.TabPages["tabPageVehicle_"];

            try
            {
                await ScanPoint(1);
                bool retCalculate = await CalculatePoses();

                if (_config[-1].UseGapChecker)
                {
                    var retGap = await _gapChecker.EstimateAndUpdateGapScanPosesAsync();
                    if (!retGap.isSuccess)
                    {
                        Logger.Warning(Lang.LogsFineLo.GapCheckScanPoseCalculationFailed);
                    }

                    UpdateRobotPoseToUI(retGap.shiftValue, gbGapSV_);
                }

                bool retUpdate = await _fineLocalizerVehicle.UpdateInstallPosesToRobot(_calPositions);

                if (retUpdate && retCalculate)
                {
                    Logger.Info(Lang.LogsFineLo.VehicleCalculationDone);
                }
                else
                {
                    Logger.Warning(Lang.LogsFineLo.VehicleCalculationFailed);
                    //btnAutoWithRobot.Enabled = true;
                    return;
                }
            }
            catch (Exception ex)
            {
                Logger.Warning($"{Lang.LogsFineLo.VehicleCalculationFailed} ({ex})");
                //btnAutoWithRobot.Enabled = true;
                return;
            }

            using (var kawa = new KawaComm(installRobotConf))
            {
                await kawa.WriteIntVarAsync(1, vehicleVcVar);
            }

            if (await WaitForKawaCompletedAsync(gapRcVar, 500, installRobotConf))
            {
                Logger.Info(Lang.LogsFineLo.SignalGapRcReceived);
            }
            else
            {
                Logger.Warning(Lang.LogsFineLo.SignalGapRcInitFailed);
            }

            if (_mode == OperationMode.Set)
            {
                //btnAutoWithRobot.Enabled = true;
                return;
            }

            tabControl1_.SelectedTab = tabControl1_.TabPages["tabPageGap_"];

            try
            {
                await ScanGapPoint(1);
                var ret = await _gapChecker.MeasureGapAsync();
                if (ret.isSuccess)
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

                    Logger.Info(Lang.LogsFineLo.GapCheckSuccess);
                }
                else
                {
                    Logger.Warning(Lang.LogsFineLo.GapCheckFailed);
                }
            }
            catch (Exception ex)
            {
                Logger.Warning($"{Lang.LogsFineLo.ExceptionInGapCheck} ({ex})");
            }

            using (var kawa = new KawaComm(installRobotConf))
            {
                await kawa.WriteIntVarAsync(1, gapVcVar);
            }

            Logger.Info(Lang.LogsFineLo.RobotVisionAutoModeEnd);
            //btnAutoWithRobot.Enabled = true;
        }
    }
}
