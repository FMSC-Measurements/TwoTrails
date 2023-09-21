using System;
using System.Collections.Generic;
using System.Linq;
using TwoTrails.Core.Points;

namespace TwoTrails.Core.ComponentModel.History
{
    class RecalculateUnitsCommand : ITtCommand
    {
        private TtManager pointsManager;

        private List<TtPoint> Points;


        public RecalculateUnitsCommand(TtManager pointsManager)
        {
            this.pointsManager = pointsManager;

            Points = pointsManager.Points.Select(pt => pt.DeepCopy()).ToList();
        }

        public Type DataType => PointProperties.DataType;

        public bool RequireRefresh => true;

        public void Redo()
        {
            pointsManager.RecalculateUnits();
        }

        public void Undo()
        {
            foreach (TtPoint point in Points)
            {
                pointsManager.ReplacePoint(point);
            }
        }
    }
}
