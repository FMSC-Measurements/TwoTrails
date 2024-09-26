using System;
using System.Collections.Generic;
using TwoTrails.Core.Points;
using TwoTrails.Core.Units;

namespace TwoTrails.Core.ComponentModel.History
{
    public class MovePointsCommand : ITtPointsCommand
    {
        private readonly TtPolygon _TargetPolygon;
        private readonly int _InsertIndex;

        private readonly Dictionary<String, int> _NewIndexes = new Dictionary<String, int>();
        private readonly Dictionary<String, int> _OldIndexes = new Dictionary<String, int>();
        private readonly Dictionary<String, TtPolygon> _OldPolygons = new Dictionary<String, TtPolygon>();
        private readonly Dictionary<String, TtPoint> _EditedPoints = new Dictionary<String, TtPoint>();
        private readonly Dictionary<String, TtPolygon> _PolygonsToBuild = new Dictionary<String, TtPolygon>();

        public MovePointsCommand(TtManager manager, IEnumerable<TtPoint> points, TtPolygon targetPoly, int insertIndex) : base(manager, points)
        {
            _TargetPolygon = targetPoly;
            _InsertIndex = insertIndex;

            _PolygonsToBuild.Add(_TargetPolygon.CN, _TargetPolygon);

            foreach (TtPoint point in Points)
            {
                _EditedPoints.Add(point.CN, point);
                _OldIndexes.Add(point.CN, point.Index);
                _OldPolygons.Add(point.CN, point.Polygon);

                if (!_PolygonsToBuild.ContainsKey(point.PolygonCN))
                    _PolygonsToBuild.Add(point.PolygonCN, point.Polygon);
            }

            foreach (TtPolygon poly in _PolygonsToBuild.Values)
            {
                foreach (TtPoint point in manager.GetPoints(poly.CN))
                {
                    if (!_EditedPoints.ContainsKey(point.CN))
                    {
                        _EditedPoints.Add(point.CN, point);
                        _OldIndexes.Add(point.CN, point.Index);
                    }
                }
            }
        }

        public override void Redo()
        {
            Manager.MovePointsToPolygon(Points, _TargetPolygon, _InsertIndex);

            _NewIndexes.Clear();
            foreach (TtPoint point in _EditedPoints.Values)
            {
                _NewIndexes.Add(point.CN, point.Index);
            }
        }

        public override void Undo()
        {
            foreach (TtPoint point in _EditedPoints.Values)
            {
                point.Index = _OldIndexes[point.CN];
            }

            foreach (TtPoint point in Points)
            {
                point.Polygon = _OldPolygons[point.CN];
            }

            foreach (TtPolygon poly in _PolygonsToBuild.Values)
                Manager.RebuildPolygon(poly);
        }

        protected override DataActionType GetActionType() => DataActionType.ModifiedPoints;
        protected override string GetCommandInfoDescription() => $"Move {Points.Count} points to unit {_TargetPolygon.Name}";
    }
}
