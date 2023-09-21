using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using TwoTrails.Core.Points;
using TwoTrails.Core.Units;

namespace TwoTrails.Core.ComponentModel.History
{
    public class CreateQuondamsCommand : ITtPointsCommand
    {
        private TtUnit TargetUnit;
        private int StartIndex;

        private EditTtPointsMultiValueCommand<int> editPointsCmd = null;
        private AddTtPointsCommand addPointsCmd;

        public CreateQuondamsCommand(IEnumerable<TtPoint> points, TtManager pointsManager, TtUnit targetUnit, int insertIndex, QuondamBoundaryMode bndMode = QuondamBoundaryMode.Inherit) : base(points)
        {
            TargetUnit = targetUnit;

            List<TtPoint> unitPoints = pointsManager.GetPoints(TargetUnit.CN);

            List<TtPoint> addPoints = new List<TtPoint>();
            List<TtPoint> editedPoints = new List<TtPoint>();
            List<int> editedIndexes = new List<int>();

            int index = insertIndex > unitPoints.Count ?
                unitPoints.Count : 
                insertIndex < 0 ? 0 : insertIndex;
            StartIndex = index;

            TtPoint prevPoint = null;
            QuondamPoint qp;

            if (StartIndex > 0)
                prevPoint = unitPoints[StartIndex - 1];

            foreach (TtPoint point in Points)
            {
                if (point.OnBoundary || bndMode != QuondamBoundaryMode.OBO)
                {
                    qp = new QuondamPoint()
                    {
                        PID = PointNamer.NamePoint(TargetUnit, prevPoint),
                        Index = index++,
                        ParentPoint = point.OpType == OpType.Quondam ? ((QuondamPoint)point).ParentPoint : point,
                        Unit = TargetUnit,
                        Metadata = point.Metadata,
                        Group = pointsManager.MainGroup,
                        OnBoundary = (bndMode == QuondamBoundaryMode.Inherit) ? point.OnBoundary : (bndMode != QuondamBoundaryMode.Off)
                    };

                    qp.SetAccuracy(TargetUnit.Accuracy);

                    addPoints.Add(qp);

                    prevPoint = qp;
                }
            }

            if (StartIndex < unitPoints.Count)
            {
                foreach (TtPoint point in unitPoints.Skip(StartIndex))
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
