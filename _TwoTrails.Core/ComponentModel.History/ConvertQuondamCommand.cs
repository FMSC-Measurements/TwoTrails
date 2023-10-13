using System.Collections.Generic;
using System.Linq;
using TwoTrails.Core.Points;

namespace TwoTrails.Core.ComponentModel.History
{
    public class ConvertQuondamCommand : ITtPointCommand
    {
        private readonly List<TtNmeaBurst> _AddNmea = new List<TtNmeaBurst>();
        private readonly TtPoint _ConvertedPoint;

        public ConvertQuondamCommand(TtManager manager, QuondamPoint point) : base(manager, point)
        {
            _ConvertedPoint = point.ConvertQuondam();

            if (point.IsGpsAtBase())
            {
                _AddNmea.AddRange(manager.GetNmeaBursts(point.ParentPointCN).Select(n => new TtNmeaBurst(n, _ConvertedPoint.CN)));
            }
        }

        public override void Redo()
        {
            Manager.ReplacePoint(_ConvertedPoint);

            if (_AddNmea.Count > 0)
            {
                Manager.AddNmeaBursts(_AddNmea);
            }
        }

        public override void Undo()
        {
            Manager.ReplacePoint(Point);

            if (_AddNmea.Count > 0)
            {
                Manager.RestoreNmeaBurts(Point.CN);
            }
        }

        protected override DataActionType GetActionType() => DataActionType.ModifiedPoints | (_AddNmea.Count > 0 ? DataActionType.InsertedNmea : DataActionType.None);
        protected override string GetCommandInfoDescription() => $"Convert Quondam {Point}";
    }
}
