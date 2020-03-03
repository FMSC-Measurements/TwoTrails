using System;
using System.Collections.Generic;
using System.Linq;
using TwoTrails.Core.Points;

namespace TwoTrails.Core.ComponentModel.History
{
    class RecalculatePolygonsCommand : ITtCommand
    {
        private TtManager pointsManager;

        private List<TtPoint> Points;


        public RecalculatePolygonsCommand(TtManager pointsManager)
        {
            this.pointsManager = pointsManager;

            Points = pointsManager.Points.Select(pt => pt.DeepCopy()).ToList();
        }

        public Type DataType => PointProperties.DataType;

        public bool RequireRefresh => true;

        public void Redo()
        {
            pointsManager.RecalculatePolygons();
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
