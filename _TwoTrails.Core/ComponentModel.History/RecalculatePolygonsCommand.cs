using System;
using System.Collections.Generic;
using System.Linq;
using TwoTrails.Core.Points;

namespace TwoTrails.Core.ComponentModel.History
{
    class RecalculatePolygonsCommand : ITtBaseCommand
    {
        private TtManager _Manager;
        private List<TtPoint> _Points;

        public RecalculatePolygonsCommand(TtManager pointsManager)
        {
            this._Manager = pointsManager;

            _Points = pointsManager.Points.Select(pt => pt.DeepCopy()).ToList();
        }


        public override void Redo()
        {
            _Manager.RecalculatePolygons();
        }

        public override void Undo()
        {
            foreach (TtPoint point in _Points)
            {
                _Manager.ReplacePoint(point);
            }
        }


        protected override int GetAffectedItemCount() => _Points.Count;
        protected override DataActionType GetActionType() => DataActionType.ModifiedPoints;
        protected override string GetCommandInfoDescription() => "Recalculate all polygons";
    }
}
