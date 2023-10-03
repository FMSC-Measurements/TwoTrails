using System;
using System.Collections.Generic;
using TwoTrails.Core.Points;

namespace TwoTrails.Core.ComponentModel.History
{
    public class MovePointsCommand : ITtPointsCommand
    {
        private TtManager _Manager;
        private TtPolygon _TargetPolygon;
        private int _InsertIndex;

        private Dictionary<String, int> _NewIndexes = new Dictionary<String, int>();
        private Dictionary<String, int> _OldIndexes = new Dictionary<String, int>();
        private Dictionary<String, TtPolygon> _OldPolygons = new Dictionary<String, TtPolygon>();
        private Dictionary<String, TtPoint> _EditedPoints = new Dictionary<String, TtPoint>();
        private Dictionary<String, TtPolygon> _PolygonsToBuild = new Dictionary<String, TtPolygon>();

        public MovePointsCommand(IEnumerable<TtPoint> points, TtManager pointsManager, TtPolygon targetPoly, int insertIndex) : base(points)
        {
            this._Manager = pointsManager;

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
                foreach (TtPoint point in pointsManager.GetPoints(poly.CN))
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
            _Manager.MovePointsToPolygon(Points, _TargetPolygon, _InsertIndex);

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
                _Manager.RebuildPolygon(poly);
        }


        protected override string GetCommandInfoDescription() => $"Move {Points.Count} points to unit {_TargetPolygon.Name}";
    }
}
