using System;
using System.Collections.Generic;
using System.Linq;
using TwoTrails.Core.Points;

namespace TwoTrails.Core.ComponentModel.History
{
    public class ConvertQuondamsCommand : ITtPointsCommand
    {
        private TtManager pointsManager;
        private List<TtNmeaBurst> _AddNmea = new List<TtNmeaBurst>();
        private List<GpsPoint> _ConvertedPoints;

        public ConvertQuondamsCommand(IEnumerable<QuondamPoint> points, TtManager pointsManager) : base(points)
        {
            this.pointsManager = pointsManager;

            _ConvertedPoints = points.Select(p => p.ConvertQuondam()).ToList();

            foreach (QuondamPoint point in points.Where(p => p.IsGpsAtBase()))
            {
                _AddNmea.AddRange(pointsManager.GetNmeaBursts(point.ParentPointCN).Select(n => new TtNmeaBurst(n, point.CN)));
            }
        }

        public override void Redo()
        {
            pointsManager.ReplacePoints(_ConvertedPoints);

            if (_AddNmea != null)
            {
                pointsManager.AddNmeaBursts(_AddNmea);
            }
        }

        public override void Undo()
        {
            pointsManager.ReplacePoints(Points);

            if (_AddNmea != null)
            {
                foreach (TtPoint point in Points.Where(p => p.IsGpsAtBase()))
                {
                    pointsManager.RestoreNmeaBurts(point.CN);
                }
            }
        }

        protected override DataActionType GetActionType() => DataActionType.ModifiedPoints | (_AddNmea.Count > 0 ? DataActionType.InsertedNmea : DataActionType.None);
        protected override string GetCommandInfoDescription() => $"Convert {Points.Count} Quondams";
    }
}
