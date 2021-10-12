using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Linq;
using CommonUtils;

namespace FineLocalizer
{
    class GapChecker
    {
        private static readonly LogHelper Logger = LogHelper.Logger;

        private IntPtr _imgData = Marshal.AllocHGlobal(1024 * 1024 * 20);
        private Dictionary<RobotAttribute, string> _robotConf;
        public IRobotComm Robot { get; set; }

        public GapChecker(Dictionary<RobotAttribute, string> robotConf)
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

        public Task<bool> TriggerScanGapAsync(bool isRef, bool save)
        {
            return Task.Run(() =>
            {
                try
                {
                    return FineLocalizerVehicleEngineAPI.TriggerScanGap(isRef, save);
                }
                catch (AccessViolationException)
                {
                    return false;
                }
            });
        }

        public Task<(bool isSuccess,
                     int[] numsGapExamined,
                     double[] gapAverages,
                     List<Bitmap> resultImages)> MeasureGapAsync()
        {
            return Task.Run(() =>
            {
                int numGaps = 4;

                int[] numsGapExamined = new int[numGaps];
                double[] gapAverages = new double[numGaps];
                int[] resultImageLengths = new int[numGaps];

                var ret = FineLocalizerVehicleEngineAPI.MeasureGap(gapAverages, numsGapExamined,
                                                                   _imgData, resultImageLengths);

                List<Bitmap> resultImages = new List<Bitmap>();
                IntPtr pImg = _imgData;
                for (var i = 0; i < numGaps; ++i)
                {
                    byte[] bytes = new byte[resultImageLengths[i]];
                    Marshal.Copy(pImg, bytes, 0, resultImageLengths[i]);
                    using (var ms = new MemoryStream(bytes))
                    {
                        resultImages.Add(new Bitmap(ms));
                    }

                    pImg = IntPtr.Add(pImg, resultImageLengths[i]);
                }

                return (ret, numsGapExamined, gapAverages, resultImages);
            });
        }

        public Task<(bool isSuccess,
                     List<RobotPose> updatingPoses)> EstimateGapScanPoseAsync(int numUpdatingPoses)
        {
            return Task.Run(() =>
            {
                Pose7D[] poses = new Pose7D[numUpdatingPoses];
                var ret = FineLocalizerVehicleEngineAPI.EstimateGapScanPoses(poses);

                List<RobotPose> updatingPoses = null;
                if (ret)
                {
                    updatingPoses = poses.Select(p => new RobotPose(p)).ToList();
                }

                return (ret, updatingPoses);
            });
        }

        public async Task<(bool isSuccess, RobotPose shiftValue)> EstimateAndUpdateGapScanPosesAsync()
        {
            try
            {
                var gapScanPoseVars = _robotConf[RobotAttribute.GapScanPoseVars].Split(',');

                for (var i = 0; i < gapScanPoseVars.Length; ++i)
                {
                    var pose = await Robot.ReadRobotPoseAsync(gapScanPoseVars[i]);
                    if (pose == null)
                    {
                        Logger.Warning($"{Lang.LogsFineLo.GapScanPoseReadError} (point {i + 1})");
                        return (false, null);
                    }

                    if (!FineLocalizerVehicleEngineAPI.InfoGapScanPose(i + 1, new Pose7D(pose), true))
                    {
                        Logger.Warning($"{Lang.LogsFineLo.GapScanPoseTransportationError} (point {i + 1})");
                        return (false, null);
                    }
                }

                Logger.Info($"{Lang.LogsFineLo.GapScanPoseReadCompleted} (#poses={gapScanPoseVars.Length})");

                var retGap = await EstimateGapScanPoseAsync(gapScanPoseVars.Length);

                if (!retGap.isSuccess)
                {
                    Logger.Error(Lang.LogsFineLo.EstimateGapScanPoseAsyncFailed);
                    return (false, null);
                }

                Logger.Info(Lang.LogsFineLo.GapScanPoseCalculated);

                var varsToUpdate = _robotConf[RobotAttribute.GapScanPoseShiftVars].Split(',');
                var nUpdates = Math.Min(varsToUpdate.Length, retGap.updatingPoses.Count);

                for (var i = 0; i < nUpdates; ++i)
                {
                    if (!await Robot.WriteRobotPoseAsync(retGap.updatingPoses[i], varsToUpdate[i]))
                    {
                        Logger.Error($"{Lang.LogsFineLo.GapScanShiftValueWriteFailed} ({varsToUpdate[i]})");
                        return (false, null);
                    }
                }

                Logger.Info($"{Lang.LogsFineLo.GapScanPoseTransportationCompleted} (#poses={nUpdates})");

                return (true, retGap.updatingPoses[0]);
            }
            catch (Exception ex)
            {
                Logger.Error($"{Lang.LogsFineLo.GapScanPoseCalculationError} ({ex})");
                return (false, null);
            }
        }
    }
}
