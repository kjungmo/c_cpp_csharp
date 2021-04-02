using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace LocalizationTesterD
{
    static class FineLocalizerVehicleEngineAPI
    {
        public delegate void WriteLogCallback(string msg);
        private static WriteLogCallback _scb;
        public static WriteLogCallback LogCallback
        {
            set
            {
                _scb = value;
                SetLogCallback(_scb);
            }
        }

        [DllImport("FineLocalizerVehicleEngine_Release.dll")]
        public static extern void SetLogCallback(WriteLogCallback cb);

        [DllImport("FineLocalizerVehicleEngine_Release.dll")]
        [return:MarshalAs(UnmanagedType.I1)]
        public static extern bool EstimateVehicleShift(out IntPtr updatingPoses, int numPoses,
                                                       [MarshalAs(UnmanagedType.LPStr)] string refDataPath,
                                                       [MarshalAs(UnmanagedType.LPStr)] string handEyeCalibFilePath,
                                                       [MarshalAs(UnmanagedType.LPStr)] string birdEyeCalibFilePath,
                                                       int scanInstallMode,
                                                       int scanRobotMaker, int installRobotMaker, int numScanPoints,
                                                       out IntPtr target, out IntPtr source, out IntPtr sourceAligned,
                                                       ref float xAvg, ref float yAvg, ref float zAvg,
                                                       ref float zMin, ref float zMax);

        [DllImport("FineLocalizerVehicleEngine_Release.dll")]
        [return:MarshalAs(UnmanagedType.I1)]
        public static extern bool EstimateGapScanPose(out IntPtr updatingPoses, int numPoses,
                                                      [MarshalAs(UnmanagedType.LPStr)] string handEyeCalibFilePath,
                                                      int robotMaker, int numScanPoints);

        
    }
}
