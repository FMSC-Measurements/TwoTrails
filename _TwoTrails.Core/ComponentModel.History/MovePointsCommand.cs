using System;
using System.Collections.Generic;
using System.Text;
using TwoTrails.Core.Points;

namespace TwoTrails.Core.ComponentModel.History
{
    public class MovePointsCommand : ITtPointsCommand
    {
        private ITtManager pointsManager;
        private TtPolygon TargetPolygon;
        private int InsertIndex;

        private List<int> NewIndexes = new List<int>();
        private List<int> OldIndexes = new List<int>();
        private List<TtPolygon> OldPolygons = new List<TtPolygon>();

        private List<TtPoint> EditedPoints = new List<TtPoint>();


        public MovePointsCommand(IEnumerable<TtPoint> points, ITtManager pointsManager, TtPolygon targetPoly, int insertIndex, bool autoCommit = true) : base(points)
        {
            this.pointsManager = pointsManager;

            TargetPolygon = targetPoly;
            InsertIndex = insertIndex;

            foreach (TtPoint point in Points)
            {
                OldIndexes.Add(point.Index);
                OldPolygons.Add(point.Polygon);
            }

            if (autoCommit)
                Redo();
        }

        public override void Redo()
        {
            pointsManager.MovePointsToPolygon(Points, TargetPolygon, InsertIndex);
        }

        public override void Undo()
        {
            for (int i = 0; i < Points.Count; i++)
            {
                TtPoint point = Points[i];
                point.Polygon = OldPolygons[i];
                point.Index = OldIndexes[i];
            }

            pointsManager.ReindexPolys();
        }
    }
}
