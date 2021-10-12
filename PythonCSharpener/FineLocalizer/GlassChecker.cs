using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Linq;
using CommonUtils;

namespace FineLocalizer
{
    public class GlassChecker
    {
        private static readonly LogHelper Logger = LogHelper.Logger;

        private IntPtr _imgData = Marshal.AllocHGlobal(1024 * 1024 * 10);
        private Dictionary<RobotAttribute, string> _robotConf;
        public IRobotComm Robot { get; set; }

        public GlassChecker(Dictionary<RobotAttribute, string> robotConf)
        {
            ConfigureRobot(robotConf, true);
        }

        public void ConfigureRobot(Dictionary<RobotAttribute, string> robotConf, bool remake = false)
        {
            if (remake)
            {
                Robot?.Dispose();
                var robotMaker = robotConf[RobotAttribute.Maker].ToEnum<RobotMaker>();
                Robot = RobotCommMaker.Make(robotMaker, robotConf);
                if (Robot == null)
                {
                    Logger.Error(Lang.LogsFineLo.NotSupportedRobot);
                }
            }

            _robotConf = robotConf;
        }

        public Task<bool> TriggerScanGlassAsync(bool isRef, bool save)
        {
            return Task.Run(() =>
            {
                var robotMaker = _robotConf[RobotAttribute.Maker].ToEnum<RobotMaker>();
                var glassGripPoseVars = _robotConf[RobotAttribute.GlassGripPoseVars].Split(',');
                var glassGripPoses = glassGripPoseVars.Select(async p => await Robot.ReadRobotPoseAsync(p))
                                                      .Select(p => p.Result).ToList();
                if (glassGripPoses.Any(p => p == null))
                {
                    Logger.Warning(Lang.LogsFineLo.GlassGripPoseReadFailed);
                }

                if (!InfoGlassGripPosesToEngine(glassGripPoses, false))
                {
                    Logger.Warning(Lang.LogsFineLo.GlassGripPoseTransportFailed);
                }

                Logger.Info(Lang.LogsFineLo.GlassGripPoseReadSuccess);

                try
                {
                    return FineLocalizerVehicleEngineAPI.TriggerScanGlass(isRef, save);
                }
                catch (AccessViolationException)
                {
                    return false;
                }
            });
        }

        public bool InfoGlassGripPosesToEngine(List<RobotPose> poses, bool isRef)
        {
            return FineLocalizerVehicleEngineAPI.InfoGlassGripPoses(poses.Select(p => new Pose7D(p)).ToArray(), isRef);
        }

        public bool InfoGlassScanPoseToEngine(RobotPose pose)
        {
            return FineLocalizerVehicleEngineAPI.InfoGlassScanPose(1, new Pose7D(pose), false);
        }

        public bool InfoGlassScanRefPoseToEngine(RobotPose pose)
        {
            return FineLocalizerVehicleEngineAPI.InfoGlassScanPose(1, new Pose7D(pose), true);
        }

        public Task<(bool isSuccess,
                     List<RobotPose> updatingPoses,
                     PointNormal[] target1,
                     PointNormal[] source1,
                     PointNormal[] sourceAligned1,
                     PointNormal[] target2, 
                     PointNormal[] source2,
                     PointNormal[] sourceAligned2,
                     float zMin,
                     float zMax,
                     float zAvg)> EstimateGlassGripPoseAsync()
        {
            return Task.Run(() =>
            {
                var robotMaker = _robotConf[RobotAttribute.Maker].ToEnum<RobotMaker>();
                var glassGripPoseVars = _robotConf[RobotAttribute.GlassGripPoseVars].Split(',');
                var glassGripPoses = glassGripPoseVars.Select(async p => await Robot.ReadRobotPoseAsync(p))
                                                      .Select(p => p.Result).ToList();
                if (glassGripPoses.Any(p => p == null))
                {
                    Logger.Warning(Lang.LogsFineLo.GlassGripPoseReadFailed);
                    return (false, null, null, null, null, null, null, null, 0, 0, 0);
                }

                if (!InfoGlassGripPosesToEngine(glassGripPoses, false))
                {
                    Logger.Warning(Lang.LogsFineLo.GlassGripPoseTransportFailed);
                    return (false, null, null, null, null, null, null, null, 0, 0, 0);
                }

                Logger.Info(Lang.LogsFineLo.GlassGripPoseReadSuccess);

                
                Pose7D[] pose7Ds = new Pose7D[glassGripPoses.Count];
                var ret = FineLocalizerVehicleEngineAPI.EstimateGlassGripPose(pose7Ds, out var pTarget1, out var pSource1, out var pSourceAligned1,
                                                                              out var pTarget2, out var pSource2, out var pSourceAligned2,
                                                                              out var xAvg1, out var yAvg1, out var xAvg2, out var yAvg2);

                float zMin = 0;
                float zMax = 600;
                float zAvg = 300;

                var target1 = new PointNormalVectorWrapper(pTarget1,
                                                           FineLocalizerVehicleEngineAPI.GetPointNormalVectorLength,
                                                           FineLocalizerVehicleEngineAPI.GetPointNormalVectorValue).CopyToArray(xAvg1, yAvg1, zAvg, zMax);
                var source1 = new PointNormalVectorWrapper(pSource1,
                                                           FineLocalizerVehicleEngineAPI.GetPointNormalVectorLength,
                                                           FineLocalizerVehicleEngineAPI.GetPointNormalVectorValue).CopyToArray(xAvg1, yAvg1, zAvg, zMax);
                var sourceAligned1 = new PointNormalVectorWrapper(pSourceAligned1,
                                                                  FineLocalizerVehicleEngineAPI.GetPointNormalVectorLength,
                                                                  FineLocalizerVehicleEngineAPI.GetPointNormalVectorValue).CopyToArray(xAvg1, yAvg1, zAvg, zMax);
                var target2 = new PointNormalVectorWrapper(pTarget2,
                                                           FineLocalizerVehicleEngineAPI.GetPointNormalVectorLength,
                                                           FineLocalizerVehicleEngineAPI.GetPointNormalVectorValue).CopyToArray(xAvg2, yAvg2, zAvg, zMax);
                var source2 = new PointNormalVectorWrapper(pSource2,
                                                           FineLocalizerVehicleEngineAPI.GetPointNormalVectorLength,
                                                           FineLocalizerVehicleEngineAPI.GetPointNormalVectorValue).CopyToArray(xAvg2, yAvg2, zAvg, zMax);
                var sourceAligned2 = new PointNormalVectorWrapper(pSourceAligned2,
                                                                  FineLocalizerVehicleEngineAPI.GetPointNormalVectorLength,
                                                                  FineLocalizerVehicleEngineAPI.GetPointNormalVectorValue).CopyToArray(xAvg2, yAvg2, zAvg, zMax);

                List<RobotPose> poses = pose7Ds.Select(p => new RobotPose(p)).ToList();

                if (ret)
                {
                    Logger.Info(Lang.LogsFineLo.GlassGripPoseShiftValueCalculated);
                }

                return (ret, poses, target1, source1, sourceAligned1, target2, source2, sourceAligned2, zMin, zMax, zAvg);
            });
        }

        public async Task<bool> UpdateGlassGripPoseAsync(List<RobotPose> shiftValues)
        {
            var varsToUpdate = _robotConf[RobotAttribute.GlassGripPoseShiftVars].Split(',');
            var nUpdates = Math.Min(varsToUpdate.Length, shiftValues.Count);

            for (var i = 0; i < nUpdates; ++i)
            {
                if (!await Robot.WriteRobotPoseAsync(shiftValues[i], varsToUpdate[i]))
                {
                    Logger.Info($"{Lang.LogsFineLo.GlassGripPoseShiftValueWriteFailed} ({varsToUpdate[i]})");
                    return false;
                }

                Logger.Info($"{Lang.LogsFineLo.GlassGripPoseShiftValueWriteCompleted} ({varsToUpdate[i]})");
            }

            return true;
        }
    }
}
