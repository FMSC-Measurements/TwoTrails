using System.Collections.Generic;
using System.Linq;
using TwoTrails.Core.Points;

namespace TwoTrails.Core.ComponentModel.History
{
    public class ConvertQuondamsCommand : ITtPointsCommand
    {
        private readonly List<TtNmeaBurst> _AddNmea = new List<TtNmeaBurst>();
        private readonly List<GpsPoint> _ConvertedPoints;

        public ConvertQuondamsCommand(TtManager manager, IEnumerable<QuondamPoint> points) : base(manager, points)
        {
            _ConvertedPoints = points.Select(p => p.ConvertQuondam()).ToList();

            foreach (QuondamPoint point in points.Where(p => p.IsGpsAtBase()))
            {
                _AddNmea.AddRange(manager.GetNmeaBursts(point.ParentPointCN).Select(n => new TtNmeaBurst(n, point.CN)));
            }
        }

        public override void Redo()
        {
            Manager.ReplacePoints(_ConvertedPoints);

            if (_AddNmea != null)
            {
                Manager.AddNmeaBursts(_AddNmea);
            }
        }

        public override void Undo()
        {
            Manager.ReplacePoints(Points);

            if (_AddNmea != null)
            {
                foreach (TtPoint point in Points.Where(p => p.IsGpsAtBase()))
                {
                    Manager.RestoreNmeaBurts(point.CN);
                }
            }
        }

        protected override DataActionType GetActionType() => DataActionType.ModifiedPoints | (_AddNmea.Count > 0 ? DataActionType.InsertedNmea : DataActionType.None);
        protected override string GetCommandInfoDescription() => $"Convert {Points.Count} Quondams";
    }
}
