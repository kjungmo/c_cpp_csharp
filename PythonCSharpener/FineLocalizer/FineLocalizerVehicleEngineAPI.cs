using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using CommonUtils;

namespace FineLocalizer
{
    static class FineLocalizerVehicleEngineAPI
    {
        public delegate void WriteLogCallback(string msg, LogLevel logLvl, string caller);
        private static WriteLogCallback _scb;
        public static WriteLogCallback LogCallback
        {
            set
            {
                _scb = value;
                SetLogCallback(_scb);
            }
        }
        [DllImport("FineLocalizerVehicleEngine_Release.dll"), Conditional("WITH_DLL")]
        public static extern void SetLogCallback(WriteLogCallback cb);

        [DllImport("FineLocalizerVehicleEngine_Release.dll"), Conditional("WITH_DLL")]
        public static extern void InfoGlassScanCamSettings(int carType, int nCams, CamSettings[] camSettings);

        [DllImport("FineLocalizerVehicleEngine_Release.dll"), Conditional("WITH_DLL")]
        public static extern void InfoVehicleScanCamSettings(int carType, int nCams, CamSettings[] camSettings);

        [DllImport("FineLocalizerVehicleEngine_Release.dll"), Conditional("WITH_DLL")]
        public static extern void InfoGapScanCamSettings(int carType, int nCams, CamSettings[] camSettings);

        [DllImport("FineLocalizerVehicleEngine_Release.dll"), Conditional("WITH_DLL")]
        public static extern void InfoGlassCheckerSettings(int carType, GlassCheckerSettings settings);

        [DllImport("FineLocalizerVehicleEngine_Release.dll"), Conditional("WITH_DLL")]
        public static extern void InfoVehicleAlignerSettings(int carType, VehicleAlignerSettings settings);

        [DllImport("FineLocalizerVehicleEngine_Release.dll"), Conditional("WITH_DLL")]
        public static extern void InfoGapCheckerSettings(int carType, GapCheckerSettings settings);

        [DllImport("FineLocalizerVehicleEngine_Release.dll"), Conditional("WITH_DLL")]
        public static extern void InfoCarType(int type);

        [DllImport("FineLocalizerVehicleEngine_Release.dll")]
        [return:MarshalAs(UnmanagedType.I1)]
        public static extern bool ReadyGlassCheck();

        [DllImport("FineLocalizerVehicleEngine_Release.dll")]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool ReadyVehicleCheck();

        [DllImport("FineLocalizerVehicleEngine_Release.dll")]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool ReadyGapCheck();

        [DllImport("FineLocalizerVehicleEngine_Release.dll")]
        [return:MarshalAs(UnmanagedType.I1)]
        public static extern bool ConnectCameras();

        [DllImport("FineLocalizerVehicleEngine_Release.dll")]
        [return:MarshalAs(UnmanagedType.I1)]
        public static extern bool DisconnectCameras();

        [DllImport("FineLocalizerVehicleEngine_Release.dll")]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool TriggerScanVehicle(int scanPoint, [MarshalAs(UnmanagedType.I1)] bool isRef, [MarshalAs(UnmanagedType.I1)] bool save);

        [DllImport("FineLocalizerVehicleEngine_Release.dll")]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool TriggerScanGap([MarshalAs(UnmanagedType.I1)] bool isRef, [MarshalAs(UnmanagedType.I1)] bool save);

        [DllImport("FineLocalizerVehicleEngine_Release.dll")]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool EstimateVehicleShift([In, Out] Pose7D[] updatingPoses,
                                                       out IntPtr target1, out IntPtr source1, out IntPtr sourceAligned1,
                                                       out IntPtr target2, out IntPtr source2, out IntPtr sourceAligned2);

        [DllImport("FineLocalizerVehicleEngine_Release.dll")]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool EstimateGapScanPoses([In, Out] Pose7D[] updatingPoses);

        [DllImport("FineLocalizerVehicleEngine_Release.dll")]
        [return:MarshalAs(UnmanagedType.I1)]
        public static extern bool InfoVehicleScanPose(int scanPoint, Pose7D position, [MarshalAs(UnmanagedType.I1)] bool isRef);

        [DllImport("FineLocalizerVehicleEngine_Release.dll")]
        [return:MarshalAs(UnmanagedType.I1)]
        public static extern bool InfoGapScanPose(int scanPoint, Pose7D position, [MarshalAs(UnmanagedType.I1)] bool isRef);

        [DllImport("FineLocalizerVehicleEngine_Release.dll")]
        [return:MarshalAs(UnmanagedType.I1)]
        public static extern bool InfoInstallPoses(Pose7D[] installPoses);

        [DllImport("FineLocalizerVehicleEngine_Release.dll")]
        public static extern PointNormal GetPointNormalVectorValue(IntPtr hVec, int idx);

        [DllImport("FineLocalizerVehicleEngine_Release.dll")]
        public static extern int GetPointNormalVectorLength(IntPtr hVec);

        [DllImport("FineLocalizerVehicleEngine_Release.dll")]
        [return:MarshalAs(UnmanagedType.I1)]
        public static extern bool MeasureGap([In, Out] double[] gapAverages, [In, Out] int[] numsGapExamined,
                                             IntPtr resultImages, [In, Out] int[] resultImageLengths);

        [DllImport("FineLocalizerVehicleEngine_Release.dll")]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool TriggerScanGlass(bool isRef, [MarshalAs(UnmanagedType.I1)] bool save);

        [DllImport("FineLocalizerVehicleEngine_Release.dll")]
        [return:MarshalAs(UnmanagedType.I1)]
        public static extern bool InfoGlassGripPoses([In] Pose7D[] poses, [MarshalAs(UnmanagedType.I1)] bool isRef);

        [DllImport("FineLocalizerVehicleEngine_Release.dll")]
        [return:MarshalAs(UnmanagedType.I1)]
        public static extern bool InfoGlassScanPose(int scanPoint, Pose7D position, [MarshalAs(UnmanagedType.I1)] bool isRef);

        [DllImport("FineLocalizerVehicleEngine_Release.dll")]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool EstimateGlassGripPose([In, Out] Pose7D[] updatingPoses,
                                                        out IntPtr target1, out IntPtr source1, out IntPtr sourceAligned1,
                                                        out IntPtr target2, out IntPtr source2, out IntPtr sourceAligned2,
                                                        out float xAvg1, out float yAvg1, out float xAvg2, out float yAvg2);

        [DllImport("FineLocalizerVehicleEngine_Release.dll")]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool TeachGap(int roiId); 

        [DllImport("FineLocalizerVehicleEngine_Release.dll")]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool AddGapData(int roiId);

        [DllImport("FineLocalizerVehicleEngine_Release.dll")]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool ManageGapData(int roiId);
    }
}
