using CommonUtils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace FineLocalizer
{
    public class MelsecComm : MelsecPLCCommunicator<PlcSignalForLocalizer>, IPlcForLocalizer
    {
        private static readonly LogHelper Logger = LogHelper.Logger;

        private bool _visionUpdateSignal;
        private bool _visionEndSignal;
        private bool _glassVisionStartSignal;
        private bool _bodyVisionStartSignal;
        private bool _gapVisionStartSignal;

        private bool _glassVisionResetSignal;
        private bool _bodyVisionResetSignal;
        private bool _gapVisionResetSignal;

        private int _glassPointNum;
        private int _bodyPointNum;
        private int _gapPointNum;

        public event EventHandler<LocalizerEventArgs> GlassVisionStart;
        public event EventHandler<LocalizerEventArgs> GlassVisionReset;
        public event EventHandler<CarTypeUpdateEventArgs> CarTypeUpdate;
        //public event EventHandler<OnOffEventArgs> GlassVisionPass;

        public event EventHandler<LocalizerEventArgs> BodyVisionStart;
        public event EventHandler<LocalizerEventArgs> BodyVisionReset;
        //public event EventHandler<OnOffEventArgs> BodyVisionPass;

        public event EventHandler<LocalizerEventArgs> GapVisionStart;
        public event EventHandler<OnOffEventArgs> VisionEnd;
        public event EventHandler<LocalizerEventArgs> GapVisionReset;
        //public event EventHandler<OnOffEventArgs> GapVisionPass;

        public bool GlassVisionPass { get; set; }
        public bool BodyVisionPass { get; set; }
        public bool GapVisionPass { get; set; }

        private PlcSignalForLocalizer[] _glassShiftValueSigs = new PlcSignalForLocalizer[]
        {
            PlcSignalForLocalizer.GLASS | PlcSignalForLocalizer.SHIFT_X | PlcSignalForLocalizer.VALUE,
            PlcSignalForLocalizer.GLASS | PlcSignalForLocalizer.SHIFT_Y | PlcSignalForLocalizer.VALUE,
            PlcSignalForLocalizer.GLASS | PlcSignalForLocalizer.SHIFT_Z | PlcSignalForLocalizer.VALUE,
            PlcSignalForLocalizer.GLASS | PlcSignalForLocalizer.SHIFT_RX | PlcSignalForLocalizer.VALUE,
            PlcSignalForLocalizer.GLASS | PlcSignalForLocalizer.SHIFT_RY | PlcSignalForLocalizer.VALUE,
            PlcSignalForLocalizer.GLASS | PlcSignalForLocalizer.SHIFT_RZ | PlcSignalForLocalizer.VALUE
        };

        private PlcSignalForLocalizer[] _bodyShiftValueSigs = new PlcSignalForLocalizer[]
        {
            PlcSignalForLocalizer.BODY | PlcSignalForLocalizer.SHIFT_X | PlcSignalForLocalizer.VALUE,
            PlcSignalForLocalizer.BODY | PlcSignalForLocalizer.SHIFT_Y | PlcSignalForLocalizer.VALUE,
            PlcSignalForLocalizer.BODY | PlcSignalForLocalizer.SHIFT_Z | PlcSignalForLocalizer.VALUE,
            PlcSignalForLocalizer.BODY | PlcSignalForLocalizer.SHIFT_RX | PlcSignalForLocalizer.VALUE,
            PlcSignalForLocalizer.BODY | PlcSignalForLocalizer.SHIFT_RY | PlcSignalForLocalizer.VALUE,
            PlcSignalForLocalizer.BODY | PlcSignalForLocalizer.SHIFT_RZ | PlcSignalForLocalizer.VALUE
        };

        private PlcSignalForLocalizer[] _gapValueSigs = new PlcSignalForLocalizer[]
        {
            PlcSignalForLocalizer.GAP_1 | PlcSignalForLocalizer.VALUE,
            PlcSignalForLocalizer.GAP_2 | PlcSignalForLocalizer.VALUE,
            PlcSignalForLocalizer.GAP_3 | PlcSignalForLocalizer.VALUE,
            PlcSignalForLocalizer.GAP_4 | PlcSignalForLocalizer.VALUE
        };

        public MelsecComm(Dictionary<PlcAttribute, string> config,
                          string heartBeatDeivceName, PlcDataType heartBeatDeviceType,
                          int heartBeatPos, int heartBeatBit)
            : base(config, heartBeatDeivceName, heartBeatDeviceType, heartBeatPos, heartBeatBit)
        {
            _sendVisionStatus = SendLocalizerStatus;
            _raiseEventIfItNeeds = RaiseEventIfItNeeds;
            LoadMonitorDeviceInfosForLocalizer();
        }

        private void LoadMonitorDeviceInfosForLocalizer()
        {
            MonitorDeviceInfoList = new List<MelsecMonitorDeviceInfo<PlcSignalForLocalizer>>()
            {
                //[0]
                new MelsecMonitorDeviceInfo<PlcSignalForLocalizer>("D", "1101", 2, PlcDataType.WORD, PlcDataType.WORD)
                {
                    SignalDict = new ConcurrentDictionary<PlcSignalForLocalizer, PlcDbInfo>()
                    {
                        [PlcSignalForLocalizer.CAR_TYPE | PlcSignalForLocalizer.VALUE] = new PlcDbInfo(1101, 0, PlcDataType.WORD),
                        [PlcSignalForLocalizer.CAR_SEQ | PlcSignalForLocalizer.VALUE] = new PlcDbInfo(1102, 0, PlcDataType.WORD)
                    }
                },

                //[1]
                new MelsecMonitorDeviceInfo<PlcSignalForLocalizer>("D", "1100", 1, PlcDataType.WORD, PlcDataType.BIT)
                {
                    SignalDict = new ConcurrentDictionary<PlcSignalForLocalizer, PlcDbInfo>()
                    {
                        [PlcSignalForLocalizer.GLASS | PlcSignalForLocalizer.VISION_START] = new PlcDbInfo(1100, 0, PlcDataType.BIT),
                        [PlcSignalForLocalizer.GLASS | PlcSignalForLocalizer.VISION_RESET] = new PlcDbInfo(1100, 2, PlcDataType.BIT),
                        [PlcSignalForLocalizer.VISION_UPDATE] = new PlcDbInfo(1100, 3, PlcDataType.BIT),
                        [PlcSignalForLocalizer.GLASS | PlcSignalForLocalizer.P1] = new PlcDbInfo(1100, 10, PlcDataType.BIT),
                        [PlcSignalForLocalizer.GLASS | PlcSignalForLocalizer.P2] = new PlcDbInfo(1100, 11, PlcDataType.BIT),
                        [PlcSignalForLocalizer.GLASS | PlcSignalForLocalizer.P3] = new PlcDbInfo(1100, 12, PlcDataType.BIT),
                        [PlcSignalForLocalizer.GLASS | PlcSignalForLocalizer.P4] = new PlcDbInfo(1100, 13, PlcDataType.BIT),
                        [PlcSignalForLocalizer.GLASS | PlcSignalForLocalizer.VISION_PASS] = new PlcDbInfo(1100, 15, PlcDataType.BIT)
                    }
                },

                //[2]
                new MelsecMonitorDeviceInfo<PlcSignalForLocalizer>("D", "1200", 1, PlcDataType.WORD, PlcDataType.BIT)
                {
                    SignalDict = new ConcurrentDictionary<PlcSignalForLocalizer, PlcDbInfo>()
                    {
                        [PlcSignalForLocalizer.GLASS | PlcSignalForLocalizer.VISION_OK] = new PlcDbInfo(1200, 0, PlcDataType.BIT),
                        [PlcSignalForLocalizer.GLASS | PlcSignalForLocalizer.VISION_NG] = new PlcDbInfo(1200, 1, PlcDataType.BIT),
                        [PlcSignalForLocalizer.GLASS | PlcSignalForLocalizer.P1_COMPLETED] = new PlcDbInfo(1200, 10, PlcDataType.BIT),
                        [PlcSignalForLocalizer.GLASS | PlcSignalForLocalizer.P2_COMPLETED] = new PlcDbInfo(1200, 11, PlcDataType.BIT),
                        [PlcSignalForLocalizer.GLASS | PlcSignalForLocalizer.P3_COMPLETED] = new PlcDbInfo(1200, 12, PlcDataType.BIT),
                        [PlcSignalForLocalizer.GLASS | PlcSignalForLocalizer.P4_COMPLETED] = new PlcDbInfo(1200, 13, PlcDataType.BIT),
                        [PlcSignalForLocalizer.GLASS | PlcSignalForLocalizer.PLC_PASS] = new PlcDbInfo(1200, 15, PlcDataType.BIT)
                    }
                },

                //[3]
                new MelsecMonitorDeviceInfo<PlcSignalForLocalizer>("D", "1201", 6, PlcDataType.WORD, PlcDataType.FLOAT)
                {
                    SignalDict = new ConcurrentDictionary<PlcSignalForLocalizer, PlcDbInfo>()
                    {
                        [PlcSignalForLocalizer.GLASS | PlcSignalForLocalizer.SHIFT_X | PlcSignalForLocalizer.VALUE] = new PlcDbInfo(1201, 0, PlcDataType.FLOAT),
                        [PlcSignalForLocalizer.GLASS | PlcSignalForLocalizer.SHIFT_Y | PlcSignalForLocalizer.VALUE] = new PlcDbInfo(1203, 0, PlcDataType.FLOAT),
                        [PlcSignalForLocalizer.GLASS | PlcSignalForLocalizer.SHIFT_Z | PlcSignalForLocalizer.VALUE] = new PlcDbInfo(1205, 0, PlcDataType.FLOAT),
                        [PlcSignalForLocalizer.GLASS | PlcSignalForLocalizer.SHIFT_RX | PlcSignalForLocalizer.VALUE] = new PlcDbInfo(1207, 0, PlcDataType.FLOAT),
                        [PlcSignalForLocalizer.GLASS | PlcSignalForLocalizer.SHIFT_RY | PlcSignalForLocalizer.VALUE] = new PlcDbInfo(1209, 0, PlcDataType.FLOAT),
                        [PlcSignalForLocalizer.GLASS | PlcSignalForLocalizer.SHIFT_RZ | PlcSignalForLocalizer.VALUE] = new PlcDbInfo(1211, 0, PlcDataType.FLOAT)
                    }
                },

                //[4]
                new MelsecMonitorDeviceInfo<PlcSignalForLocalizer>("D", "1300", 1, PlcDataType.WORD, PlcDataType.BIT)
                {
                    SignalDict = new ConcurrentDictionary<PlcSignalForLocalizer, PlcDbInfo>()
                    {
                        [PlcSignalForLocalizer.BODY | PlcSignalForLocalizer.VISION_START] = new PlcDbInfo(1300, 0, PlcDataType.BIT),
                        [PlcSignalForLocalizer.BODY | PlcSignalForLocalizer.VISION_RESET] = new PlcDbInfo(1300, 2, PlcDataType.BIT),
                        [PlcSignalForLocalizer.BODY | PlcSignalForLocalizer.P1] = new PlcDbInfo(1300, 10, PlcDataType.BIT),
                        [PlcSignalForLocalizer.BODY | PlcSignalForLocalizer.P2] = new PlcDbInfo(1300, 11, PlcDataType.BIT),
                        [PlcSignalForLocalizer.BODY | PlcSignalForLocalizer.P3] = new PlcDbInfo(1300, 12, PlcDataType.BIT),
                        [PlcSignalForLocalizer.BODY | PlcSignalForLocalizer.P4] = new PlcDbInfo(1300, 13, PlcDataType.BIT),
                        [PlcSignalForLocalizer.BODY | PlcSignalForLocalizer.VISION_PASS] = new PlcDbInfo(1300, 15, PlcDataType.BIT)
                    }
                },

                //[5]
                new MelsecMonitorDeviceInfo<PlcSignalForLocalizer>("D", "1400", 1, PlcDataType.WORD, PlcDataType.BIT)
                {
                    SignalDict = new ConcurrentDictionary<PlcSignalForLocalizer, PlcDbInfo>()
                    {
                        [PlcSignalForLocalizer.BODY | PlcSignalForLocalizer.VISION_OK] = new PlcDbInfo(1400, 0, PlcDataType.BIT),
                        [PlcSignalForLocalizer.BODY | PlcSignalForLocalizer.VISION_NG] = new PlcDbInfo(1400, 1, PlcDataType.BIT),
                        [PlcSignalForLocalizer.BODY | PlcSignalForLocalizer.P1_COMPLETED] = new PlcDbInfo(1400, 10, PlcDataType.BIT),
                        [PlcSignalForLocalizer.BODY | PlcSignalForLocalizer.P2_COMPLETED] = new PlcDbInfo(1400, 11, PlcDataType.BIT),
                        [PlcSignalForLocalizer.BODY | PlcSignalForLocalizer.P3_COMPLETED] = new PlcDbInfo(1400, 12, PlcDataType.BIT),
                        [PlcSignalForLocalizer.BODY | PlcSignalForLocalizer.P4_COMPLETED] = new PlcDbInfo(1400, 13, PlcDataType.BIT),
                        [PlcSignalForLocalizer.BODY | PlcSignalForLocalizer.PLC_PASS] = new PlcDbInfo(1400, 15, PlcDataType.BIT)
                    }
                },

                //[6]
                new MelsecMonitorDeviceInfo<PlcSignalForLocalizer>("D", "1401", 12, PlcDataType.WORD, PlcDataType.FLOAT)
                {
                    SignalDict = new ConcurrentDictionary<PlcSignalForLocalizer, PlcDbInfo>()
                    {
                        [PlcSignalForLocalizer.BODY | PlcSignalForLocalizer.SHIFT_X | PlcSignalForLocalizer.VALUE] = new PlcDbInfo(1401, 0, PlcDataType.FLOAT),
                        [PlcSignalForLocalizer.BODY | PlcSignalForLocalizer.SHIFT_Y | PlcSignalForLocalizer.VALUE] = new PlcDbInfo(1403, 0, PlcDataType.FLOAT),
                        [PlcSignalForLocalizer.BODY | PlcSignalForLocalizer.SHIFT_Z | PlcSignalForLocalizer.VALUE] = new PlcDbInfo(1405, 0, PlcDataType.FLOAT),
                        [PlcSignalForLocalizer.BODY | PlcSignalForLocalizer.SHIFT_RX | PlcSignalForLocalizer.VALUE] = new PlcDbInfo(1407, 0, PlcDataType.FLOAT),
                        [PlcSignalForLocalizer.BODY | PlcSignalForLocalizer.SHIFT_RY | PlcSignalForLocalizer.VALUE] = new PlcDbInfo(1409, 0, PlcDataType.FLOAT),
                        [PlcSignalForLocalizer.BODY | PlcSignalForLocalizer.SHIFT_RZ | PlcSignalForLocalizer.VALUE] = new PlcDbInfo(1411, 0, PlcDataType.FLOAT)
                    }
                },

                //[7]
                new MelsecMonitorDeviceInfo<PlcSignalForLocalizer>("D", "1500", 1, PlcDataType.WORD, PlcDataType.BIT)
                {
                    SignalDict = new ConcurrentDictionary<PlcSignalForLocalizer, PlcDbInfo>()
                    {
                        [PlcSignalForLocalizer.GAP | PlcSignalForLocalizer.VISION_START] = new PlcDbInfo(1500, 0, PlcDataType.BIT),
                        [PlcSignalForLocalizer.VISION_END] = new PlcDbInfo(1500, 1, PlcDataType.BIT),
                        [PlcSignalForLocalizer.GAP | PlcSignalForLocalizer.VISION_RESET] = new PlcDbInfo(1500, 2, PlcDataType.BIT),
                        [PlcSignalForLocalizer.GAP | PlcSignalForLocalizer.P1] = new PlcDbInfo(1500, 10, PlcDataType.BIT),
                        [PlcSignalForLocalizer.GAP | PlcSignalForLocalizer.P2] = new PlcDbInfo(1500, 11, PlcDataType.BIT),
                        [PlcSignalForLocalizer.GAP | PlcSignalForLocalizer.P3] = new PlcDbInfo(1500, 12, PlcDataType.BIT),
                        [PlcSignalForLocalizer.GAP | PlcSignalForLocalizer.P4] = new PlcDbInfo(1500, 13, PlcDataType.BIT),
                        [PlcSignalForLocalizer.GAP | PlcSignalForLocalizer.VISION_PASS] = new PlcDbInfo(1500, 15, PlcDataType.BIT)
                    }
                },

                //[8]
                new MelsecMonitorDeviceInfo<PlcSignalForLocalizer>("D", "1600", 1, PlcDataType.WORD, PlcDataType.BIT)
                {
                    SignalDict = new ConcurrentDictionary<PlcSignalForLocalizer, PlcDbInfo>()
                    {
                        [PlcSignalForLocalizer.GAP | PlcSignalForLocalizer.VISION_OK] = new PlcDbInfo(1600, 0, PlcDataType.BIT),
                        [PlcSignalForLocalizer.GAP | PlcSignalForLocalizer.VISION_NG] = new PlcDbInfo(1600, 1, PlcDataType.BIT),
                        [PlcSignalForLocalizer.GAP | PlcSignalForLocalizer.SCAN_POSE_SENT] = new PlcDbInfo(1600, 2, PlcDataType.BIT),
                        [PlcSignalForLocalizer.GAP | PlcSignalForLocalizer.P1_COMPLETED] = new PlcDbInfo(1600, 10, PlcDataType.BIT),
                        [PlcSignalForLocalizer.GAP | PlcSignalForLocalizer.P2_COMPLETED] = new PlcDbInfo(1600, 11, PlcDataType.BIT),
                        [PlcSignalForLocalizer.GAP | PlcSignalForLocalizer.P3_COMPLETED] = new PlcDbInfo(1600, 12, PlcDataType.BIT),
                        [PlcSignalForLocalizer.GAP | PlcSignalForLocalizer.P4_COMPLETED] = new PlcDbInfo(1600, 13, PlcDataType.BIT),
                        [PlcSignalForLocalizer.GAP | PlcSignalForLocalizer.PLC_PASS] = new PlcDbInfo(1600, 15, PlcDataType.BIT)
                    }
                },

                //[9]
                new MelsecMonitorDeviceInfo<PlcSignalForLocalizer>("D", "1601", 8, PlcDataType.WORD, PlcDataType.FLOAT)
                {
                    SignalDict = new ConcurrentDictionary<PlcSignalForLocalizer, PlcDbInfo>()
                    {
                        [PlcSignalForLocalizer.GAP_1 | PlcSignalForLocalizer.VALUE] = new PlcDbInfo(1601, 0, PlcDataType.FLOAT),
                        [PlcSignalForLocalizer.GAP_2 | PlcSignalForLocalizer.VALUE] = new PlcDbInfo(1603, 0, PlcDataType.FLOAT),
                        [PlcSignalForLocalizer.GAP_3 | PlcSignalForLocalizer.VALUE] = new PlcDbInfo(1605, 0, PlcDataType.FLOAT),
                        [PlcSignalForLocalizer.GAP_4 | PlcSignalForLocalizer.VALUE] = new PlcDbInfo(1607, 0, PlcDataType.FLOAT),
                    }
                }
            };
        }

        private int ExtractGlassPointNumber()
        {
            int pointNum = 0;
            if (MonitorDeviceInfoList[1].SignalDict[PlcSignalForLocalizer.GLASS | PlcSignalForLocalizer.P4].IsOn)
            {
                pointNum = 4;
            }
            else if (MonitorDeviceInfoList[1].SignalDict[PlcSignalForLocalizer.GLASS | PlcSignalForLocalizer.P3].IsOn)
            {
                pointNum = 3;
            }
            else if (MonitorDeviceInfoList[1].SignalDict[PlcSignalForLocalizer.GLASS | PlcSignalForLocalizer.P2].IsOn)
            {
                pointNum = 2;
            }
            else if (MonitorDeviceInfoList[1].SignalDict[PlcSignalForLocalizer.GLASS | PlcSignalForLocalizer.P1].IsOn)
            {
                pointNum = 1;
            }

            return pointNum;
        }

        private int ExtractBodyPointNumber()
        {
            int pointNum = 0;
            if (MonitorDeviceInfoList[4].SignalDict[PlcSignalForLocalizer.BODY | PlcSignalForLocalizer.P4].IsOn)
            {
                pointNum = 4;
            }
            else if (MonitorDeviceInfoList[4].SignalDict[PlcSignalForLocalizer.BODY | PlcSignalForLocalizer.P3].IsOn)
            {
                pointNum = 3;
            }
            else if (MonitorDeviceInfoList[4].SignalDict[PlcSignalForLocalizer.BODY | PlcSignalForLocalizer.P2].IsOn)
            {
                pointNum = 2;
            }
            else if (MonitorDeviceInfoList[4].SignalDict[PlcSignalForLocalizer.BODY | PlcSignalForLocalizer.P1].IsOn)
            {
                pointNum = 1;
            }

            return pointNum;
        }

        private int ExtractGapPointNumber()
        {
            int pointNum = 0;
            if (MonitorDeviceInfoList[7].SignalDict[PlcSignalForLocalizer.GAP | PlcSignalForLocalizer.P4].IsOn)
            {
                pointNum = 4;
            }
            else if (MonitorDeviceInfoList[7].SignalDict[PlcSignalForLocalizer.GAP | PlcSignalForLocalizer.P3].IsOn)
            {
                pointNum = 3;
            }
            else if (MonitorDeviceInfoList[7].SignalDict[PlcSignalForLocalizer.GAP | PlcSignalForLocalizer.P2].IsOn)
            {
                pointNum = 2;
            }
            else if (MonitorDeviceInfoList[7].SignalDict[PlcSignalForLocalizer.GAP | PlcSignalForLocalizer.P1].IsOn)
            {
                pointNum = 1;
            }

            return pointNum;
        }

        private bool UpdateCarTypeAndSeq()
        {
            int oldCarType = CarType;
            int oldCarSeq = CarSeq;

            CarType = MonitorDeviceInfoList[0].SignalDict[PlcSignalForLocalizer.CAR_TYPE | PlcSignalForLocalizer.VALUE].IntValue;
            CarSeq = MonitorDeviceInfoList[0].SignalDict[PlcSignalForLocalizer.CAR_SEQ | PlcSignalForLocalizer.VALUE].IntValue;

            return oldCarSeq != CarSeq || oldCarType != CarType;
        }

        private void RaiseCarTypeUpdateEvent()
        {
            if (!_visionUpdateSignal)
            {
                UpdateCarTypeAndSeq();
                CarTypeUpdate?.Invoke(this, new CarTypeUpdateEventArgs(CarType, CarSeq));
            }
        }

        private void RaiseGlassVisionStartEvent()
        {
            int currentGlassPointNumber = ExtractGlassPointNumber();
            if (currentGlassPointNumber != 0 && (!_glassVisionStartSignal || _glassPointNum != currentGlassPointNumber))
            {
                _glassPointNum = currentGlassPointNumber;
                GlassVisionStart?.Invoke(this, new LocalizerEventArgs(_glassPointNum, true));
            }
        }

        private void RaiseBodyVisionStartEvent()
        {
            int currentBodyPointNumber = ExtractBodyPointNumber();
            if (currentBodyPointNumber != 0 && (!_bodyVisionStartSignal || _bodyPointNum != currentBodyPointNumber))
            {
                _bodyPointNum = currentBodyPointNumber;
                BodyVisionStart?.Invoke(this, new LocalizerEventArgs(_bodyPointNum, true));
            }
        }

        private void RaiseGapVisionStartEvent()
        {
            int currentGapPointNumber = ExtractGapPointNumber();
            if (currentGapPointNumber != 0 && (!_gapVisionStartSignal || _gapPointNum != currentGapPointNumber))
            {
                _gapPointNum = currentGapPointNumber;
                GapVisionStart?.Invoke(this, new LocalizerEventArgs(_gapPointNum, true));
            }
        }

        private void RaiseGlassVisionResetEvent()
        {
            if (!_glassVisionResetSignal)
            {
                GlassVisionReset?.Invoke(this, new LocalizerEventArgs(ExtractGlassPointNumber(), true));
            }
        }

        private void RaiseBodyVisionResetEvent()
        {
            if (!_bodyVisionResetSignal)
            {
                BodyVisionReset?.Invoke(this, new LocalizerEventArgs(ExtractBodyPointNumber(), true));
            }
        }

        private void RaiseGapVisionResetEvent()
        {
            if (!_gapVisionResetSignal)
            {
                GapVisionReset?.Invoke(this, new LocalizerEventArgs(ExtractGapPointNumber(), true));
            }
        }

        private void RaiseVisionEndEvent()
        {
            if (!_visionEndSignal)
            {
                VisionEnd?.Invoke(this, new OnOffEventArgs(true));
            }
        }

        private void RaiseEventIfItNeeds()
        {
            if (MonitorDeviceInfoList[1].SignalDict[PlcSignalForLocalizer.VISION_UPDATE].IsOn)
            {
                RaiseCarTypeUpdateEvent();
            }
            _visionUpdateSignal = MonitorDeviceInfoList[1].SignalDict[PlcSignalForLocalizer.VISION_UPDATE].IsOn;

            if (MonitorDeviceInfoList[1].SignalDict[PlcSignalForLocalizer.GLASS | PlcSignalForLocalizer.VISION_START].IsOn)
            {
                RaiseGlassVisionStartEvent();
            }
            else if (MonitorDeviceInfoList[4].SignalDict[PlcSignalForLocalizer.BODY | PlcSignalForLocalizer.VISION_START].IsOn)
            {
                RaiseBodyVisionStartEvent();
            }
            else if (MonitorDeviceInfoList[7].SignalDict[PlcSignalForLocalizer.GAP | PlcSignalForLocalizer.VISION_START].IsOn)
            {
                RaiseGapVisionStartEvent();
            }

            _glassVisionStartSignal = MonitorDeviceInfoList[1].SignalDict[PlcSignalForLocalizer.GLASS | PlcSignalForLocalizer.VISION_START].IsOn;
            _bodyVisionStartSignal = MonitorDeviceInfoList[4].SignalDict[PlcSignalForLocalizer.BODY | PlcSignalForLocalizer.VISION_START].IsOn;
            _gapVisionStartSignal = MonitorDeviceInfoList[7].SignalDict[PlcSignalForLocalizer.GAP | PlcSignalForLocalizer.VISION_START].IsOn;

            _glassPointNum = ExtractGlassPointNumber();
            _bodyPointNum = ExtractBodyPointNumber();
            _gapPointNum = ExtractGapPointNumber();

            if (MonitorDeviceInfoList[1].SignalDict[PlcSignalForLocalizer.GLASS | PlcSignalForLocalizer.VISION_RESET].IsOn)
            {
                RaiseGlassVisionResetEvent();
            }
            else if (MonitorDeviceInfoList[4].SignalDict[PlcSignalForLocalizer.BODY | PlcSignalForLocalizer.VISION_RESET].IsOn)
            {
                RaiseBodyVisionResetEvent();
            }
            else if (MonitorDeviceInfoList[7].SignalDict[PlcSignalForLocalizer.GAP | PlcSignalForLocalizer.VISION_RESET].IsOn)
            {
                RaiseGapVisionResetEvent();
            }

            _glassVisionResetSignal = MonitorDeviceInfoList[1].SignalDict[PlcSignalForLocalizer.GLASS | PlcSignalForLocalizer.VISION_RESET].IsOn;
            _bodyVisionResetSignal = MonitorDeviceInfoList[4].SignalDict[PlcSignalForLocalizer.BODY | PlcSignalForLocalizer.VISION_RESET].IsOn;
            _gapVisionResetSignal = MonitorDeviceInfoList[7].SignalDict[PlcSignalForLocalizer.GAP | PlcSignalForLocalizer.VISION_RESET].IsOn;

            GlassVisionPass = MonitorDeviceInfoList[1].SignalDict[PlcSignalForLocalizer.GLASS | PlcSignalForLocalizer.VISION_PASS].IsOn;
            BodyVisionPass = MonitorDeviceInfoList[4].SignalDict[PlcSignalForLocalizer.BODY | PlcSignalForLocalizer.VISION_PASS].IsOn;
            GapVisionPass = MonitorDeviceInfoList[7].SignalDict[PlcSignalForLocalizer.GAP | PlcSignalForLocalizer.VISION_PASS].IsOn;

            if (MonitorDeviceInfoList[7].SignalDict[PlcSignalForLocalizer.VISION_END].IsOn)
            {
                RaiseVisionEndEvent();
            }

            _visionEndSignal = MonitorDeviceInfoList[7].SignalDict[PlcSignalForLocalizer.VISION_END].IsOn;
        }

        public async Task<int> SendLocalizerStatus(PlcSignalForLocalizer status, bool val, int nMaxTrials = 1, int delay = 50)
        {
            int idxMonitor;
            PlcSignalForLocalizer sig;

            if (status.HasFlag(PlcSignalForLocalizer.GLASS) || status == PlcSignalForLocalizer.VISION_UPDATE)
            {
                sig = status & ~PlcSignalForLocalizer.GLASS;
                idxMonitor = MonitorDeviceInfoList[2].SignalDict.ContainsKey(status) ? 2 : 1;
            }
            else if (status.HasFlag(PlcSignalForLocalizer.BODY))
            {
                sig = status & ~PlcSignalForLocalizer.BODY;
                idxMonitor = MonitorDeviceInfoList[5].SignalDict.ContainsKey(status) ? 5 : 4;
            }
            else 
            {
                sig = status & ~PlcSignalForLocalizer.GAP;
                idxMonitor = MonitorDeviceInfoList[8].SignalDict.ContainsKey(status) ? 8 : 7;
            }

            _writeBuf[0] = (int)sig;

            int ret = 0;
            for (var i = 0; i < nMaxTrials; ++i)
            {
                ret = SetBit("D", MonitorDeviceInfoList[idxMonitor].DeviceType,
                             MonitorDeviceInfoList[idxMonitor].SignalDict[status], val);
                await Task.Delay(delay);

                if (MonitorDeviceInfoList[idxMonitor].SignalDict[status].IsOn == val)
                {
                    if (val)
                    {
                        Logger.Info($"{Lang.LogsFineLo.MelsecPlcSigOnDone} ({sig})");
                    }
                    else
                    {
                        Logger.Info($"{Lang.LogsFineLo.MelsecPlcSigOffDone} ({sig})");
                    }
                    return 0;
                }
            }

            if (val)
            {
                Logger.Warning($"{Lang.LogsFineLo.MelsecPlcSigOnFailed} ({sig} / {ret})");
            }
            else
            {
                Logger.Warning($"{Lang.LogsFineLo.MelsecPlcSigOffFailed} ({sig} / {ret})");
            }

            return ret;
        }

        public async Task<int> SendShiftValue(PlcSignalForLocalizer task, RobotPose shiftValue, int nMaxTrials = 1, int delay = 50)
        {
            _writeFloatBuf = new float[] { (float)shiftValue.Tx,
                                           (float)shiftValue.Ty,
                                           (float)shiftValue.Tz,
                                           (float)shiftValue.Rx,
                                           (float)shiftValue.Ry,
                                           (float)shiftValue.Rz };

            string startPos;
            PlcSignalForLocalizer[] svSigs;
            ConcurrentDictionary<PlcSignalForLocalizer, PlcDbInfo> sigDict;

            if (task == PlcSignalForLocalizer.GLASS)
            {
                sigDict = MonitorDeviceInfoList[3].SignalDict;
                startPos = "1201";
                svSigs = _glassShiftValueSigs;
            }
            else
            {
                sigDict = MonitorDeviceInfoList[6].SignalDict;
                startPos = "1401";
                svSigs = _bodyShiftValueSigs;
            }

            int ret = 0;
            for (var i = 0; i < nMaxTrials; ++i)
            {
                ret = SendToPlcFloat("D", startPos);
                await Task.Delay(delay);
                if (Enumerable.Range(0, 6).All(n => sigDict[svSigs[n]].FloatValue - _writeFloatBuf[n] < 0.01))
                {
                    Logger.Info($"{Lang.LogsFineLo.MelsecPlcWritingShiftValueDone} ({task})");
                    return 0;
                }
            }

            Logger.Warning($"{Lang.LogsFineLo.MelsecPlcWritingShiftValueFailed} ({task} / {ret})");
            return ret;
        }

        public async Task<int> SendGapValue(float[] gapValues, int nMaxTrials = 1, int delay = 50)
        {
            _writeFloatBuf = gapValues;
            var sigDict = MonitorDeviceInfoList[9].SignalDict;

            int ret = 0;
            for (var i = 0; i < nMaxTrials; ++i)
            {
                ret = SendToPlcFloat("D", "1601");
                await Task.Delay(delay);
                if (Enumerable.Range(0, 4).All(n => sigDict[_gapValueSigs[n]].FloatValue - _writeFloatBuf[n] < 0.01))
                {
                    Logger.Info(Lang.LogsFineLo.MelsecPlcWritingGapValueDone);
                    return 0;
                }
            }

            Logger.Warning($"{Lang.LogsFineLo.MelsecPlcWritingGapValueFailed} ({ret})");
            return ret;
        }
    }
}
