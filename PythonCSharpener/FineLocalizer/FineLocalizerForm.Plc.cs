using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Drawing;
using CommonUtils;

namespace FineLocalizer
{
    partial class FineLocalizerForm_
    {
        private void ResetAllPlcSignalStatus()
        {
            foreach (var lblSig in _plcSignalLabelDict.Values)
            {
                lblSig.BackColor = Color.White;
            }
        }

        private void UpdateS7PlcSignalStatus()
        {
            foreach (var mInfo in (_plcComm as S7PLCCommunicator<PlcSignalForLocalizer>).MonitorDbInfoList)
            {
                foreach (var sig in mInfo.SignalDict.Keys)
                {
                    if (mInfo.SignalDict[sig].IsOn)
                    {
                        _plcSignalLabelDict[sig].BackColor = Color.LightGreen;
                    }
                    else
                    {
                        _plcSignalLabelDict[sig].BackColor = Color.White;
                    }
                }
            }
        }

        private void UpdateMelsecPlcSignalStatus()
        {
            foreach (var mInfo in (_plcComm as MelsecPLCCommunicator<PlcSignalForLocalizer>).MonitorDeviceInfoList)
            {
                foreach (var sig in mInfo.SignalDict.Keys)
                {
                    if (!_plcSignalLabelDict.ContainsKey(sig)) continue;

                    if (mInfo.DataParseType == PlcDataType.WORD)
                    {
                        _plcSignalLabelDict[sig].Text = mInfo.SignalDict[sig].IntValue.ToString();
                    }
                    else if (mInfo.DataParseType == PlcDataType.FLOAT)
                    {
                        _plcSignalLabelDict[sig].Text = mInfo.SignalDict[sig].FloatValue.ToString();
                    }
                    else
                    {
                        if (mInfo.SignalDict[sig].IsOn)
                        {
                            _plcSignalLabelDict[sig].BackColor = Color.LightGreen;
                        }
                        else
                        {
                            _plcSignalLabelDict[sig].BackColor = Color.White;
                        }
                    }
                }
            }
        }

        private void RegisterPlcEventHandlers()
        {
            if (_isPlcEvtHandlersRegistered) return;

            _plcComm.PlcSent += (s, args) =>
            {
                BeginInvoke(new MethodInvoker(async () =>
                {
                    await _plcStatusPainter.BlinkWriteStatus(100);
                }));
            };

            _plcComm.PlcReceived += (s, args) =>
            {
                BeginInvoke(new MethodInvoker(() =>
                {
                    _ = _plcStatusPainter.BlinkReadStatus(100);
                    _updatePlcSignalStatus();
                }));
            };

            _plcComm.PlcDisconnected += (s, args) =>
            {
                BeginInvoke(new MethodInvoker(async() =>
                {
                    IsPLCConnected = false;
                    _plcStatusPainter.DrawConnStatus(PlcStatus.OFF);

                    if (_mode == OperationMode.Auto)
                    {
                        await _plcComm.ConnectAsync();
                    }
                }));
            };

            _plcComm.PlcConnected += (s, args) =>
            {
                BeginInvoke(new MethodInvoker(() =>
                {
                    IsPLCConnected = true;
                    _plcStatusPainter.DrawConnStatus(PlcStatus.ON);
                    _plcComm.StartHeartbeat(500);
                    _plcComm.StartMonitoring(550);
                }));
            };

            _plcForThisTask.GlassVisionStart += PlcForThisTask_GlassVisionStart;
            _plcForThisTask.BodyVisionStart += PlcForThisTask_BodyVisionStart;
            _plcForThisTask.GapVisionStart += PlcForThisTask_GapVisionStart;
            _plcForThisTask.GlassVisionReset += PlcForThisTask_GlassVisionReset;
            _plcForThisTask.BodyVisionReset += PlcForThisTask_BodyVisionReset;
            _plcForThisTask.GapVisionReset += PlcForThisTask_GapVisionReset;
            _plcForThisTask.VisionEnd += PlcForThisTask_VisionEnd;
            _plcForThisTask.CarTypeUpdate += PlcForThisTask_CarTypeUpdate;

            _isPlcEvtHandlersRegistered = true;
        }

        private void SetS7PlcToUse()
        {
            var plcConf = _config.PlcConfigs[PlcModel.SIEMENS];
            Invoke(new MethodInvoker(() =>
            {
                lblBodyVisionStart.Text = "VISION START (DBX0.0)";
                lblBodyVisionReset.Text = "POINT 1 (DBX1.0)";
                lblBodyP1.Text = "POINT 3 (DBX1.2)";
                lblBodyP2.Text = "POINT 4 (DBX1.3)";
                lblBodyP1Completed.Text = "P1 COMPLETED (DBX1.0)";
                lblBodyP2Completed.Text = "P2 COMPLETED (DBX1.1)";
                lblBodyP3Completed.Text = "P3 COMPLETED (DBX1.2)";
                lblBodyP4Completed.Text = "P4 COMPLETED (DBX1.3)";
                lblBodyVisionOk.Text = "VISION OK (DBX1.4)";
                lblBodyVisionNg.Text = "VISION NG (DBX1.5)";
            }));
            _plcComm = new S7Comm(plcConf, 100, 0,
                                  int.Parse(plcConf[PlcAttribute.READ_DB]),
                                  int.Parse(plcConf[PlcAttribute.WRITE_DB]));
            _plcForThisTask = _plcComm as IPlcForLocalizer;
            _updatePlcSignalStatus = UpdateS7PlcSignalStatus;

            _isPlcEvtHandlersRegistered = false;
        }

        private void SetMelsecPlcToUse()
        {
            var plcConf = _config.PlcConfigs[PlcModel.MELSEC];

            _plcComm = new MelsecComm(plcConf, "D", PlcDataType.WORD, 1220, 15);
            _plcForThisTask = _plcComm as IPlcForLocalizer;
            _updatePlcSignalStatus = UpdateMelsecPlcSignalStatus;
            if (_plcSignalLabelDict != null) LoadMelsecPlcSingalText();

            _isPlcEvtHandlersRegistered = false;
        }

        void LoadMelsecPlcSingalText()
        {
            Invoke(new MethodInvoker(() =>
            {
                foreach (var mon in (_plcComm as MelsecComm).MonitorDeviceInfoList)
                {
                    foreach (var kv in mon.SignalDict)
                    {
                        PlcSignalForLocalizer sig = kv.Key;
                        Label lbl = _plcSignalLabelDict[sig.HasFlag(PlcSignalForLocalizer.VALUE) ? sig & ~PlcSignalForLocalizer.VALUE : sig];
                        var title = sig & ~PlcSignalForLocalizer.BODY & ~PlcSignalForLocalizer.GLASS & ~PlcSignalForLocalizer.GAP & ~PlcSignalForLocalizer.VALUE;
                        lbl.Text = $"{title} ({mon.DeviceName}{kv.Value})";
                    }
                }
            }));
        }

        private void LoadPlcSignalLabelDict()
        {
            _plcSignalLabelDict = new Dictionary<PlcSignalForLocalizer, Label>()
            {
                { PlcSignalForLocalizer.CAR_TYPE, lblGlassCarType },
                { PlcSignalForLocalizer.CAR_SEQ, lblGlassCarSeq },
                { PlcSignalForLocalizer.CAR_TYPE | PlcSignalForLocalizer.VALUE, lblGlassCarTypeVal },
                { PlcSignalForLocalizer.CAR_SEQ | PlcSignalForLocalizer.VALUE, lblGlassCarSeqVal },
                { PlcSignalForLocalizer.GLASS | PlcSignalForLocalizer.VISION_START, lblGlassVisionStart },
                { PlcSignalForLocalizer.GLASS | PlcSignalForLocalizer.VISION_RESET, lblGlassVisionReset },
                { PlcSignalForLocalizer.VISION_UPDATE, lblVisionUpdate },
                { PlcSignalForLocalizer.GLASS | PlcSignalForLocalizer.P1, lblGlassP1 },
                { PlcSignalForLocalizer.GLASS | PlcSignalForLocalizer.P2, lblGlassP2 },
                { PlcSignalForLocalizer.GLASS | PlcSignalForLocalizer.P3, lblGlassP3 },
                { PlcSignalForLocalizer.GLASS | PlcSignalForLocalizer.P4, lblGlassP4 },
                { PlcSignalForLocalizer.GLASS | PlcSignalForLocalizer.VISION_PASS, lblGlassVisionPass },
                { PlcSignalForLocalizer.GLASS | PlcSignalForLocalizer.VISION_OK, lblGlassVisionOk },
                { PlcSignalForLocalizer.GLASS | PlcSignalForLocalizer.VISION_NG, lblGlassVisionNg },
                { PlcSignalForLocalizer.GLASS | PlcSignalForLocalizer.P1_COMPLETED, lblGlassP1Completed },
                { PlcSignalForLocalizer.GLASS | PlcSignalForLocalizer.P2_COMPLETED, lblGlassP2Completed },
                { PlcSignalForLocalizer.GLASS | PlcSignalForLocalizer.P3_COMPLETED, lblGlassP3Completed },
                { PlcSignalForLocalizer.GLASS | PlcSignalForLocalizer.P4_COMPLETED, lblGlassP4Completed },
                { PlcSignalForLocalizer.GLASS | PlcSignalForLocalizer.PLC_PASS, lblGlassPlcPass },
                { PlcSignalForLocalizer.GLASS | PlcSignalForLocalizer.SHIFT_X, lblGlassSvTx },
                { PlcSignalForLocalizer.GLASS | PlcSignalForLocalizer.SHIFT_Y, lblGlassSvTy },
                { PlcSignalForLocalizer.GLASS | PlcSignalForLocalizer.SHIFT_Z, lblGlassSvTz },
                { PlcSignalForLocalizer.GLASS | PlcSignalForLocalizer.SHIFT_RX, lblGlassSvRx },
                { PlcSignalForLocalizer.GLASS | PlcSignalForLocalizer.SHIFT_RY, lblGlassSvRy },
                { PlcSignalForLocalizer.GLASS | PlcSignalForLocalizer.SHIFT_RZ, lblGlassSvRz },
                { PlcSignalForLocalizer.GLASS | PlcSignalForLocalizer.SHIFT_X | PlcSignalForLocalizer.VALUE, lblGlassSvTxVal },
                { PlcSignalForLocalizer.GLASS | PlcSignalForLocalizer.SHIFT_Y | PlcSignalForLocalizer.VALUE, lblGlassSvTyVal },
                { PlcSignalForLocalizer.GLASS | PlcSignalForLocalizer.SHIFT_Z | PlcSignalForLocalizer.VALUE, lblGlassSvTzVal },
                { PlcSignalForLocalizer.GLASS | PlcSignalForLocalizer.SHIFT_RX | PlcSignalForLocalizer.VALUE, lblGlassSvRxVal },
                { PlcSignalForLocalizer.GLASS | PlcSignalForLocalizer.SHIFT_RY | PlcSignalForLocalizer.VALUE, lblGlassSvRyVal },
                { PlcSignalForLocalizer.GLASS | PlcSignalForLocalizer.SHIFT_RZ | PlcSignalForLocalizer.VALUE, lblGlassSvRzVal },

                { PlcSignalForLocalizer.BODY | PlcSignalForLocalizer.VISION_START, lblBodyVisionStart },
                { PlcSignalForLocalizer.BODY | PlcSignalForLocalizer.VISION_RESET, lblBodyVisionReset },
                { PlcSignalForLocalizer.BODY | PlcSignalForLocalizer.P1, lblBodyP1 },
                { PlcSignalForLocalizer.BODY | PlcSignalForLocalizer.P2, lblBodyP2 },
                { PlcSignalForLocalizer.BODY | PlcSignalForLocalizer.P3, lblBodyP3 },
                { PlcSignalForLocalizer.BODY | PlcSignalForLocalizer.P4, lblBodyP4 },
                { PlcSignalForLocalizer.BODY | PlcSignalForLocalizer.VISION_PASS, lblBodyVisionPass },
                { PlcSignalForLocalizer.BODY | PlcSignalForLocalizer.VISION_OK, lblBodyVisionOk },
                { PlcSignalForLocalizer.BODY | PlcSignalForLocalizer.VISION_NG, lblBodyVisionNg },
                { PlcSignalForLocalizer.BODY | PlcSignalForLocalizer.P1_COMPLETED, lblBodyP1Completed },
                { PlcSignalForLocalizer.BODY | PlcSignalForLocalizer.P2_COMPLETED, lblBodyP2Completed },
                { PlcSignalForLocalizer.BODY | PlcSignalForLocalizer.P3_COMPLETED, lblBodyP3Completed },
                { PlcSignalForLocalizer.BODY | PlcSignalForLocalizer.P4_COMPLETED, lblBodyP4Completed },
                { PlcSignalForLocalizer.BODY | PlcSignalForLocalizer.PLC_PASS, lblBodyPlcPass },
                { PlcSignalForLocalizer.BODY | PlcSignalForLocalizer.SHIFT_X, lblBodySvTx },
                { PlcSignalForLocalizer.BODY | PlcSignalForLocalizer.SHIFT_Y, lblBodySvTy },
                { PlcSignalForLocalizer.BODY | PlcSignalForLocalizer.SHIFT_Z, lblBodySvTz },
                { PlcSignalForLocalizer.BODY | PlcSignalForLocalizer.SHIFT_RX, lblBodySvRx },
                { PlcSignalForLocalizer.BODY | PlcSignalForLocalizer.SHIFT_RY, lblBodySvRy },
                { PlcSignalForLocalizer.BODY | PlcSignalForLocalizer.SHIFT_RZ, lblBodySvRz },
                { PlcSignalForLocalizer.BODY | PlcSignalForLocalizer.SHIFT_X | PlcSignalForLocalizer.VALUE, lblBodySvTxVal },
                { PlcSignalForLocalizer.BODY | PlcSignalForLocalizer.SHIFT_Y | PlcSignalForLocalizer.VALUE, lblBodySvTyVal },
                { PlcSignalForLocalizer.BODY | PlcSignalForLocalizer.SHIFT_Z | PlcSignalForLocalizer.VALUE, lblBodySvTzVal },
                { PlcSignalForLocalizer.BODY | PlcSignalForLocalizer.SHIFT_RX | PlcSignalForLocalizer.VALUE, lblBodySvRxVal },
                { PlcSignalForLocalizer.BODY | PlcSignalForLocalizer.SHIFT_RY | PlcSignalForLocalizer.VALUE, lblBodySvRyVal },
                { PlcSignalForLocalizer.BODY | PlcSignalForLocalizer.SHIFT_RZ | PlcSignalForLocalizer.VALUE, lblBodySvRzVal },

                { PlcSignalForLocalizer.GAP | PlcSignalForLocalizer.VISION_START, lblGapVisionStart },
                { PlcSignalForLocalizer.VISION_END, lblGapVisionEnd },
                { PlcSignalForLocalizer.GAP | PlcSignalForLocalizer.VISION_RESET, lblGapVisionReset },
                { PlcSignalForLocalizer.GAP | PlcSignalForLocalizer.P1, lblGapP1 },
                { PlcSignalForLocalizer.GAP | PlcSignalForLocalizer.P2, lblGapP2 },
                { PlcSignalForLocalizer.GAP | PlcSignalForLocalizer.P3, lblGapP3 },
                { PlcSignalForLocalizer.GAP | PlcSignalForLocalizer.P4, lblGapP4 },
                { PlcSignalForLocalizer.GAP | PlcSignalForLocalizer.VISION_PASS, lblGapVisionPass },
                { PlcSignalForLocalizer.GAP | PlcSignalForLocalizer.VISION_OK, lblGapVisionOk },
                { PlcSignalForLocalizer.GAP | PlcSignalForLocalizer.VISION_NG, lblGapVisionNg },
                { PlcSignalForLocalizer.GAP | PlcSignalForLocalizer.SCAN_POSE_SENT, lblGapScanPoseSent },
                { PlcSignalForLocalizer.GAP | PlcSignalForLocalizer.P1_COMPLETED, lblGapP1Completed },
                { PlcSignalForLocalizer.GAP | PlcSignalForLocalizer.P2_COMPLETED, lblGapP2Completed },
                { PlcSignalForLocalizer.GAP | PlcSignalForLocalizer.P3_COMPLETED, lblGapP3Completed },
                { PlcSignalForLocalizer.GAP | PlcSignalForLocalizer.P4_COMPLETED, lblGapP4Completed },
                { PlcSignalForLocalizer.GAP | PlcSignalForLocalizer.PLC_PASS, lblGapPlcPass },
                { PlcSignalForLocalizer.GAP_1, lblGap1 },
                { PlcSignalForLocalizer.GAP_2, lblGap2 },
                { PlcSignalForLocalizer.GAP_3, lblGap3 },
                { PlcSignalForLocalizer.GAP_4, lblGap4 },
                { PlcSignalForLocalizer.GAP_1 | PlcSignalForLocalizer.VALUE, lblGap1Val },
                { PlcSignalForLocalizer.GAP_2 | PlcSignalForLocalizer.VALUE, lblGap2Val },
                { PlcSignalForLocalizer.GAP_3 | PlcSignalForLocalizer.VALUE, lblGap3Val },
                { PlcSignalForLocalizer.GAP_4 | PlcSignalForLocalizer.VALUE, lblGap4Val }
            };
        }

        private PlcSignalForLocalizer ConvertPointNumberToPlcSignal(int pointNum)
        {
            return (PlcSignalForLocalizer)(1 << (9 + pointNum));
        }

        private async Task<bool> WaitForCameraConnection(int delay, int nMaxTrials)
        {
            for (var i = 0; i < nMaxTrials && !_isCameraConnected; ++i)
            {
                Logger.Info(Lang.LogsFineLo.WaitingForCameraConnection);
                await Task.Delay(delay);
            }

            return _isCameraConnected;
        }

        private void PlcForThisTask_GlassVisionReset(object sender, LocalizerEventArgs e)
        {
            //Invoke(new MethodInvoker(() =>
            //{
            //    ClearRobotPoseUi(gbGlassPoint_);
            //    ClearRobotPoseUi(gbGlassShiftValue_);
            //}));
        }

        private void PlcForThisTask_BodyVisionReset(object sender, LocalizerEventArgs e)
        {
            //Invoke(new MethodInvoker(() =>
            //{
            //    foreach (var gb in _gbPoints)
            //    {
            //        ClearRobotPoseUi(gb);
            //    }

            //    ClearRobotPoseUi(gbSV_);
            //}));
        }

        private void PlcForThisTask_GapVisionReset(object sender, LocalizerEventArgs e)
        {
            //Invoke(new MethodInvoker(() =>
            //{
            //    foreach (var gb in _gbGapPoints)
            //    {
            //        ClearRobotPoseUi(gb);
            //    }

            //    ClearRobotPoseUi(gbGapSV_);

            //    tbGap1.Text = "";
            //    tbGap2.Text = "";
            //    tbGap3.Text = "";
            //    tbGap4.Text = "";

            //    tbNumGapExamined1.Text = "";
            //    tbNumGapExamined2.Text = "";
            //    tbNumGapExamined3.Text = "";
            //    tbNumGapExamined4.Text = "";
            //}));
        }

        private async void PlcForThisTask_VisionEnd(object sender, OnOffEventArgs e)
        {
            Logger.Info(Lang.LogsFineLo.SignalVisionEndReceived);

            if (_isConfigModified)
            {
                _currentCar = -3;
                InfoSettingsToEngine();
                _isConfigModified = false;
            }
            if (_melsecPlcDataTransmitter != null)
            {
                await _melsecPlcDataTransmitter.InitMelsecPlcDataProcessAsync();
            }
        }

        private async void PlcForThisTask_CarTypeUpdate(object sender, CarTypeUpdateEventArgs e)
        {
            if (!await WaitForCameraConnection(250, 20))
            {
                Logger.Error(Lang.LogsFineLo.StopForCameraConnectionFailed);
                return;
            }

            InitVehicleUI();
            InitGapUI();
            InitGlassUI();
            
            Logger.Info($"{Lang.LogsFineLo.SignalVisionUpdateReceived} " +
                        $"({Lang.LogsFineLo.CarSeqNum}: {e.CarSeq}, {Lang.LogsFineLo.CarTypeNum}: {e.CarType})");

            _currentCarSeq = e.CarSeq;
            BeginInvoke(new MethodInvoker(() =>
            {
                string strCarSeq = _currentCarSeq.ToString();
                tbCarSeqNumber.Text = strCarSeq;
                _dashBoard?.LoadCarSeqNum(strCarSeq);
            }));

            if (e.CarType != _currentCar && _config.GetCarTypeList().Contains(e.CarType))
            {
                Logger.Info($"{Lang.LogsFineLo.CarTypeChanged} : {e.CarType}");
                Invoke(new MethodInvoker(() => comboBoxCarType.SelectedItem = e.CarType));
            }

            _isGlassCheckReady = false;
            _isVehicleCheckReady = false;
            _isGapCheckReady = false;

            if (_config[-1].UseGlassChecker)
            {
                _isGlassCheckReady = FineLocalizerVehicleEngineAPI.ReadyGlassCheck();
            }
            else
            {
                _isVehicleCheckReady = await _fineLocalizerVehicle.ReadyVehicleCheckerAsync();
            }
        }

        private async Task DoGlassVisionPass(int pointNum)
        {
            bool isScanDone = _isGlassCheckReady && await ScanGlassPoint();
            bool isCompletedSigSent = await _plcForThisTask.SendLocalizerStatus(PlcSignalForLocalizer.GLASS | ConvertPointNumberToPlcSignal(pointNum), true, 15, 200) >= 0;

            if (pointNum == _refGlassPoses.Values.Count)
            {
                if (isScanDone && isCompletedSigSent)
                {
                    await CalculateGlassAsync();
                }

                var zeroSvs = new List<RobotPose>() { new RobotPose() };

                if (!await _glassChecker.UpdateGlassGripPoseAsync(zeroSvs))
                {
                    Logger.Error(Lang.LogsFineLo.GlassGripPoseShiftValueUpdateFailed);
                }

                await _plcForThisTask.SendShiftValue(PlcSignalForLocalizer.GLASS, zeroSvs[0], 15, 200);
                await _plcForThisTask.SendLocalizerStatus(PlcSignalForLocalizer.GLASS | PlcSignalForLocalizer.VISION_OK, true, 15, 200);
                await _plcForThisTask.SendLocalizerStatus(PlcSignalForLocalizer.GLASS | PlcSignalForLocalizer.VISION_START, false, 15, 200);
                _isVehicleCheckReady = await _fineLocalizerVehicle.ReadyVehicleCheckerAsync();

                Invoke(new MethodInvoker(() => DisplayGlassVisionStatus(VisionStatus.OK)));
                Logger.CaptureFineLocalizer(VisionStatus.OK, "glass", _config[-1].CarName, _currentCarSeq.ToString());
            }
        }

        private async Task DoBodyVisionPass(int pointNum)
        {
            bool isScanDone = _isVehicleCheckReady && await ScanPoint(pointNum);
            bool isSigSent = await _plcForThisTask.SendLocalizerStatus(PlcSignalForLocalizer.BODY | ConvertPointNumberToPlcSignal(pointNum), true, 15, 200) >= 0;

            if (pointNum == _refPoses.Values.Count)
            {
                if (isScanDone && isSigSent)
                {
                    await CalculatePoses();
                }

                var zeroSvs = new List<RobotPose>() { new RobotPose() };
                await _fineLocalizerVehicle.UpdateInstallPosesToRobot(zeroSvs);

                await _plcForThisTask.SendShiftValue(PlcSignalForLocalizer.BODY, zeroSvs[0], 15, 200);
                await _plcForThisTask.SendLocalizerStatus(PlcSignalForLocalizer.BODY | PlcSignalForLocalizer.VISION_OK, true, 15, 200);
                await _plcForThisTask.SendLocalizerStatus(PlcSignalForLocalizer.BODY | PlcSignalForLocalizer.VISION_START, false, 15, 200);

                if (_config[-1].UseGapChecker)
                {
                    var retGap = await _gapChecker.EstimateAndUpdateGapScanPosesAsync();
                    if (retGap.isSuccess)
                    {
                        UpdateRobotPoseToUI(retGap.shiftValue, gbGapSV_);
                        await _plcForThisTask.SendLocalizerStatus(PlcSignalForLocalizer.GAP | PlcSignalForLocalizer.SCAN_POSE_SENT, true, 15, 200);
                    }
                    
                    _isGapCheckReady = FineLocalizerVehicleEngineAPI.ReadyGapCheck();
                }

                Invoke(new MethodInvoker(() => DisplayVehicleVisionStatus(VisionStatus.OK)));
                Logger.CaptureFineLocalizer(VisionStatus.OK, "vehicle", _config[-1].CarName, _currentCarSeq.ToString());
            }
        }

        private async Task DoGapVisionPass(int pointNum)
        {
            await ScanGapPoint(pointNum);
            bool isSigsSent = await _plcForThisTask.SendLocalizerStatus(PlcSignalForLocalizer.GAP | ConvertPointNumberToPlcSignal(pointNum), true, 15, 200) >= 0;

            if (_isGapCheckReady && isSigsSent)
            {
                (bool isSuccess, double[] gapAverages) = await CalculateGapAsync();
                Logger.Info(Lang.LogsFineLo.GapCheckCalculationDone);
                Logger.WriteGapValues(_currentCar, _config[-1].CarName, _currentCarSeq, -1, gapAverages);
            }

            await _plcForThisTask.SendGapValue(new float[] { 0, 0, 0, 0 }, 15, 200);
            await _plcForThisTask.SendLocalizerStatus(PlcSignalForLocalizer.GAP | PlcSignalForLocalizer.VISION_OK, true, 15, 200);
            await _plcForThisTask.SendLocalizerStatus(PlcSignalForLocalizer.GAP | PlcSignalForLocalizer.VISION_START, false, 15, 200);
            Invoke(new MethodInvoker(() => DisplayGapVisionStatus(VisionStatus.OK)));

            Logger.CaptureFineLocalizer(VisionStatus.OK, "gap", _config[-1].CarName, _currentCarSeq.ToString());
        }

        private async void PlcForThisTask_GlassVisionStart(object sender, LocalizerEventArgs e)
        {
            if (_mode == OperationMode.Set) return;

            Logger.Info($"{Lang.LogsFineLo.SignalGlassVisionStartReceived} (point= {e.PointNum})");

            if (_refGlassPoses.Values == null)
            {
                Logger.Error(Lang.LogsFineLo.RefPoseDataNotFound);
                return;
            }

            if (e.PointNum > _refGlassPoses.Values.Count)
            {
                Logger.Warning(Lang.LogsFineLo.InvalidPointNumberInput);
                return;
            }

            if (!await WaitForCameraConnection(250, 20))
            {
                Logger.Error(Lang.LogsFineLo.StopForCameraConnectionFailed);
                return;
            }

            Invoke(new MethodInvoker(() => tabControl1_.SelectedTab = tabControl1_.TabPages["tabPageGlass_"]));

            if (_plcForThisTask.GlassVisionPass)
            {
                await DoGlassVisionPass(e.PointNum);
                return;
            }

            if (_isGlassCheckReady && await ScanGlassPoint())
            {
                int ret = await _plcForThisTask.SendLocalizerStatus(PlcSignalForLocalizer.GLASS | ConvertPointNumberToPlcSignal(e.PointNum), true, 15, 200);
                if (ret < 0)
                {
                    Logger.Error(Lang.LogsFineLo.PointCompletedSignalSentFailed);
                }
                else
                {
                    if (e.PointNum != _refGlassPoses.Values.Count)
                    {
                        return;
                    }

                    if (await CalculateGlassAsync() && await UpdateGlassPoseAsync())
                    {
                        await _plcForThisTask.SendShiftValue(PlcSignalForLocalizer.GLASS, _glassShiftValues[0], 15, 200);
                        await _plcForThisTask.SendLocalizerStatus(PlcSignalForLocalizer.GLASS | PlcSignalForLocalizer.VISION_OK, true, 15, 200);
                        await _plcForThisTask.SendLocalizerStatus(PlcSignalForLocalizer.GLASS | PlcSignalForLocalizer.VISION_START, false, 15, 200);
                        Invoke(new MethodInvoker(() => DisplayGlassVisionStatus(VisionStatus.OK)));

                        _isVehicleCheckReady = await _fineLocalizerVehicle.ReadyVehicleCheckerAsync();
                        Logger.CaptureFineLocalizer(VisionStatus.OK, "glass", _config[-1].CarName, _currentCarSeq.ToString());

                        return;
                    }
                }
            }

            await _plcForThisTask.SendLocalizerStatus(PlcSignalForLocalizer.GLASS | PlcSignalForLocalizer.VISION_NG, true, 15, 200);
            await _plcForThisTask.SendLocalizerStatus(PlcSignalForLocalizer.GLASS | PlcSignalForLocalizer.VISION_START, false, 15, 200);
            Invoke(new MethodInvoker(() => DisplayGlassVisionStatus(VisionStatus.NG)));

            if (e.PointNum == _refGlassPoses.Values.Count)
            {
                _isVehicleCheckReady = await _fineLocalizerVehicle.ReadyVehicleCheckerAsync();
            }

            Logger.CaptureFineLocalizer(VisionStatus.NG, "glass", _config[-1].CarName, _currentCarSeq.ToString());
        }

        private async void PlcForThisTask_BodyVisionStart(object sender, LocalizerEventArgs e)
        {
            if (_mode == OperationMode.Set) return;

            Logger.Info($"{Lang.LogsFineLo.SignalBodyVisionStartReceived} (point= {e.PointNum})");

            if (_refPoses.Values == null)
            {
                Logger.Error(Lang.LogsFineLo.RefPoseDataNotFound);
                return;
            }

            if (e.PointNum > _refPoses.Values.Count)
            {
                Logger.Warning(Lang.LogsFineLo.InvalidPointNumberInput);
                return;
            }

            if (!await WaitForCameraConnection(250, 20))
            {
                Logger.Error(Lang.LogsFineLo.StopForCameraConnectionFailed);
                return;
            }

            Invoke(new MethodInvoker(() => tabControl1_.SelectedTab = tabControl1_.TabPages["tabPageVehicle_"]));

            if (_plcForThisTask.BodyVisionPass)
            {
                await DoBodyVisionPass(e.PointNum);
                return;
            }

            if (_isVehicleCheckReady && await ScanPoint(e.PointNum))
            {
                int ret = await _plcForThisTask.SendLocalizerStatus(PlcSignalForLocalizer.BODY | ConvertPointNumberToPlcSignal(e.PointNum), true, 15, 200);
                if (ret < 0)
                {
                    Logger.Error(Lang.LogsFineLo.PointCompletedSignalSentFailed);
                }
                else
                {
                    if (e.PointNum != _refPoses.Values.Count)
                    {
                        return;
                    }

                    if (await CalculatePoses() && await UpdateInstallPosesToRobotAsync())
                    {
                        await _plcForThisTask.SendShiftValue(PlcSignalForLocalizer.BODY, _calPositions[0], 15, 200);
                        await _plcForThisTask.SendLocalizerStatus(PlcSignalForLocalizer.BODY | PlcSignalForLocalizer.VISION_OK, true, 15, 200);
                        await _plcForThisTask.SendLocalizerStatus(PlcSignalForLocalizer.BODY | PlcSignalForLocalizer.VISION_START, false, 15, 200);
                        Invoke(new MethodInvoker(() => DisplayVehicleVisionStatus(VisionStatus.OK)));

                        Logger.CaptureFineLocalizer(VisionStatus.OK, "vehicle", _config[-1].CarName, _currentCarSeq.ToString());

                        if (_config[-1].UseGapChecker)
                        {
                            if ((_refGapPoses.Values?.Count ?? 0) < 1)
                            {
                                Logger.Warning(Lang.LogsFineLo.ReadingGapCheckRefPosesFailed);
                            }
                            else
                            {
                                (bool isSuccess, RobotPose shiftValue) = await _gapChecker.EstimateAndUpdateGapScanPosesAsync();
                                if (!isSuccess)
                                {
                                    Logger.Warning(Lang.LogsFineLo.GapCheckScanPoseCalculationFailed);
                                }
                                else
                                {
                                    await _plcForThisTask.SendLocalizerStatus(PlcSignalForLocalizer.GAP | PlcSignalForLocalizer.SCAN_POSE_SENT, true, 15, 200);
                                }

                                UpdateRobotPoseToUI(shiftValue, gbGapSV_);
                                _isGapCheckReady = FineLocalizerVehicleEngineAPI.ReadyGapCheck();
                            }
                        }

                        return;
                    }
                }
            }

            await _plcForThisTask.SendLocalizerStatus(PlcSignalForLocalizer.BODY | PlcSignalForLocalizer.VISION_NG, true, 15, 200);
            await _plcForThisTask.SendLocalizerStatus(PlcSignalForLocalizer.BODY | PlcSignalForLocalizer.VISION_START, false, 15, 200);
            Invoke(new MethodInvoker(() => DisplayVehicleVisionStatus(VisionStatus.NG)));

            if (e.PointNum == _refPoses.Values.Count && _config[-1].UseGapChecker)
            {
                _isGapCheckReady = FineLocalizerVehicleEngineAPI.ReadyGapCheck();
            }

            Logger.CaptureFineLocalizer(VisionStatus.NG, "vehicle", _config[-1].CarName, _currentCarSeq.ToString());
        }

        private async void PlcForThisTask_GapVisionStart(object sender, LocalizerEventArgs e)
        {
            if (_mode == OperationMode.Set) return;

            Logger.Info($"{Lang.LogsFineLo.SignalGapVisionStartReceived} (point= {e.PointNum})");

            if (_refGapPoses.Values == null)
            {
                Logger.Error(Lang.LogsFineLo.RefPoseDataNotFound);
                return;
            }

            if (e.PointNum > _refGapPoses.Values.Count)
            {
                Logger.Warning(Lang.LogsFineLo.InvalidPointNumberInput);
                return;
            }

            if (!await WaitForCameraConnection(250, 20))
            {
                Logger.Error(Lang.LogsFineLo.StopForCameraConnectionFailed);
                return;
            }

            Invoke(new MethodInvoker(() => tabControl1_.SelectedTab = tabControl1_.TabPages["tabPageGap_"]));

            if (_plcForThisTask.GapVisionPass)
            {
                await DoGapVisionPass(e.PointNum);
                return;
            }

            if (_isGapCheckReady)
            {
                await ScanGapPoint(e.PointNum);
                int retPoint = await _plcForThisTask.SendLocalizerStatus(PlcSignalForLocalizer.GAP | ConvertPointNumberToPlcSignal(e.PointNum), true, 15, 200);
                if (retPoint < 0)
                {
                    Logger.Error(Lang.LogsFineLo.PointCompletedSignalSentFailed);
                }
                else
                {
                    (bool isSuccess, double[] gapValues) = await CalculateGapAsync();
                    if (isSuccess)
                    {
                        await _plcForThisTask.SendGapValue(gapValues.Select(g => (float)g).ToArray(), 15, 200);
                        await _plcForThisTask.SendLocalizerStatus(PlcSignalForLocalizer.GAP | PlcSignalForLocalizer.VISION_OK, true, 15, 200);
                        await _plcForThisTask.SendLocalizerStatus(PlcSignalForLocalizer.GAP | PlcSignalForLocalizer.VISION_START, false, 15, 200);
                        Invoke(new MethodInvoker(() => DisplayGapVisionStatus(VisionStatus.OK)));
                        Logger.CaptureFineLocalizer(VisionStatus.OK, "gap", _config[-1].CarName, _currentCarSeq.ToString());
                        return;
                    }
                }
            }

            await _plcForThisTask.SendLocalizerStatus(PlcSignalForLocalizer.GAP | PlcSignalForLocalizer.VISION_NG, true, 15, 200);
            await _plcForThisTask.SendLocalizerStatus(PlcSignalForLocalizer.GAP | PlcSignalForLocalizer.VISION_START, false, 15, 200);
            Invoke(new MethodInvoker(() => DisplayGapVisionStatus(VisionStatus.NG)));
            Logger.CaptureFineLocalizer(VisionStatus.NG, "gap", _config[-1].CarName, _currentCarSeq.ToString());
        }
    }
}
