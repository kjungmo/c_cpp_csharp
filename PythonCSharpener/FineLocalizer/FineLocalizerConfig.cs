using CommonUtils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;

namespace FineLocalizer
{
    [Serializable]
    public class FineLocalizerConfig
    {
        [LocalizedCategory("CategoryGeneral", 1, 4)]
        [LocalizedDescription("DescCarName")]
        public string CarName { get; set; } = "unknown";

        [Browsable(false)]
        public string InstallRobot { get; set; }

        [Browsable(false)]
        public string ScanRobot { get; set; }

        [Browsable(false)]
        public string VehicleCamera { get; set; }

        [Browsable(false)]
        public string GapCamera { get; set; }

        [Browsable(false)]
        public string GlassCamera { get; set; }

        #region glass checker
        [LocalizedCategory("CategoryGlassChecker", 2, 4)]
        [LocalizedDescription("DescUseGlassChecker")]
        public bool UseGlassChecker { get; set; } = true;

        [LocalizedCategory("CategoryGlassChecker", 2, 4)]
        [LocalizedDescription("DescGlassScanMode")]
        public CameraScanMode GlassScanMode { get; set; } = CameraScanMode.SingleCamera;

        [LocalizedCategory("CategoryGlassChecker", 2, 4)]
        [LocalizedDescription("DescGlassRefDataPath")]
        [Editor(typeof(FolderPathEditor), typeof(UITypeEditor)), ValidatorType(ValidatorType.FolderPath)]
        public string GlassRefDataPath { get; set; } = "C:/";

        [LocalizedCategory("CategoryGlassChecker", 2, 4)]
        [LocalizedDescription("DescGlassHandEyeCalibFilePath")]
        [Editor(typeof(FolderPathEditor), typeof(UITypeEditor)), ValidatorType(ValidatorType.FolderPath)]
        public string GlassHandEyeCalibFilePath { get; set; } = "C:/";

        [LocalizedCategory("CategoryGlassChecker", 2, 4)]
        [LocalizedDescription("DescGlassType")]
        public GlassCategory GlassType { get; set; } = GlassCategory.Normal;

        [LocalizedCategory("CategoryGlassChecker", 2, 4)]
        [LocalizedDescription("DescNumPoses")]
        [ValidatorType(ValidatorType.Int, 1, 3)]
        public int NumPosesForGlass { get; set; } = 1;

        [LocalizedCategory("CategoryGlassChecker", 2, 4)]
        [LocalizedDescription("DescNumScanPoints")]
        [ValidatorType(ValidatorType.Int, 1, 4)]
        public int NumScanPointsForGlass { get; set; } = 1;

        [LocalizedCategory("CategoryGlassChecker", 2, 4)]
        [LocalizedDescription("DescVisualizationXAverage")]
        [ValidatorType(ValidatorType.Float, -2000.0f, 2000.0f)]
        public float VisualizationX1Average { get; set; } = 0.0f;

        [LocalizedCategory("CategoryGlassChecker", 2, 4)]
        [LocalizedDescription("DescVisualizationYAverage")]
        [ValidatorType(ValidatorType.Float, -2000.0f, 2000.0f)]
        public float VisualizationY1Average { get; set; } = 0.0f;

        [LocalizedCategory("CategoryGlassChecker", 2, 4)]
        [LocalizedDescription("DescVisualizationXAverage")]
        [ValidatorType(ValidatorType.Float, -2000.0f, 2000.0f)]
        public float VisualizationX2Average { get; set; } = 0.0f;

        [LocalizedCategory("CategoryGlassChecker", 2, 4)]
        [LocalizedDescription("DescVisualizationYAverage")]
        [ValidatorType(ValidatorType.Float, -2000.0f, 2000.0f)]
        public float VisualizationY2Average { get; set; } = 0.0f;

        [LocalizedCategory("CategoryGlassChecker", 2, 4)]
        [LocalizedDescription("DescMinDepthForGlass")]
        [ValidatorType(ValidatorType.Float, 200.0f, 500.0f)]
        public float MinDepthForGlass { get; set; } = 300.0f;

        [LocalizedCategory("CategoryGlassChecker", 2, 4)]
        [LocalizedDescription("DescMaxDepthForGlass")]
        [ValidatorType(ValidatorType.Float, 300.0f, 2000.0f)]
        public float MaxDepthForGlass { get; set; } = 1000.0f;

        [LocalizedCategory("CategoryGlassChecker", 2, 4)]
        [LocalizedDescription("DescPlaneDistanceForGlass")]
        [ValidatorType(ValidatorType.Float, 1.0f, 50.0f)]
        public float PlaneDistanceForGlass { get; set; } = 10.0f;

        [LocalizedCategory("CategoryGlassChecker", 2, 4)]
        [LocalizedDescription("DescGlassPlaneDistanceForGlass")]
        [ValidatorType(ValidatorType.Float, 1.0f, 50.0f)]
        public float GlassPlaneDistanceForGlass { get; set; } = 10.0f;

        [LocalizedCategory("CategoryGlassChecker", 2, 4)]
        [LocalizedDescription("DescFlipUpsideDown")]
        [ValidatorType(ValidatorType.Ssv)]
        public string FlipUpsideDown { get; set; } = "1 0";

        [LocalizedCategory("CategoryGlassChecker", 2, 4)]
        [LocalizedDescription("DescLeftMarginForGlass")]
        [ValidatorType(ValidatorType.Int, 0, 9999)]
        public int LeftMarginForGlass { get; set; } = 0;

        [LocalizedCategory("CategoryGlassChecker", 2, 4)]
        [LocalizedDescription("DescRightMarginForGlass")]
        [ValidatorType(ValidatorType.Int, 0, 9999)]
        public int RightMarginForGlass { get; set; } = 300;

        [LocalizedCategory("CategoryGlassChecker", 2, 4)]
        [LocalizedDescription("DescTopMarginForGlass")]
        [ValidatorType(ValidatorType.Int, 0, 9999)]
        public int TopMarginForGlass { get; set; } = 300;

        [LocalizedCategory("CategoryGlassChecker", 2, 4)]
        [LocalizedDescription("DescBottomMarginForGlass")]
        [ValidatorType(ValidatorType.Int, 0, 9999)]
        public int BottomMarginForGlass { get; set; } = 0;

        [LocalizedCategory("CategoryGlassChecker", 2, 4)]
        [LocalizedDescription("DescMaxTranslationXForGlass")]
        [ValidatorType(ValidatorType.Float, 1.0f, 20.0f)]
        public float MaxTranslationXForGlass { get; set; } = 6.0f;

        [LocalizedCategory("CategoryGlassChecker", 2, 4)]
        [LocalizedDescription("DescMaxTranslationYForGlass")]
        [ValidatorType(ValidatorType.Float, 1.0f, 20.0f)]
        public float MaxTranslationYForGlass { get; set; } = 6.0f;

        [LocalizedCategory("CategoryGlassChecker", 2, 4)]
        [LocalizedDescription("DescMaxRotation")]
        [ValidatorType(ValidatorType.Float, 0.1f, 1.0f)]
        public float MaxRotation { get; set; } = 0.5f;

        [LocalizedCategory("CategoryGlassChecker", 2, 4)]
        [LocalizedDescription("DescOptPolicyForGlass")]
        [ValidatorType(ValidatorType.Ssv)]
        public string OptPolicyForGlass { get; set; } = "50 20 5 1 0.5";

        [LocalizedCategory("CategoryGlassChecker", 2, 4)]
        [LocalizedDescription("DescEdgeParameter1ForGlass")]
        [ValidatorType(ValidatorType.Int, 0, 500)]
        public int EdgeParameter1ForGlass { get; set; } = 100;

        [LocalizedCategory("CategoryGlassChecker", 2, 4)]
        [LocalizedDescription("DescEdgeParameter2ForGlass")]
        [ValidatorType(ValidatorType.Int, 0, 500)]
        public int EdgeParameter2ForGlass { get; set; } = 100;

        [LocalizedCategory("CategoryGlassChecker", 2, 4)]
        [LocalizedDescription("DescSamplingXForGlass")]
        [ValidatorType(ValidatorType.Int, 1, 2048)]
        public int SamplingXForGlass { get; set; } = 32;

        [LocalizedCategory("CategoryGlassChecker", 2, 4)]
        [LocalizedDescription("DescSamplingYForGlass")]
        [ValidatorType(ValidatorType.Int, 1, 2448)]
        public int SamplingYForGlass { get; set; } = 32;

        [LocalizedCategory("CategoryGlassChecker", 2, 4)]
        [LocalizedDescription("DescNormalizeParameterForGlass")]
        [ValidatorType(ValidatorType.Int, 1, 255)]
		public int NormalizeParameterForGlass { get; set; } = 50;

        [LocalizedCategory("CategoryGlassChecker", 2, 4)]
        [LocalizedDescription("DescGlassIntensityThresholdForGlass")]
        [ValidatorType(ValidatorType.Int, 0, 255)]
		public int GlassIntensityThresholdForGlass { get; set; } = 5;

        [LocalizedCategory("CategoryGlassChecker", 2, 4)]
        [LocalizedDescription("DescConnectivityThresholdForGlass")]
        [ValidatorType(ValidatorType.Float, 0.01f, 1000.0f)]
        public float ConnectivityThresholdForGlass { get; set; } = 0.2f;

        [LocalizedCategory("CategoryGlassChecker", 2, 4)]
        [LocalizedDescription("DescVisualizeDebuggingImagesForGlass")]
        public bool VisualizeDebuggingImagesForGlass { get; set; } = false;

        [LocalizedCategory("CategoryGlassChecker", 2, 4)]
        [LocalizedDescription("DescUpdateToVehicleShift")]
        public bool UpdateToVehicleShift { get; set; } = false;

        #endregion

        #region vehicle aligner
        [LocalizedCategory("CategoryVehicleAligner", 3, 4)]
        [LocalizedDescription("DescVehicleScanMode")]
        public CameraScanMode VehicleScanMode { get; set; } = CameraScanMode.SingleCamera;

        [LocalizedCategory("CategoryVehicleAligner", 3, 4)]
        [LocalizedDescription("DescSamplingRate")]
        [ValidatorType(ValidatorType.Int, 1, 6)]
        public int SamplingRate { get; set; } = 3;

        [LocalizedCategory("CategoryVehicleAligner", 3, 4)]
        [LocalizedDescription("DescVehicleRefDataPath")]
        [Editor(typeof(FolderPathEditor), typeof(UITypeEditor)), ValidatorType(ValidatorType.FolderPath)]
        public string VehicleRefDataPath { get; set; } = "C:/";

        [LocalizedCategory("CategoryVehicleAligner", 3, 4)]
        [LocalizedDescription("DescVehicleHandEyeCalibFilePath")]
        [Editor(typeof(FolderPathEditor), typeof(UITypeEditor)), ValidatorType(ValidatorType.FolderPath)]
        public string VehicleHandEyeCalibFilePath { get; set; } = "C:/";

        [LocalizedCategory("CategoryVehicleAligner", 3, 4)]
        [LocalizedDescription("DescVehicleBirdEyeCalibFilePath")]
        [Editor(typeof(FilePathEditor), typeof(UITypeEditor)), ValidatorType(ValidatorType.FilePath)]
        public string VehicleBirdEyeCalibFilePath { get; set; } = "C:/";

        [LocalizedCategory("CategoryVehicleAligner", 3, 4)]
        [LocalizedDescription("DescNumPoses")]
        [ValidatorType(ValidatorType.Int, 1, 3)]
        public int NumPosesForVehicle { get; set; } = 1;

        [LocalizedCategory("CategoryVehicleAligner", 3, 4)]
        [LocalizedDescription("DescScanInstallMode")]
        public ScanInstallMode ScanInstallMode { get; set; } = ScanInstallMode.DIFF_SCAN_INSTALL;

        [LocalizedCategory("CategoryVehicleAligner", 3, 4)]
        [LocalizedDescription("DescNumScanPoints")]
        [ValidatorType(ValidatorType.Int, 1, 4)]
        public int NumScanPointsForVehicle { get; set; } = 1;

        [LocalizedCategory("CategoryVehicleAligner", 3, 4)]
        [LocalizedDescription("DescVisualizationXAverage")]
        [ValidatorType(ValidatorType.Float, -2000.0f, 2000.0f)]
        public float VisualizationXAverage { get; set; } = 0.0f;

        [LocalizedCategory("CategoryVehicleAligner", 3, 4)]
        [LocalizedDescription("DescVisualizationYAverage")]
        [ValidatorType(ValidatorType.Float, -2000.0f, 2000.0f)]
        public float VisualizationYAverage { get; set; } = 0.0f;

        [LocalizedCategory("CategoryVehicleAligner", 3, 4)]
        [LocalizedDescription("DescVisualizationZAverage")]
        [ValidatorType(ValidatorType.Float, 500.0f, 2000.0f)]
        public float VisualizationZAverage { get; set; } = 1000.0f;

        [LocalizedCategory("CategoryVehicleAligner", 3, 4)]
        [LocalizedDescription("DescVisualizationZMin")]
        [ValidatorType(ValidatorType.Float, 300.0f, 2000.0f)]
        public float VisualizationZMin { get; set; } = 600.0f;

        [LocalizedCategory("CategoryVehicleAligner", 3, 4)]
        [LocalizedDescription("DescVisualizationZMax")]
        [ValidatorType(ValidatorType.Float, 600.0f, 2000.0f)]
        public float VisualizationZMax { get; set; } = 1500.0f;

        [LocalizedCategory("CategoryVehicleAligner", 3, 4)]
        [LocalizedDescription("DescVisualizationZRotation")]
        [ValidatorType(ValidatorType.Float, -180.0f, 180.0f)]
        public float VisualizationZRotation { get; set; } = 90.0f;

        [LocalizedCategory("CategoryVehicleAligner", 3, 4)]
        [LocalizedDescription("DescMaxNumIterations")]
        [ValidatorType(ValidatorType.Int, 1, 1000)]
        public int MaxNumIterations { get; set; } = 100;

        [LocalizedCategory("CategoryVehicleAligner", 3, 4)]
        [LocalizedDescription("DescOptPolicyForVehicle")]
        public string OptPolicyForVehicle { get; set; } = "10 2 1 0.5";

        [LocalizedCategory("CategoryVehicleAligner", 3, 4)]
        [LocalizedDescription("DescMaxTranslationXForVehicle")]
        [ValidatorType(ValidatorType.Float, 5.0f, 50.0f)]
        public float MaxTranslationXForVehicle { get; set; } = 30.0f;

        [LocalizedCategory("CategoryVehicleAligner", 3, 4)]
        [LocalizedDescription("DescMaxTranslationYForVehicle")]
        [ValidatorType(ValidatorType.Float, 5.0f, 50.0f)]
        public float MaxTranslationYForVehicle { get; set; } = 30.0f;

        [LocalizedCategory("CategoryVehicleAligner", 3, 4)]
        [LocalizedDescription("DescMaxTranslationZForVehicle")]
        [ValidatorType(ValidatorType.Float, 5.0f, 50.0f)]
        public float MaxTranslationZForVehicle { get; set; } = 30.0f;

        [LocalizedCategory("CategoryVehicleAligner", 3, 4)]
        [LocalizedDescription("DescMaxRotationX")]
        [ValidatorType(ValidatorType.Float, 1.0f, 6.0f)]
        public float MaxRotationX { get; set; } = 3.0f;

        [LocalizedCategory("CategoryVehicleAligner", 3, 4)]
        [LocalizedDescription("DescMaxRotationY")]
        [ValidatorType(ValidatorType.Float, 1.0f, 6.0f)]
        public float MaxRotationY { get; set; } = 3.0f;

        [LocalizedCategory("CategoryVehicleAligner", 3, 4)]
        [LocalizedDescription("DescMaxRotationZ")]
        [ValidatorType(ValidatorType.Float, 1.0f, 6.0f)]
        public float MaxRotationZ { get; set; } = 3.0f;

        [LocalizedCategory("CategoryVehicleAligner", 3, 4)]
        [LocalizedDescription("DescMinDepthForVehicle")]
        [ValidatorType(ValidatorType.Float, 0.0f, 1500.0f)]
        public float MinDepthForVehicle { get; set; } = 300.0f;

        [LocalizedCategory("CategoryVehicleAligner", 3, 4)]
        [LocalizedDescription("DescMaxDepthForVehicle")]
        [ValidatorType(ValidatorType.Float, 0.0f, 1500.0f)]
        public float MaxDepthForVehicle { get; set; } = 1000.0f;

        #endregion

        #region gap checker
        [LocalizedCategory("CategoryGapChecker", 4, 4)]
        [LocalizedDescription("DescUseGapChecker")]
        public bool UseGapChecker { get; set; } = true;

        [LocalizedCategory("CategoryGapChecker", 4, 4)]
        [LocalizedDescription("DescGapScanMode")]
        public CameraScanMode GapScanMode { get; set; } = CameraScanMode.SingleCamera;

        [LocalizedCategory("CategoryGapChecker", 4, 4)]
        [LocalizedDescription("DescGapRefDataPath")]
        [Editor(typeof(FolderPathEditor), typeof(UITypeEditor)), ValidatorType(ValidatorType.FolderPath)]
        public string GapRefDataPath { get; set; } = "C:/";

        [LocalizedCategory("CategoryGapChecker", 4, 4)]
        [LocalizedDescription("DescGapHandEyeCalibFilePath")]
        [Editor(typeof(FolderPathEditor), typeof(UITypeEditor)), ValidatorType(ValidatorType.FolderPath)]
        public string GapHandEyeCalibFilePath { get; set; } = "C:/";

        [LocalizedCategory("CategoryGapChecker", 4, 4)]
        [LocalizedDescription("DescNumPoses")]
        [ValidatorType(ValidatorType.Int, 1, 3)]
        public int NumPosesForGap { get; set; } = 1;

        [LocalizedCategory("CategoryGapChecker", 4, 4)]
        [LocalizedDescription("DescNumScanPoints")]
        [ValidatorType(ValidatorType.Int, 1, 4)]
        public int NumScanPointsForGap { get; set; } = 1;

        [LocalizedCategory("CategoryGapChecker", 4, 4)]
        [LocalizedDescription("DescGapType")]
        public GapCategory GapType { get; set; } = GapCategory.PillarSalient1;

        [LocalizedCategory("CategoryGapChecker", 4, 4)]
        [LocalizedDescription("DescMinDepthForGap")]
        [ValidatorType(ValidatorType.Float, 200.0f, 500.0f)]
        public float MinDepthForGap { get; set; } = 300.0f;

        [LocalizedCategory("CategoryGapChecker", 4, 4)]
        [LocalizedDescription("DescMaxDepthForGap")]
        [ValidatorType(ValidatorType.Float, 300.0f, 2000.0f)]
        public float MaxDepthForGap { get; set; } = 1000.0f;

        [LocalizedCategory("CategoryGapChecker", 4, 4)]
        [LocalizedDescription("DescMinGap")]
        [ValidatorType(ValidatorType.Float, 1.0f, 10.0f)]
        public float MinGap { get; set; } = 2.0f;

        [LocalizedCategory("CategoryGapChecker", 4, 4)]
        [LocalizedDescription("DescMaxGap")]
        [ValidatorType(ValidatorType.Float, 1.0f, 100.0f)]
        public float MaxGap { get; set; } = 20.0f;

        [LocalizedCategory("CategoryGapChecker", 4, 4)]
        [LocalizedDescription("DescMaxGapDiff1")]
        [ValidatorType(ValidatorType.Float, 0.1f, 20.0f)]
        public float MaxGapDiff1 { get; set; } = 1.0f;

        [LocalizedCategory("CategoryGapChecker", 4, 4)]
        [LocalizedDescription("DescMaxGapDiff2")]
        [ValidatorType(ValidatorType.Float, 0.1f, 20.0f)]
        public float MaxGapDiff2 { get; set; } = 1.0f;

        [LocalizedCategory("CategoryGapChecker", 4, 4)]
        [LocalizedDescription("DescPillarGlassPixelDistance")]
        [ValidatorType(ValidatorType.Int, 0, 9999)]
        public int PillarGlassPixelDistance { get; set; } = 3;

        [LocalizedCategory("CategoryGapChecker", 4, 4)]
        [LocalizedDescription("DescPlaneDistanceForGap")]
        [ValidatorType(ValidatorType.Float, 0.01f, 30.0f)]
        public float PlaneDistanceForGap { get; set; } = 0.1f;

        [LocalizedCategory("CategoryGapChecker", 4, 4)]
        [LocalizedDescription("DescPillarPlaneDistance")]
        [ValidatorType(ValidatorType.Float, -100.0f, 100.0f)]
        public float PillarPlaneDistance { get; set; } = -0.5f;

        [LocalizedCategory("CategoryGapChecker", 4, 4)]
        [LocalizedDescription("DescGlassPlaneDistanceForGap")]
        [ValidatorType(ValidatorType.Float, -100.0f, 100.0f)]
        public float GlassPlaneDistanceForGap { get; set; } = 1.0f;

        [LocalizedCategory("CategoryGapChecker", 4, 4)]
        [LocalizedDescription("DescLeftMarginForGap")]
        [ValidatorType(ValidatorType.Int, 0, 9999)]
        public int LeftMarginForGap { get; set; } = 5;

        [LocalizedCategory("CategoryGapChecker", 4, 4)]
        [LocalizedDescription("DescRightMarginForGap")]
        [ValidatorType(ValidatorType.Int, 0, 9999)]
        public int RightMarginForGap { get; set; } = 5;

        [LocalizedCategory("CategoryGapChecker", 4, 4)]
        [LocalizedDescription("DescBottomMarginForGap")]
        [ValidatorType(ValidatorType.Int, 0, 9999)]
        public int BottomMarginForGap { get; set; } = 5;

        [LocalizedCategory("CategoryGapChecker", 4, 4)]
        [LocalizedDescription("DescTopMarginForGap")]
        [ValidatorType(ValidatorType.Int, 0, 9999)]
        public int TopMarginForGap { get; set; } = 5;

        [LocalizedCategory("CategoryGapChecker", 4, 4)]
        [LocalizedDescription("DescEdgeParameter1ForGap")]
        [ValidatorType(ValidatorType.Int, 0, 500)]
        public int EdgeParameter1ForGap { get; set; } = 100;

        [LocalizedCategory("CategoryGapChecker", 4, 4)]
        [LocalizedDescription("DescEdgeParameter2ForGap")]
        [ValidatorType(ValidatorType.Int, 0, 500)]
        public int EdgeParameter2ForGap { get; set; } = 100;

        [LocalizedCategory("CategoryGapChecker", 4, 4)]
        [LocalizedDescription("DescSamplingXForGap")]
        [ValidatorType(ValidatorType.Int, 1, 2048)]
        public int SamplingXForGap { get; set; } = 32;

        [LocalizedCategory("CategoryGapChecker", 4, 4)]
        [LocalizedDescription("DescSamplingYForGap")]
        [ValidatorType(ValidatorType.Int, 1, 2448)]
        public int SamplingYForGap { get; set; } = 32;

        [LocalizedCategory("CategoryGapChecker", 4, 4)]
        [LocalizedDescription("DescNormalizeParameterForGap")]
        [ValidatorType(ValidatorType.Int, 1, 255)]
		public int NormalizeParameterForGap { get; set; } = 50;

        [LocalizedCategory("CategoryGapChecker", 4, 4)]
        [LocalizedDescription("DescGlassIntensityThresholdForGap")]
        [ValidatorType(ValidatorType.Int, 0, 255)]
		public int GlassIntensityThresholdForGap { get; set; } = 5;

        [LocalizedCategory("CategoryGapChecker", 4, 4)]
        [LocalizedDescription("DescConnectivityThresholdForGap")]
        [ValidatorType(ValidatorType.Float, 0.01f, 1000.0f)]
        public float ConnectivityThresholdForGap { get; set; } = 0.2f;

        [LocalizedCategory("CategoryGapChecker", 4, 4)]
        [LocalizedDescription("DescVisualizeDebuggingImagesForGap")]
        public bool VisualizeDebuggingImagesForGap { get; set; } = false;

        [LocalizedCategory("CategoryGapChecker", 4, 4)]
        [LocalizedDescription("DescResultImagesRotations")]
        [ValidatorType(ValidatorType.Ssv)]
        public string ResultImageRotations { get; set; } = "0 0 0 0";

        [LocalizedCategory("CategoryGapChecker", 4, 4)]
        [LocalizedDescription("DescRoiScanId")]
        [ValidatorType(ValidatorType.Ssv)]
        public string RoiScanId { get; set; } = "1 1 0 0";

        #endregion
    }

    public static class DefaultSettingLoader
    {
        public static Dictionary<RobotMaker, Func<Dictionary<RobotAttribute, string>>> Robots = new Dictionary<RobotMaker, Func<Dictionary<RobotAttribute, string>>>()
        {
            [RobotMaker.YASKAWA] = GetYaskawaSettings,
            [RobotMaker.KAWASAKI] = GetKawasakiSettings,
            [RobotMaker.FANUC] = GetFanucSettings,
            [RobotMaker.HYUNDAI] = GetHyundaiSettings
        };

        public static Dictionary<RobotAttribute, string> GetYaskawaSettings()
        {
            return new Dictionary<RobotAttribute, string>()
            {
                [RobotAttribute.Maker] = RobotMaker.YASKAWA.ToString(),
                [RobotAttribute.Ip] = "",
                [RobotAttribute.Port] = "",
                [RobotAttribute.YrcCoordinateSystem] = YrcCoordinateSystem.BASE.ToString(),
            };
        }

        public static Dictionary<RobotAttribute, string> GetKawasakiSettings()
        {
            return new Dictionary<RobotAttribute, string>()
            {
                [RobotAttribute.Maker] = RobotMaker.KAWASAKI.ToString(),
                [RobotAttribute.Ip] = "",
                [RobotAttribute.Port] = "",
                [RobotAttribute.VehicleInstallVars] = "",
                [RobotAttribute.VehicleShiftVars] = "",
                [RobotAttribute.GapScanPoseVars] = "",
                [RobotAttribute.GapScanPoseShiftVars] = "",
                [RobotAttribute.GlassGripPoseVars] = "",
                [RobotAttribute.GlassGripPoseShiftVars] = "",
                [RobotAttribute.MaxNumberOfTrials] = ""
            };
        }

        public static Dictionary<RobotAttribute, string> GetFanucSettings()
        {
            return new Dictionary<RobotAttribute, string>()
            {
                [RobotAttribute.Maker] = RobotMaker.FANUC.ToString(),
                [RobotAttribute.Ip] = "",
                [RobotAttribute.Port] = "",
                [RobotAttribute.VehicleInstallVars] = "",
                [RobotAttribute.VehicleShiftVars] = "",
                [RobotAttribute.GapScanPoseVars] = "",
                [RobotAttribute.GapScanPoseShiftVars] = "",
                [RobotAttribute.GlassGripPoseVars] = "",
                [RobotAttribute.GlassGripPoseShiftVars] = "",
                [RobotAttribute.UserFrame] = FanucUserFrame.WORLD.ToString()
            };
        }

        public static Dictionary<RobotAttribute, string> GetHyundaiSettings()
        {
            return new Dictionary<RobotAttribute, string>()
            {
                [RobotAttribute.Maker] = RobotMaker.HYUNDAI.ToString(),
                [RobotAttribute.Ip] = "192.168.178.206",
                [RobotAttribute.ClientIp] = "192.168.178.102",
                [RobotAttribute.VehicleInstallVars] = "",
                [RobotAttribute.VehicleShiftVars] = "",
                [RobotAttribute.GapScanPoseVars] = "",
                [RobotAttribute.GapScanPoseShiftVars] = "",
                [RobotAttribute.GlassGripPoseVars] = "",
                [RobotAttribute.GlassGripPoseShiftVars] = "",
                [RobotAttribute.HrCoordinateSystem] = HrCoordinateSystem.BASE.ToString()
            };
        }

        public static Dictionary<CameraModel, Func<Dictionary<CameraAttribute, string>>> Cameras = new Dictionary<CameraModel, Func<Dictionary<CameraAttribute, string>>>()
        {
            [CameraModel.CoPick3D_250] = GetCoPick3D250Settings,
            [CameraModel.CoPick3D_350] = GetCoPick3D350Settings,
            [CameraModel.Phoxi_M] = GetPhoxiMSettings,
            [CameraModel.Phoxi_S] = GetPhoxiSSettings
        };

        private static Dictionary<CameraAttribute, string> GetPhoxiSSettings()
        {
            return new Dictionary<CameraAttribute, string>()
            {
                [CameraAttribute.Model] = CameraModel.Phoxi_S.ToString(),
                [CameraAttribute.Serial] = "",
                [CameraAttribute.Ip] = ""
            };
        }

        private static Dictionary<CameraAttribute, string> GetPhoxiMSettings()
        {
            return new Dictionary<CameraAttribute, string>()
            {
                [CameraAttribute.Model] = CameraModel.Phoxi_M.ToString(),
                [CameraAttribute.Serial] = "",
                [CameraAttribute.Ip] = ""
            };
        }

        private static Dictionary<CameraAttribute, string> GetCoPick3D350Settings()
        {
            return new Dictionary<CameraAttribute, string>()
            {
                [CameraAttribute.Model] = CameraModel.CoPick3D_350.ToString(),
                [CameraAttribute.Serial] = "",
                [CameraAttribute.Ip] = "",
                [CameraAttribute.ScanMode] = CameraScanMode.MultiCamera.ToString(),
                [CameraAttribute.OutputResolution] = OutputResolution.W1224xH1024.ToString(),
                [CameraAttribute.IsolationDistance] = "1.0",
                [CameraAttribute.IsolationMinNeighbors] = "10",
                [CameraAttribute.SendNormalMap] = "False",
                [CameraAttribute.TextureExposureMultiplier] = "1",
                [CameraAttribute.TextureExposure1] = "16.0",
                [CameraAttribute.TextureExposure2] = "16.0",
                [CameraAttribute.TextureExposure3] = "16.0",
                [CameraAttribute.TextureGain1] = "5.0",
                [CameraAttribute.TextureGain2] = "5.0",
                [CameraAttribute.TextureGain3] = "5.0",
                [CameraAttribute.PatternExposureMultiplier] = "1",
                [CameraAttribute.PatternExposure1] = "10.0",
                [CameraAttribute.PatternExposure2] = "20.0",
                [CameraAttribute.PatternExposure3] = "30.0",
                [CameraAttribute.PatternGain1] = "3.0",
                [CameraAttribute.PatternGain2] = "3.0",
                [CameraAttribute.PatternGain3] = "3.0",
                [CameraAttribute.DecodeThreshold1] = "1",
                [CameraAttribute.DecodeThreshold2] = "1",
                [CameraAttribute.DecodeThreshold3] = "1",
                [CameraAttribute.NormalEstimationRadius] = "2.0",
                [CameraAttribute.SurfaceSmoothness] = SurfaceSmoothness.Sharp.ToString(),
                [CameraAttribute.StructurePatternType] = StructurePatternType.NormalAndInverted.ToString(),
                [CameraAttribute.LedPower] = "1",
                [CameraAttribute.PatternStrategy] = PatternStrategy.PhaseShiftDouble.ToString(),
                [CameraAttribute.PatternColor] = "3",
                [CameraAttribute.TextureSource] = "2",
                [CameraAttribute.MaxNomalAngle] = "90"
            };
        }

        private static Dictionary<CameraAttribute, string> GetCoPick3D250Settings()
        {
            return new Dictionary<CameraAttribute, string>()
            {
                [CameraAttribute.Model] = CameraModel.CoPick3D_250.ToString(),
                [CameraAttribute.Serial] = "",
                [CameraAttribute.Ip] = "",
                [CameraAttribute.ScanMode] = CameraScanMode.MultiCamera.ToString(),
                [CameraAttribute.OutputResolution] = OutputResolution.W1224xH1024.ToString(),
                [CameraAttribute.IsolationDistance] = "1.0",
                [CameraAttribute.IsolationMinNeighbors] = "10",
                [CameraAttribute.SendNormalMap] = "False",
                [CameraAttribute.TextureExposureMultiplier] = "1",
                [CameraAttribute.TextureExposure1] = "16.0",
                [CameraAttribute.TextureExposure2] = "16.0",
                [CameraAttribute.TextureExposure3] = "16.0",
                [CameraAttribute.TextureGain1] = "5.0",
                [CameraAttribute.TextureGain2] = "5.0",
                [CameraAttribute.TextureGain3] = "5.0",
                [CameraAttribute.PatternExposureMultiplier] = "1",
                [CameraAttribute.PatternExposure1] = "10.0",
                [CameraAttribute.PatternExposure2] = "20.0",
                [CameraAttribute.PatternExposure3] = "30.0",
                [CameraAttribute.PatternGain1] = "3.0",
                [CameraAttribute.PatternGain2] = "3.0",
                [CameraAttribute.PatternGain3] = "3.0",
                [CameraAttribute.DecodeThreshold1] = "1",
                [CameraAttribute.DecodeThreshold2] = "1",
                [CameraAttribute.DecodeThreshold3] = "1",
                [CameraAttribute.NormalEstimationRadius] = "2.0",
                [CameraAttribute.SurfaceSmoothness] = SurfaceSmoothness.Sharp.ToString(),
                [CameraAttribute.StructurePatternType] = StructurePatternType.NormalAndInverted.ToString(),
                [CameraAttribute.LedPower] = "1",
                [CameraAttribute.PatternStrategy] = PatternStrategy.PhaseShiftDouble.ToString(),
                [CameraAttribute.PatternColor] = PatternColor.Blue.ToString(),
                [CameraAttribute.TextureSource] = TextureSource.Led.ToString(),
                [CameraAttribute.MaxNomalAngle] = "90"
            };
        }
    }
}