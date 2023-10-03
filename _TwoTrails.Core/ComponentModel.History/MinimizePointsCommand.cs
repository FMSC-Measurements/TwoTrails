using System;
using System.Collections.Generic;
using System.Linq;
using TwoTrails.Core.Points;

namespace TwoTrails.Core.ComponentModel.History
{
    public class MinimizePointsCommand : ITtPointsCommand
    {
        private TtManager _Manager;
        private List<bool> NewValues;
        private List<bool> OldValues;
        private readonly Guid _ID = Guid.NewGuid();

        public MinimizePointsCommand(IEnumerable<TtPoint> points, IEnumerable<bool> newValues, TtManager manager) : base(points)
        {
            _Manager = manager;

            this.NewValues = new List<bool>(newValues);
            this.OldValues = points.Select(p => p.OnBoundary).ToList();
        }

        public override void Redo()
        {

            for (int i = 0; i < Points.Count; i++)
            {
                Points[i].OnBoundary = NewValues[i];
            }

            _Manager.AddAction(DataActionType.PointMinimization, $"{Points[0].Polygon} minimized to {Points.Count(p => p.OnBoundary)} points.", _ID);
        }

        public override void Undo()
        {
            for (int i = 0; i < Points.Count; i++)
            {
                Points[i].OnBoundary = OldValues[i];
            }

            _Manager.RemoveAction(_ID);
        }
    }
}
