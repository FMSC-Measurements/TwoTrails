using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using TwoTrails.Core.Points;

namespace TwoTrails.Core.ComponentModel.History
{
    public class CreateQuondamsCommand : ITtPointsCommand
    {
        private ITtManager pointsManager;
        private TtPolygon TargetPolygon;
        private int StartIndex;

        private EditTtPointsMultiValueCommand<int> editPointsCmd = null;
        private AddTtPointsCommand addPointsCmd;


        public CreateQuondamsCommand(IEnumerable<TtPoint> points, ITtManager pointsManager, TtPolygon targetPoly, int insertIndex, QuondamBoundaryMode bndMode = QuondamBoundaryMode.Inherit) : base(points)
        {
            this.pointsManager = pointsManager;

            TargetPolygon = targetPoly;

            List<TtPoint> polyPoints = pointsManager.GetPoints(TargetPolygon.CN);

            List<TtPoint> addPoints = new List<TtPoint>();
            List<TtPoint> editedPoints = new List<TtPoint>();
            List<int> editedIndexes = new List<int>();

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
                if (point.OnBoundary || bndMode != QuondamBoundaryMode.OBO)
                {
                    qp = new QuondamPoint()
                    {
                        PID = PointNamer.NamePoint(TargetPolygon, prevPoint),
                        Index = index++,
                        ParentPoint = point.OpType == OpType.Quondam ? ((QuondamPoint)point).ParentPoint : point,
                        Polygon = TargetPolygon,
                        Metadata = pointsManager.DefaultMetadata,
                        Group = pointsManager.MainGroup,
                        OnBoundary = (bndMode == QuondamBoundaryMode.Inherit) ? point.OnBoundary : (bndMode == QuondamBoundaryMode.On)
                    };

                    qp.SetAccuracy(TargetPolygon.Accuracy);

                    addPoints.Add(qp);

                    prevPoint = qp;
                }
            }

            if (StartIndex < polyPoints.Count)
            {
                foreach (TtPoint point in polyPoints.Skip(StartIndex))
                {
                    editedPoints.Add(point);
                    editedIndexes.Add(index++);
                }
            }

            addPointsCmd = new AddTtPointsCommand(addPoints, pointsManager);

            if (editedPoints.Count > 0)
                editPointsCmd = new EditTtPointsMultiValueCommand<int>(editedPoints, PointProperties.INDEX, editedIndexes);
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

    public enum QuondamBoundaryMode
    {
        [Description("(On Boundary Only) Quondams only created for points that are on the Boundary")]
        OBO = 0,
        [Description("Quondam inherits the parents OnBoundary")]
        Inherit = 1,
        [Description("Quondam will be On the Boundarys")]
        On = 2,
        [Description("Quondam will be Off the Boundary")]
        Off = 3
    }
}
