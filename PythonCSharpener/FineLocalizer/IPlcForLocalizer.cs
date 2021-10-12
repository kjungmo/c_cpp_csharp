using CommonUtils;
using System;
using System.Threading.Tasks;

namespace FineLocalizer
{
    public interface IPlcForLocalizer
    {
        event EventHandler<LocalizerEventArgs> GlassVisionStart;
        event EventHandler<LocalizerEventArgs> GlassVisionReset;
        event EventHandler<CarTypeUpdateEventArgs> CarTypeUpdate;

        event EventHandler<LocalizerEventArgs> BodyVisionStart;
        event EventHandler<LocalizerEventArgs> BodyVisionReset;

        event EventHandler<LocalizerEventArgs> GapVisionStart;
        event EventHandler<OnOffEventArgs> VisionEnd;
        event EventHandler<LocalizerEventArgs> GapVisionReset;

        bool GlassVisionPass { get; set; }
        bool BodyVisionPass { get; set; }
        bool GapVisionPass { get; set; }

        Task<int> SendLocalizerStatus(PlcSignalForLocalizer status, bool val, int nMaxTrials, int delay);
        Task<int> SendShiftValue(PlcSignalForLocalizer task, RobotPose shiftValue, int nMaxTrials, int delay);
        Task<int> SendGapValue(float[] gapValues, int nMaxTrials, int delay);
    }

    [Flags]
    public enum PlcSignalForLocalizer
    {
        GLASS = 0x1000_0000,
        BODY = 0x0100_0000,
        GAP = 0x0010_0000,

        VALUE = 0x4000_0000,

        VISION_UPDATE = 0x4000,
        CAR_TYPE = 0x5000,
        CAR_SEQ = 0x6000,

        VISION_START = 0x100,
        VISION_RESET = 0x200,
        VISION_END = 0x300,
        VISION_READY = 0x500,
        VISION_PASS = 0x600,

        P1 = 0x10,
        P2 = 0x20,
        P3 = 0x30,
        P4 = 0x40,

        VISION_OK = 0x0001,
        VISION_NG = 0x0002,
        SCAN_POSE_SENT = 0x0004,

        P1_COMPLETED = 0x0400,
        P2_COMPLETED = 0x0800,
        P3_COMPLETED = 0x1000,
        P4_COMPLETED = 0x2000,
        PLC_PASS = 0x3000,
        CLEAR = 0,
            
        SHIFT_X = 0x0001_0000,
        SHIFT_Y = 0x0002_0000,
        SHIFT_Z = 0x0003_0000,
        SHIFT_RX = 0x0004_0000,
        SHIFT_RY = 0x0005_0000,
        SHIFT_RZ = 0x0006_0000,

        GAP_1 = 0x0007_0000,
        GAP_2 = 0x0008_0000,
        GAP_3 = 0x0009_0000,
        GAP_4 = 0x000A_0000,

        CAR_TYPE_BIT0 = 0x8000,
        CAR_TYPE_BIT1 = 0x8100,
        CAR_TYPE_BIT2 = 0x8200,
        CAR_TYPE_BIT3 = 0x8300,
        CAR_TYPE_BIT4 = 0x8400,
        CAR_TYPE_BIT5 = 0x8500,
        CAR_TYPE_BIT6 = 0x8600,
        CAR_TYPE_BIT7 = 0x8700
    }
}