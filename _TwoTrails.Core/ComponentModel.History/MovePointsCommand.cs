using System;
using System.Collections.Generic;
using TwoTrails.Core.Points;
using TwoTrails.Core.Units;

namespace TwoTrails.Core.ComponentModel.History
{
    public class MovePointsCommand : ITtPointsCommand
    {
        private TtManager pointsManager;
        private TtUnit TargetUnit;
        private int InsertIndex;

        private Dictionary<String, int> NewIndexes = new Dictionary<String, int>();
        private Dictionary<String, int> OldIndexes = new Dictionary<String, int>();
        private Dictionary<String, TtUnit> OldUnits = new Dictionary<String, TtUnit>();
        private Dictionary<String, TtPoint> EditedPoints = new Dictionary<String, TtPoint>();
        private Dictionary<String, TtUnit> UnitsToBuild = new Dictionary<String, TtUnit>();

        public MovePointsCommand(IEnumerable<TtPoint> points, TtManager pointsManager, TtUnit targetUnit, int insertIndex) : base(points)
        {
            this.pointsManager = pointsManager;

            TargetUnit = targetUnit;
            InsertIndex = insertIndex;

            UnitsToBuild.Add(TargetUnit.CN, TargetUnit);

            foreach (TtPoint point in Points)
            {
                EditedPoints.Add(point.CN, point);
                OldIndexes.Add(point.CN, point.Index);
                OldUnits.Add(point.CN, point.Unit);

                if (!UnitsToBuild.ContainsKey(point.UnitCN))
                    UnitsToBuild.Add(point.UnitCN, point.Unit);
            }

            foreach (TtUnit poly in UnitsToBuild.Values)
            {
                foreach (TtPoint point in pointsManager.GetPoints(poly.CN))
                {
                    if (!EditedPoints.ContainsKey(point.CN))
                    {
                        EditedPoints.Add(point.CN, point);
                        OldIndexes.Add(point.CN, point.Index);
                    }
                }
            }
        }

        public override void Redo()
        {
            pointsManager.MovePointsToUnit(Points, TargetUnit, InsertIndex);

            NewIndexes.Clear();
            foreach (TtPoint point in EditedPoints.Values)
            {
                NewIndexes.Add(point.CN, point.Index);
            }
        }

        public override void Undo()
        {
            foreach (TtPoint point in EditedPoints.Values)
            {
                point.Index = OldIndexes[point.CN];
            }

            foreach (TtPoint point in Points)
            {
                point.Unit = OldUnits[point.CN];
            }

            foreach (TtUnit poly in UnitsToBuild.Values)
                pointsManager.RebuildUnit(poly);
        }
    }
}
