using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TwoTrails.Core.Points;

namespace TwoTrails.Core.ComponentModel.History
{
    public class MovePointsCommand : ITtPointsCommand
    {
        private ITtManager pointsManager;
        private TtPolygon TargetPolygon;
        private int InsertIndex;

        private Dictionary<String, int> NewIndexes = new Dictionary<String, int>();
        private Dictionary<String, int> OldIndexes = new Dictionary<String, int>();
        private Dictionary<String, TtPolygon> OldPolygons = new Dictionary<String, TtPolygon>();
        private Dictionary<String, TtPoint> EditedPoints = new Dictionary<String, TtPoint>();
        private Dictionary<String, TtPolygon> PolygonsToBuild = new Dictionary<String, TtPolygon>();

        public MovePointsCommand(IEnumerable<TtPoint> points, ITtManager pointsManager, TtPolygon targetPoly, int insertIndex, bool autoCommit = true) : base(points)
        {
            this.pointsManager = pointsManager;

            TargetPolygon = targetPoly;
            InsertIndex = insertIndex;

            PolygonsToBuild.Add(TargetPolygon.CN, TargetPolygon);

            foreach (TtPoint point in Points)
            {
                EditedPoints.Add(point.CN, point);
                OldIndexes.Add(point.CN, point.Index);
                OldPolygons.Add(point.CN, point.Polygon);

                if (!PolygonsToBuild.ContainsKey(point.PolygonCN))
                    PolygonsToBuild.Add(point.PolygonCN, point.Polygon);
            }

            foreach (TtPolygon poly in PolygonsToBuild.Values)
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

            if (autoCommit)
                Redo();
        }

        public override void Redo()
        {
            pointsManager.MovePointsToPolygon(Points, TargetPolygon, InsertIndex);

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
                point.Polygon = OldPolygons[point.CN];
            }

            foreach (TtPolygon poly in PolygonsToBuild.Values)
                pointsManager.RebuildPolygon(poly);
        }
    }
}
