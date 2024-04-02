using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using TwoTrails.Core;
using TwoTrails.Core.Points;
using TwoTrails.DAL;

namespace TwoTrails.Utils
{
    public class DataHelper
    {
        public static DataErrors Analyze(ITtManager manager)
        {
            DataErrors errors = DataErrors.None;

            if (AnalyzeMiszonedPoints(manager))
                errors |= DataErrors.MiszonedPoints;

            if (AnalyzeOrphanedQuondams(manager))
                errors |= DataErrors.OrphanedQuondams;

            if (AnalyzeChildlessPoints(manager))
                errors |= DataErrors.MissingChildren;

            if (AnalyzeEmptyPolygons(manager))
                errors |= DataErrors.EmptyPolygons;

            if (AnalyzeUnusedMetadata(manager))
                errors |= DataErrors.UnusedMetadata;

            if (AnalyzeUnusedGroups(manager))
                errors |= DataErrors.UnusedGroups;

            if (AnalyzeDuplicateMetadata(manager))
                errors |= DataErrors.DuplicateMetadata;

            return errors;
        }

        public static bool AnalyzeMiszonedPoints(ITtManager manager)
        {
            return manager.GetPoints().Any(p => p.IsGpsType() && p.IsMiszoned());
        }

        public static bool AnalyzeOrphanedQuondams(ITtManager manager)
        {
            Dictionary<string, TtPoint> points = manager.GetPoints().ToDictionary(p => p.CN, p => p);

            return points.Values.Where(p => p.OpType == OpType.Quondam)
                .Any(point => point is QuondamPoint qp && (qp.ParentPoint == null || !points.ContainsKey(qp.ParentPointCN)));
        }

        public static bool AnalyzeChildlessPoints(ITtManager manager)
        {
            Dictionary<string, TtPoint> points = manager.GetPoints().ToDictionary(p => p.CN, p => p);

            return points.Values.Any(p => p.HasQuondamLinks && p.LinkedPoints.Any(lpk => !points.ContainsKey(lpk)));
        }

        public static bool HasUnsetPolygonAccuracies(ITtManager manager)
        {
            return manager.GetPolygons().Any(p => p.Accuracy == Consts.DEFAULT_POINT_ACCURACY);
        }

        public static bool AnalyzeEmptyPolygons(ITtManager manager)
        {
            return manager.GetPolygons().Any(poly => !manager.GetPoints().Any(p => p.PolygonCN == poly.CN));
        }

        public static bool AnalyzeUnusedMetadata(ITtManager manager)
        {
            return manager.GetMetadata().Any(meta => !manager.GetPoints().Any(p => p.MetadataCN == meta.CN));
        }

        public static bool AnalyzeUnusedGroups(ITtManager manager)
        {
            return manager.GetGroups().Any(group => !manager.GetPoints().Any(g => g.GroupCN == group.CN));
        }

        public static bool AnalyzeDuplicateMetadata(ITtManager manager)
        {
            return manager.GetMetadata()
                .GroupBy(meta =>
                    new { meta.Zone, meta.Comment, meta.Compass, meta.Crew, meta.Datum, meta.DecType, meta.Distance, meta.Elevation, meta.GpsReceiver, meta.MagDec, meta.RangeFinder, meta.Slope })
                    .Any(group => group.Count() > 1);
        }


        public static bool CheckAndFixErrors(ITtDataLayer dal, ITtSettings settings)
        {
            DalError errors = dal.GetErrors();

            if (errors.HasFlag(DalError.CriticalIssue))
            {
                MessageBox.Show("It appears that the TwoTrails file is physically corrupted beyond the repair capabilities of the the program, possibly due to a hardware or operating system error. " +
                   "If the file is from a mobile device try copyping it over to the computer again. Please contact the development team for support for further assistance.",
                   "File is physically corrupted ", MessageBoxButton.OK, MessageBoxImage.Error);

                return false;
            }
            else if (errors.HasFlag(DalError.CorruptDatabase))
            {
                MessageBox.Show("It appears that the TwoTrails file is physically corrupted, possibly due to a hardware or operating system error. " +
                   "If the file is from a mobile device try copyping it over to the computer again. Please contact the development team for support for further assistance.",
                   "File is physically corrupted", MessageBoxButton.OK, MessageBoxImage.Error);

                return false;
            }
            else if (errors > 0)
            {
                List<string> errList = new List<string>();

                if (errors.HasFlag(DalError.OrphanedQuondams))
                    errList.Add("Orphaned Quondams");
                if (errors.HasFlag(DalError.MissingChildren))
                    errList.Add("Points with Missing Children");
                if (errors.HasFlag(DalError.PointIndexes))
                    errList.Add("Skipped Point Indexes");
                if (errors.HasFlag(DalError.NullAdjLocs))
                    errList.Add("Invalid Point Locations");
                if (errors.HasFlag(DalError.MissingPolygon))
                    errList.Add("Missing Polygons");
                if (errors.HasFlag(DalError.MissingMetadata))
                    errList.Add("Missing Metadata");
                if (errors.HasFlag(DalError.MissingGroup))
                    errList.Add("Missing Groups");

                bool hardErrors =
                    errors.HasFlag(DalError.OrphanedQuondams) || errors.HasFlag(DalError.MissingChildren) ||
                    errors.HasFlag(DalError.MissingGroup) || errors.HasFlag(DalError.MissingMetadata) ||
                    errors.HasFlag(DalError.MissingPolygon);

                MessageBoxResult mbr = MessageBox.Show(
                    $"It appears part of the TwoTrails data {(hardErrors ? "is corrupt" : "needs adjusting")}. The error{(errList.Count > 1 ? "s include" : " is")}: {String.Join(" | ", errList)}. " +
                    (hardErrors ? $"Would you like to try and fix the data by recreating, moving and modifing (Yes) or removing invalid (No)?" :
                    "Would you like to fix the data?"),
                    hardErrors ? "Data is corrupted" : "Data needs adjusting",
                    hardErrors ? MessageBoxButton.YesNoCancel : MessageBoxButton.OKCancel, MessageBoxImage.Error, MessageBoxResult.Cancel);

                if (mbr == MessageBoxResult.Cancel)
                    return false;
                else
                {
                    try
                    {
                        File.Copy(dal.FilePath, $"{dal.FilePath}.corrupt", true);

                        TtUserAction action = new TtUserAction("Fixer", settings.DeviceName, $"PC: {settings.AppVersion}", DateTime.Now, DataActionType.None, String.Join(", ", errList));

                        if (mbr == MessageBoxResult.No)
                        {
                            dal.FixErrors(true);

                            action.UpdateAction(DataActionType.DeletedPoints);
                            action.UpdateAction(DataActionType.ReindexPoints);
                        }
                        else
                        {
                            dal.FixErrors(false);

                            if (errors.HasFlag(DalError.MissingPolygon))
                                action.UpdateAction(DataActionType.InsertedPolygons);
                            if (errors.HasFlag(DalError.OrphanedQuondams) || errors.HasFlag(DalError.MissingMetadata) || errors.HasFlag(DalError.MissingGroup))
                                action.UpdateAction(DataActionType.ModifiedPoints);
                        }

                        if (errors.HasFlag(DalError.NullAdjLocs) || errors.HasFlag(DalError.MissingChildren))
                            action.UpdateAction(DataActionType.ModifiedPoints);

                        if (errors.HasFlag(DalError.PointIndexes))
                            action.UpdateAction(DataActionType.ReindexPoints);

                        dal.InsertActivity(action);
                    }
                    catch (Exception e)
                    {
                        Trace.WriteLine(e.Message);
                        MessageBox.Show("Error fixing file.");
                        return false;
                    }
                }
            }

            return true;
        }

        public static bool IsEmptyFile(string path)
        {
            return new FileInfo(path).Length == 0;
        }
    }

    [Flags]
    public enum DataErrors
    {
        None = 0,
        MiszonedPoints = 1 << 0,
        OrphanedQuondams = 1 << 1,
        MissingChildren = 1 << 2,
        EmptyPolygons = 1 << 3,
        UnusedMetadata = 1 << 4,
        UnusedGroups = 1 << 5,
        DuplicateMetadata = 1 << 6
    }
}
