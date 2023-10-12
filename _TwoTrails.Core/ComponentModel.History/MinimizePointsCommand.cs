using System;
using System.Collections.Generic;
using System.Linq;
using TwoTrails.Core.Points;

namespace TwoTrails.Core.ComponentModel.History
{
    public class MinimizePointsCommand : EditTtPointsMultiValueCommand<bool>
    {
        private AddDataActionCommand _AddDataActionCommand;

        public MinimizePointsCommand(IEnumerable<TtPoint> points, IEnumerable<bool> newValues, TtManager manager) : base(points, PointProperties.BOUNDARY, newValues)
        {
            _AddDataActionCommand = new AddDataActionCommand(DataActionType.PointMinimization, manager, $"{Points[0].Polygon} minimized to {Points.Count(p => p.OnBoundary)} points.");
        }

        public override void Redo()
        {
            base.Redo();
            _AddDataActionCommand.Redo();
        }

        public override void Undo()
        {
            _AddDataActionCommand.Undo();
            base.Undo();
        }


        protected override DataActionType GetActionType() => base.GetActionType() | DataActionType.PointMinimization;

        protected override string GetCommandInfoDescription() =>
            $"Minimize polygon {(Points.Count > 0 ? Points[0].Polygon.Name : String.Empty)} to {NewValues.Count(b => b)} points";
    }
}
