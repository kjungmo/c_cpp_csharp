using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using CommonUtils;

namespace FineLocalizer
{
    class FineLocalizerVehicle
    {
        private static readonly LogHelper Logger = LogHelper.Logger;

        private Dictionary<RobotAttribute, string> _installRobotConf;
        private Dictionary<RobotAttribute, string> _scanRobotConf;
        public IRobotComm InstallRobot { get; set; }
        public IRobotComm ScanRobot { get; set; }

        public FineLocalizerVehicle(Dictionary<RobotAttribute, string> installRobotConf,
                                    Dictionary<RobotAttribute, string> scanRobotConf)
        {
            ConfigureInstallRobot(installRobotConf, true);
            ConfigureScanRobot(scanRobotConf, true);
        }

        public void ConfigureInstallRobot(Dictionary<RobotAttribute, string> installRobotConf, bool remake = false)
        {
            if (remake)
            {
                InstallRobot?.Dispose();
                var installRobotMaker = installRobotConf[RobotAttribute.Maker].ToEnum<RobotMaker>();
                InstallRobot = RobotCommMaker.Make(installRobotMaker, installRobotConf);
                if (InstallRobot == null)
                {
                    Logger.Error(Lang.LogsFineLo.NotSupportedRobot);
                }
            }

            _installRobotConf = installRobotConf;
        }

        public void ConfigureScanRobot(Dictionary<RobotAttribute, string> scanRobotConf, bool remake = false)
        {
            if (remake)
            {
                ScanRobot?.Dispose();
                var scanRobotMaker = scanRobotConf[RobotAttribute.Maker].ToEnum<RobotMaker>();
                ScanRobot = RobotCommMaker.Make(scanRobotMaker, scanRobotConf);
                if (ScanRobot == null)
                {
                    Logger.Error(Lang.LogsFineLo.NotSupportedRobot);
                }
            }

            _scanRobotConf = scanRobotConf;
        }

        public async Task<bool> ReadyVehicleCheckerAsync()
        {
            var installVars = _installRobotConf[RobotAttribute.VehicleInstallVars].Split(',').ToList();
            int numUpdatingPoses = installVars.Count;
            List<RobotPose> poses = new List<RobotPose>(numUpdatingPoses);
            foreach (var installVar in installVars)
            {
                var pose = await InstallRobot.ReadRobotPoseAsync(installVar);
                if (pose == null)
                {
                    Logger.Error(Lang.LogsFineLo.ReadingPoseFailed);
                    return false;
                }
                poses.Add(pose);
            }

            InfoInstallPoses(poses);

            return FineLocalizerVehicleEngineAPI.ReadyVehicleCheck();
        }

        public async Task<(bool isOK, List<RobotPose> calculatedPoses,
                           PointNormal[] target1,
                           PointNormal[] source1,
                           PointNormal[] sourceAligned1,
                           PointNormal[] target2,
                           PointNormal[] source2,
                           PointNormal[] sourceAligned2)> CalculatePosesAsync(float xAvg, float yAvg, float zAvg, float zMax)
        {
            return await Task.Run(() =>
            {
                int numPoses = _installRobotConf[RobotAttribute.VehicleInstallVars].Split(',').Count();
                Pose7D[] poses = new Pose7D[numPoses];

                bool isOk = FineLocalizerVehicleEngineAPI.EstimateVehicleShift(poses, out var pTarget1, out var pSource1, out var pSourceAligned1,
                                                                               out var pTarget2, out var pSource2, out var pSourceAligned2);
                var calPoses = poses.Select(p => new RobotPose(p)).ToList();

                var target1 = new PointNormalVectorWrapper(pTarget1,
                                                           FineLocalizerVehicleEngineAPI.GetPointNormalVectorLength,
                                                           FineLocalizerVehicleEngineAPI.GetPointNormalVectorValue).CopyToArray(xAvg, yAvg, zAvg, zMax);
                var source1 = new PointNormalVectorWrapper(pSource1,
                                                           FineLocalizerVehicleEngineAPI.GetPointNormalVectorLength,
                                                           FineLocalizerVehicleEngineAPI.GetPointNormalVectorValue).CopyToArray(xAvg, yAvg, zAvg, zMax);
                var sourceAligned1 = new PointNormalVectorWrapper(pSourceAligned1,
                                                                  FineLocalizerVehicleEngineAPI.GetPointNormalVectorLength,
                                                                  FineLocalizerVehicleEngineAPI.GetPointNormalVectorValue).CopyToArray(xAvg, yAvg, zAvg, zMax);

                var target2 = new PointNormalVectorWrapper(pTarget2,
                                                           FineLocalizerVehicleEngineAPI.GetPointNormalVectorLength,
                                                           FineLocalizerVehicleEngineAPI.GetPointNormalVectorValue).CopyToArray(xAvg, yAvg, zAvg, zMax);
                var source2 = new PointNormalVectorWrapper(pSource2,
                                                           FineLocalizerVehicleEngineAPI.GetPointNormalVectorLength,
                                                           FineLocalizerVehicleEngineAPI.GetPointNormalVectorValue).CopyToArray(xAvg, yAvg, zAvg, zMax);
                var sourceAligned2 = new PointNormalVectorWrapper(pSourceAligned2,
                                                                  FineLocalizerVehicleEngineAPI.GetPointNormalVectorLength,
                                                                  FineLocalizerVehicleEngineAPI.GetPointNormalVectorValue).CopyToArray(xAvg, yAvg, zAvg, zMax);

                return (isOk, calPoses, target1, source1, sourceAligned1, target2, source2, sourceAligned2);
            });
        }

        public async Task<bool> UpdateInstallPosesToRobot(List<RobotPose> installPoses)
        {
            try
            {
                var varNames = _installRobotConf[RobotAttribute.VehicleShiftVars].Split(',');
                for (var i = 0; i < installPoses.Count; ++i)
                {
                    Logger.Debug($"{Lang.LogsFineLo.InfoUpdatingInstallPose} ([{i}] {installPoses[i]} -> {varNames[i]})");
                    var ret = await InstallRobot.WriteRobotPoseAsync(installPoses[i], varNames[i]);

                    if (_installRobotConf[RobotAttribute.Maker].ToEnum<RobotMaker>() == RobotMaker.KAWASAKI)
                    {
                        var retPendent = await (InstallRobot as KawaComm).WriteRobotPoseVariableToPendantAsync(varNames[i]);
                    }

                    if (!ret) return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                Logger.Debug($"{Lang.LogsFineLo.UpdateInstallPosesToRobotError} ({ex})");
                return false;
            }
        }

        public bool InfoInstallPoses(List<RobotPose> installPoses)
        {
            var arrPoses = installPoses.Select(p => new Pose7D(p)).ToArray();
            return FineLocalizerVehicleEngineAPI.InfoInstallPoses(arrPoses);
        }

        public bool InfoVehicleScanPoseToEngine(int pointNum, RobotPose pose)
        {
            return FineLocalizerVehicleEngineAPI.InfoVehicleScanPose(pointNum, new Pose7D(pose), false);
        }

        public bool InfoVehicleScanRefPoseToEngine(int pointNum, RobotPose pose)
        {
            return FineLocalizerVehicleEngineAPI.InfoVehicleScanPose(pointNum, new Pose7D(pose), true);
        }

        public Task<bool> TriggerScanVehicleAsync(int pointNum, bool isRef, bool save)
        {
            return Task.Run(() =>
            {
                try
                {
                    return FineLocalizerVehicleEngineAPI.TriggerScanVehicle(pointNum, isRef, save);
                }
                catch (AccessViolationException)
                {
                    return false;
                }
            });
        }
    }
}
