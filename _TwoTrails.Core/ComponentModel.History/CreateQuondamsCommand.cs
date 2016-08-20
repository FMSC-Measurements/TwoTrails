using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TwoTrails.Core.Points;

namespace TwoTrails.Core.ComponentModel.History
{
    public class CreateQuondamsCommand : ITtPointsCommand
    {
        private ITtManager pointsManager;
        private TtPolygon TargetPolygon;
        private int StartIndex;

        private EditTtPointsMultiValueCommand editPointsCmd = null;
        private AddTtPointsCommand addPointsCmd;


        public CreateQuondamsCommand(IEnumerable<TtPoint> points, ITtManager pointsManager, TtPolygon targetPoly, int insertIndex, bool rev = false, bool autoCommit = true) : base(points)
        {
            this.pointsManager = pointsManager;

            TargetPolygon = targetPoly;

            List<TtPoint> polyPoints = pointsManager.GetPoints(TargetPolygon.CN);

            List<TtPoint> addPoints = new List<TtPoint>();
            List<TtPoint> editedPoints = new List<TtPoint>();
            List<object> editedIndexes = new List<object>();

            int index = insertIndex > polyPoints.Count ?
                polyPoints.Count : 
                insertIndex < 0 ? 0 : insertIndex;
            StartIndex = index;

            TtPoint prevPoint = null;
            QuondamPoint qp;

            if (StartIndex > 0)
                prevPoint = polyPoints[StartIndex - 1];

            foreach (TtPoint point in Points)
            {
                qp = new QuondamPoint()
                {
                    PID = PointNamer.NamePoint(TargetPolygon, prevPoint),
                    Index = index++,
                    ParentPoint = point,
                    Polygon = TargetPolygon,
                    Metadata = pointsManager.DefaultMetadata,
                    Group = pointsManager.MainGroup
                };

                addPoints.Add(qp);

                prevPoint = qp;
            }

            if (StartIndex < polyPoints.Count)
            {
                foreach (TtPoint point in polyPoints.Skip(StartIndex))
                {
                    editedPoints.Add(point);
                    editedIndexes.Add(index++);
                }
            }

            addPointsCmd = new AddTtPointsCommand(addPoints, pointsManager, false);

            if (editedPoints.Count > 0)
                editPointsCmd = new EditTtPointsMultiValueCommand(editedPoints, PointProperties.INDEX, editedIndexes, false);

            if (autoCommit)
                Redo();
        }

        public override void Redo()
        {
            if (editPointsCmd != null)
                editPointsCmd.Redo();

            addPointsCmd.Redo();
        }

        public override void Undo()
        {
            if (editPointsCmd != null)
                editPointsCmd.Undo();

            addPointsCmd.Undo();
        }
    }
}
