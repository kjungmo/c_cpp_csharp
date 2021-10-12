using CommonUtils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FineLocalizer
{
    public class S7Comm : S7PLCCommunicator<PlcSignalForLocalizer>, IPlcForLocalizer
    {
        private bool _localizerVisionEndSignal = false;
        private int _localizerPointNum;
        public event EventHandler<OnOffEventArgs> VisionEnd;
        public event EventHandler<CarTypeUpdateEventArgs> CarTypeUpdate;
        public event EventHandler<LocalizerEventArgs> GlassVisionStart;
        public event EventHandler<LocalizerEventArgs> BodyVisionStart;
        public event EventHandler<LocalizerEventArgs> GapVisionStart;
        public event EventHandler<LocalizerEventArgs> GlassVisionReset;
        public event EventHandler<LocalizerEventArgs> BodyVisionReset;
        public event EventHandler<LocalizerEventArgs> GapVisionReset;

        public bool GlassVisionPass { get; set; }
        public bool BodyVisionPass { get; set; }
        public bool GapVisionPass { get; set; }

        public S7Comm(Dictionary<PlcAttribute, string> config,
                      int heartBeatDbNum, int heartBeatPos, int readDbNum, int writeDbNum)
            : base(config, heartBeatDbNum, heartBeatPos)
        {
            _sendVisionStatus = SendLocalizerStatus;
            _raiseEventIfItNeeds = RaiseLocalizerEventIfItNeeds;
            LoadPlcSignalDictForLocalizer(readDbNum, writeDbNum);
        }

        private void LoadPlcSignalDictForLocalizer(int readDbNum, int writeDbNum)
        {
            var plcSignalDictR = new ConcurrentDictionary<PlcSignalForLocalizer, PlcDbInfo>()
            {
                [PlcSignalForLocalizer.VISION_START] = new PlcDbInfo(0, 0, PlcDataType.BIT),
                [PlcSignalForLocalizer.VISION_END] = new PlcDbInfo(0, 1, PlcDataType.BIT),
                [PlcSignalForLocalizer.BODY | PlcSignalForLocalizer.P1] = new PlcDbInfo(1, 0, PlcDataType.BIT),
                [PlcSignalForLocalizer.BODY | PlcSignalForLocalizer.P2] = new PlcDbInfo(1, 1, PlcDataType.BIT),
                [PlcSignalForLocalizer.BODY | PlcSignalForLocalizer.P3] = new PlcDbInfo(1, 2, PlcDataType.BIT),
                [PlcSignalForLocalizer.BODY | PlcSignalForLocalizer.P4] = new PlcDbInfo(1, 3, PlcDataType.BIT),
                [PlcSignalForLocalizer.CAR_TYPE_BIT0] = new PlcDbInfo(3, 0, PlcDataType.BIT),
                [PlcSignalForLocalizer.CAR_TYPE_BIT1] = new PlcDbInfo(3, 1, PlcDataType.BIT),
                [PlcSignalForLocalizer.CAR_TYPE_BIT2] = new PlcDbInfo(3, 2, PlcDataType.BIT),
                [PlcSignalForLocalizer.CAR_TYPE_BIT3] = new PlcDbInfo(3, 3, PlcDataType.BIT),
                [PlcSignalForLocalizer.CAR_TYPE_BIT4] = new PlcDbInfo(3, 4, PlcDataType.BIT),
                [PlcSignalForLocalizer.CAR_TYPE_BIT5] = new PlcDbInfo(3, 5, PlcDataType.BIT),
                [PlcSignalForLocalizer.CAR_TYPE_BIT6] = new PlcDbInfo(3, 6, PlcDataType.BIT),
                [PlcSignalForLocalizer.CAR_TYPE_BIT7] = new PlcDbInfo(3, 7, PlcDataType.BIT)
            };

            var plcSignalDictW = new ConcurrentDictionary<PlcSignalForLocalizer, PlcDbInfo>()
            { 
                [PlcSignalForLocalizer.BODY | PlcSignalForLocalizer.P1_COMPLETED] = new PlcDbInfo(1, 0, PlcDataType.BIT),
                [PlcSignalForLocalizer.BODY | PlcSignalForLocalizer.P2_COMPLETED] = new PlcDbInfo(1, 1, PlcDataType.BIT),
                [PlcSignalForLocalizer.BODY | PlcSignalForLocalizer.P3_COMPLETED] = new PlcDbInfo(1, 2, PlcDataType.BIT),
                [PlcSignalForLocalizer.BODY | PlcSignalForLocalizer.P4_COMPLETED] = new PlcDbInfo(1, 3, PlcDataType.BIT),
                [PlcSignalForLocalizer.VISION_OK] = new PlcDbInfo(1, 4, PlcDataType.BIT),
                [PlcSignalForLocalizer.VISION_NG] = new PlcDbInfo(1, 5, PlcDataType.BIT)
            };

            MonitorDbInfoList = new List<S7MonitorDbInfo<PlcSignalForLocalizer>>()
            {
                new S7MonitorDbInfo<PlcSignalForLocalizer>(readDbNum, 0, 4, plcSignalDictR),
                new S7MonitorDbInfo<PlcSignalForLocalizer>(writeDbNum, 0, 2, plcSignalDictW)
            };
        }

        private void RaiseLocalizerEventIfItNeeds()
        {
            int pointNum = 0;
            if (MonitorDbInfoList[0].SignalDict[PlcSignalForLocalizer.BODY | PlcSignalForLocalizer.P1].IsOn) pointNum = 1;
            else if (MonitorDbInfoList[0].SignalDict[PlcSignalForLocalizer.BODY | PlcSignalForLocalizer.P2].IsOn) pointNum = 2;
            else if (MonitorDbInfoList[0].SignalDict[PlcSignalForLocalizer.BODY | PlcSignalForLocalizer.P3].IsOn) pointNum = 3;
            else if (MonitorDbInfoList[0].SignalDict[PlcSignalForLocalizer.BODY | PlcSignalForLocalizer.P4].IsOn) pointNum = 4;

            if (MonitorDbInfoList[0].SignalDict[PlcSignalForLocalizer.VISION_START].IsOn &&
                pointNum != _localizerPointNum)
            {
                CheckCarType(MonitorDbInfoList[0].SignalDict[PlcSignalForLocalizer.CAR_TYPE_BIT0].IsOn,
                             MonitorDbInfoList[0].SignalDict[PlcSignalForLocalizer.CAR_TYPE_BIT1].IsOn,
                             MonitorDbInfoList[0].SignalDict[PlcSignalForLocalizer.CAR_TYPE_BIT2].IsOn,
                             MonitorDbInfoList[0].SignalDict[PlcSignalForLocalizer.CAR_TYPE_BIT3].IsOn,
                             MonitorDbInfoList[0].SignalDict[PlcSignalForLocalizer.CAR_TYPE_BIT4].IsOn,
                             MonitorDbInfoList[0].SignalDict[PlcSignalForLocalizer.CAR_TYPE_BIT5].IsOn,
                             MonitorDbInfoList[0].SignalDict[PlcSignalForLocalizer.CAR_TYPE_BIT6].IsOn,
                             MonitorDbInfoList[0].SignalDict[PlcSignalForLocalizer.CAR_TYPE_BIT7].IsOn);

                BodyVisionStart?.Invoke(this, new LocalizerEventArgs(pointNum, true));
                _localizerPointNum = pointNum;
            }

            if (MonitorDbInfoList[0].SignalDict[PlcSignalForLocalizer.VISION_END].IsOn != _localizerVisionEndSignal)
            {
                VisionEnd?.Invoke(this, new OnOffEventArgs(MonitorDbInfoList[0].SignalDict[PlcSignalForLocalizer.VISION_END].IsOn));
                _localizerVisionEndSignal = MonitorDbInfoList[0].SignalDict[PlcSignalForLocalizer.VISION_END].IsOn;
                _localizerPointNum = 0;
            }
        }

        public int SendLocalizerStatus(PlcSignalForLocalizer status)
        {
            return 1;
            //switch (status)
            //{
            //    case PlcSignalForLocalizer.P1_COMPLETED:
            //        _writeBuf[0] = 0b0000_0001;
            //        break;

            //    case PlcSignalForLocalizer.P2_COMPLETED:
            //        _writeBuf[0] = 0b0000_0011;
            //        break;

            //    case PlcSignalForLocalizer.P3_COMPLETED:
            //        _writeBuf[0] = 0b0000_0111;
            //        break;

            //    case PlcSignalForLocalizer.P4_COMPLETED:
            //        _writeBuf[0] = 0b0000_1111;
            //        break;

            //    case PlcSignalForLocalizer.POINTS_RESET:
            //        _writeBuf[0] = 0b0000_0000;
            //        break;
                
            //    case PlcSignalForLocalizer.OK:
            //        _writeBuf[0] = 0b0001_0000;
            //        break;

            //    case PlcSignalForLocalizer.NG:
            //        _writeBuf[0] = 0b0010_0000;
            //        break;
            //}

            //return SendToPlc();
        }

        public Task<int> SendShiftValue(PlcSignalForLocalizer task, RobotPose shiftValue, int nMaxTrials, int delay)
        {
            throw new NotImplementedException();
        }

        public Task<int> SendGapValue(float[] gapValues, int nMaxTrials, int delay)
        {
            throw new NotImplementedException();
        }

        public Task<int> SendLocalizerStatus(PlcSignalForLocalizer status, bool val, int nMaxTrials, int delay)
        {
            throw new NotImplementedException();
        }
    }
}
