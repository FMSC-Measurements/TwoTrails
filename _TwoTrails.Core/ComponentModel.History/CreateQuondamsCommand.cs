using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using TwoTrails.Core.Points;

namespace TwoTrails.Core.ComponentModel.History
{
    public class CreateQuondamsCommand : ITtPointsCommand
    {
        private TtPolygon _TargetPolygon;
        private int _StartIndex;

        private EditTtPointsMultiValueCommand<int> _EditPointsCommand = null;
        private AddTtPointsCommand _AddPointsCommand;

        public CreateQuondamsCommand(IEnumerable<TtPoint> points, TtManager pointsManager, TtPolygon targetPoly, int insertIndex, QuondamBoundaryMode bndMode = QuondamBoundaryMode.Inherit) : base(points)
        {
            _TargetPolygon = targetPoly;

            List<TtPoint> polyPoints = pointsManager.GetPoints(_TargetPolygon.CN);

            List<TtPoint> addPoints = new List<TtPoint>();
            List<TtPoint> editedPoints = new List<TtPoint>();
            List<int> editedIndexes = new List<int>();

            int index = insertIndex > polyPoints.Count ?
                polyPoints.Count : 
                insertIndex < 0 ? 0 : insertIndex;
            _StartIndex = index;

            TtPoint prevPoint = null;
            QuondamPoint qp;

            if (_StartIndex > 0)
                prevPoint = polyPoints[_StartIndex - 1];

            foreach (TtPoint point in Points)
            {
                if (point.OnBoundary || bndMode != QuondamBoundaryMode.OBO)
                {
                    qp = new QuondamPoint()
                    {
                        PID = PointNamer.NamePoint(_TargetPolygon, prevPoint),
                        Index = index++,
                        ParentPoint = point.OpType == OpType.Quondam ? ((QuondamPoint)point).ParentPoint : point,
                        Polygon = _TargetPolygon,
                        Metadata = point.Metadata,
                        Group = pointsManager.MainGroup,
                        OnBoundary = (bndMode == QuondamBoundaryMode.Inherit) ? point.OnBoundary : (bndMode != QuondamBoundaryMode.Off)
                    };

                    qp.SetAccuracy(_TargetPolygon.Accuracy);

                    addPoints.Add(qp);

                    prevPoint = qp;
                }
            }

            if (_StartIndex < polyPoints.Count)
            {
                foreach (TtPoint point in polyPoints.Skip(_StartIndex))
                {
                    editedPoints.Add(point);
                    editedIndexes.Add(index++);
                }
            }

            _AddPointsCommand = new AddTtPointsCommand(addPoints, pointsManager);

            if (editedPoints.Count > 0)
                _EditPointsCommand = new EditTtPointsMultiValueCommand<int>(editedPoints, PointProperties.INDEX, editedIndexes);
        }

        public override void Redo()
        {
            if (_EditPointsCommand != null)
                _EditPointsCommand.Redo();

            _AddPointsCommand.Redo();
        }

        public override void Undo()
        {
            if (_EditPointsCommand != null)
                _EditPointsCommand.Undo();

            _AddPointsCommand.Undo();
        }


        protected override string GetCommandInfoDescription() => $"Create {_AddPointsCommand.CommandInfo.AffectedItems} Quondams in unit {_TargetPolygon.Name}";
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
