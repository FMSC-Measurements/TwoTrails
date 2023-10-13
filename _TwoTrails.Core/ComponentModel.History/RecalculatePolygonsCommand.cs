using System;
using System.Collections.Generic;
using System.Linq;
using TwoTrails.Core.Points;

namespace TwoTrails.Core.ComponentModel.History
{
    public class RecalculatePolygonsCommand : ITtBaseCommand
    {
        private readonly List<TtPoint> _Points;

        public RecalculatePolygonsCommand(TtManager manager) : base(manager)
        {
            _Points = manager.Points.Select(pt => pt.DeepCopy()).ToList();
        }


        public override void Redo()
        {
            Manager.RecalculatePolygons();
        }

        public override void Undo()
        {
            foreach (TtPoint point in _Points)
            {
                Manager.ReplacePoint(point);
            }
        }


        protected override int GetAffectedItemCount() => _Points.Count;
        protected override DataActionType GetActionType() => DataActionType.ModifiedPoints;
        protected override string GetCommandInfoDescription() => "Recalculate all polygons";
    }
}
