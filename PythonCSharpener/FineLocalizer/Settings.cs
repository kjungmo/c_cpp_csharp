using CommonUtils;
using System.Runtime.InteropServices;

namespace FineLocalizer
{
    [StructLayout(LayoutKind.Sequential)]
    public struct GlassCheckerSettings
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)] public string RefDataPath;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)] public string HandEyeCalibFilePath;
        public int RobotMaker;
        public int GlassType;
        public int NumPoses;
        public int NumScanPoints;
        public float VisualizationX1Average;
        public float VisualizationY1Average;
        public float VisualizationX2Average;
        public float VisualizationY2Average;
        public float MinDepth;
        public float MaxDepth;
        public float PlaneDistance;
        public float GlassPlaneDistance;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)] public string FlipUpsideDown;
        public int LeftMargin;
        public int RightMargin;
        public int TopMargin;
        public int BottomMargin;
        public float MaxTranslationX;
        public float MaxTranslationY;
        public float MaxRotation;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)] public string OptPolicy;
        public int EdgeParameter1;
        public int EdgeParameter2;
		public int SamplingX;
		public int SamplingY;
		public int NormalizeParameter;
        public int GlassIntensityThreshold;
		public float ConnectivityThreshold;
        [MarshalAs(UnmanagedType.I1)] public bool VisualizeDebuggingImages;
        [MarshalAs(UnmanagedType.I1)] public bool UpdateToVehicleShift;

        public GlassCheckerSettings(FineLocalizerConfig config, RobotMaker robotMaker)
        {
            RefDataPath = config.GlassRefDataPath;
            HandEyeCalibFilePath = config.GlassHandEyeCalibFilePath;
            RobotMaker = (int)robotMaker;
            GlassType = (int)config.GlassType;
            NumPoses = config.NumPosesForGlass;
            NumScanPoints = config.NumScanPointsForGlass;
            VisualizationX1Average = config.VisualizationX1Average;
            VisualizationY1Average = config.VisualizationY1Average;
            VisualizationX2Average = config.VisualizationX2Average;
            VisualizationY2Average = config.VisualizationY2Average;
            MinDepth = config.MinDepthForGlass;
            MaxDepth = config.MaxDepthForGlass;
            PlaneDistance = config.PlaneDistanceForGlass;
            GlassPlaneDistance = config.GlassPlaneDistanceForGlass;
            FlipUpsideDown = config.FlipUpsideDown;
            LeftMargin = config.LeftMarginForGlass;
            RightMargin = config.RightMarginForGlass;
            TopMargin = config.TopMarginForGlass;
            BottomMargin = config.BottomMarginForGlass;
            MaxTranslationX = config.MaxTranslationXForGlass;
            MaxTranslationY = config.MaxTranslationYForGlass;
            MaxRotation = config.MaxRotation;
            OptPolicy = config.OptPolicyForGlass;
            EdgeParameter1 = config.EdgeParameter1ForGlass;
            EdgeParameter2 = config.EdgeParameter2ForGlass;
            SamplingX = config.SamplingXForGlass;
            SamplingY = config.SamplingYForGlass;
            NormalizeParameter = config.NormalizeParameterForGlass;
            GlassIntensityThreshold = config.GlassIntensityThresholdForGlass;
            ConnectivityThreshold = config.ConnectivityThresholdForGlass;
            VisualizeDebuggingImages = config.VisualizeDebuggingImagesForGlass;
            UpdateToVehicleShift = config.UpdateToVehicleShift;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct VehicleAlignerSettings
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)] public string RefDataPath;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)] public string HandEyeCalibFilePath;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)] public string BirdEyeCalibFilePath;
        public int ScanInstallMode;
        public int ScanRobotMaker;
        public int InstallRobotMaker;
        public int SamplingRate;
        public int NumPoses;
        public int NumScanPoints;
        public float MinDepth;
        public float MaxDepth;
        public float VisualizationXAverage;
        public float VisualizationYAverage;
        public float VisualizationZAverage;
        public float VisualizationZMin;
        public float VisualizationZMax;
        public float VisualizationZRotation;
        public int MaxNumIterations;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)] public string OptPolicy;
        public float MaxTranslationX;
        public float MaxTranslationY;
        public float MaxTranslationZ;
        public float MaxRotationX;
        public float MaxRotationY;
        public float MaxRotationZ;

        public VehicleAlignerSettings(FineLocalizerConfig config, RobotMaker scanRobotMaker, RobotMaker installRobotMaker)
        {
            RefDataPath = config.VehicleRefDataPath;
            HandEyeCalibFilePath = config.VehicleHandEyeCalibFilePath;
            BirdEyeCalibFilePath = config.VehicleBirdEyeCalibFilePath;
            ScanInstallMode = (int)config.ScanInstallMode;
            ScanRobotMaker = (int)scanRobotMaker;
            InstallRobotMaker = (int)installRobotMaker;
            SamplingRate = config.SamplingRate;
            NumPoses = config.NumPosesForVehicle;
            NumScanPoints = config.NumScanPointsForVehicle;
            VisualizationXAverage = config.VisualizationXAverage;
            VisualizationYAverage = config.VisualizationYAverage;
            VisualizationZAverage = config.VisualizationZAverage;
            VisualizationZMin = config.VisualizationZMin;
            VisualizationZMax = config.VisualizationZMax;
            VisualizationZRotation = config.VisualizationZRotation;
            MaxNumIterations = config.MaxNumIterations;
            OptPolicy = config.OptPolicyForVehicle;
            MaxTranslationX = config.MaxTranslationXForVehicle;
            MaxTranslationY = config.MaxTranslationYForVehicle;
            MaxTranslationZ = config.MaxTranslationZForVehicle;
            MaxRotationX = config.MaxRotationX;
            MaxRotationY = config.MaxRotationY;
            MaxRotationZ = config.MaxRotationZ;
            MinDepth = config.MinDepthForVehicle;
            MaxDepth = config.MaxDepthForVehicle;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct GapCheckerSettings
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)] public string RefDataPath;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)] public string HandEyeCalibFilePath;
        public int RobotMaker;
        public int GapType;
        public int NumPoses;
        public int NumScanPoints;
        public float MinDepth;
        public float MaxDepth;
        public float MinGap;
        public float MaxGap;
        public float MaxGapDiff1;
        public float MaxGapDiff2;
        public int PillarGlassPixelDistance;
        public float PlaneDistance;
        public float PillarPlaneDistance;
        public float GlassPlaneDistance;
        public int LeftMargin;
        public int RightMargin;
        public int TopMargin;
        public int BottomMargin;
        public int EdgeParameter1;
        public int EdgeParameter2;
		public int SamplingX;
		public int SamplingY;
		public int NormalizeParameter;
        public int GlassIntensityThreshold;
		public float ConnectivityThreshold;
        [MarshalAs(UnmanagedType.I1)] public bool VisualizeDebuggingImages;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)] public string ResultImageRotations;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)] public string RoiScanId;

        public GapCheckerSettings(FineLocalizerConfig config, RobotMaker robotMaker)
        {
            RefDataPath = config.GapRefDataPath;
            HandEyeCalibFilePath = config.GapHandEyeCalibFilePath;
            RobotMaker = (int)robotMaker; 
            GapType = (int)config.GapType;
            NumPoses = config.NumPosesForGap;
            NumScanPoints = config.NumScanPointsForGap;
            MinDepth = config.MinDepthForGap;
            MaxDepth = config.MaxDepthForGap;
            MinGap = config.MinGap;
            MaxGap = config.MaxGap;
            MaxGapDiff1 = config.MaxGapDiff1;
            MaxGapDiff2 = config.MaxGapDiff2;
            PillarGlassPixelDistance = config.PillarGlassPixelDistance;
            PlaneDistance = config.PlaneDistanceForGap;
            PillarPlaneDistance = config.PillarPlaneDistance;
            GlassPlaneDistance = config.GlassPlaneDistanceForGap;
            GlassIntensityThreshold = config.GlassIntensityThresholdForGap;
            LeftMargin = config.LeftMarginForGap;
            RightMargin = config.RightMarginForGap;
            TopMargin = config.TopMarginForGap;
            BottomMargin = config.BottomMarginForGap;
            EdgeParameter1 = config.EdgeParameter1ForGap;
            EdgeParameter2 = config.EdgeParameter2ForGap;
            SamplingX = config.SamplingXForGap;
            SamplingY = config.SamplingYForGap;
            NormalizeParameter = config.NormalizeParameterForGap;
            GlassIntensityThreshold = config.GlassIntensityThresholdForGap;
            ConnectivityThreshold = config.ConnectivityThresholdForGap;
            VisualizeDebuggingImages = config.VisualizeDebuggingImagesForGap;
            ResultImageRotations = config.ResultImageRotations;
            RoiScanId = config.RoiScanId;
        }
    }
}
