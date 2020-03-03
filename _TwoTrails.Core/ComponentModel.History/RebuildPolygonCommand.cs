using System;
using System.Collections.Generic;
using System.Linq;
using TwoTrails.Core.Points;

namespace TwoTrails.Core.ComponentModel.History
{
    class RebuildPolygonCommand : ITtCommand
    {
        private TtManager pointsManager;

        private List<TtPoint> Points;

        private TtPolygon Polygon;
        private bool Reindex;


        public RebuildPolygonCommand(TtPolygon polygon, bool reindex, TtManager pointsManager)
        {
            this.pointsManager = pointsManager;
            this.Polygon = polygon;
            this.Reindex = reindex;

            Points = pointsManager.GetPoints(polygon.CN).Select(pt => pt.DeepCopy()).ToList();

            List<string> polyCNs = new List<string>() { polygon.CN };
            List<TtPoint> addPoints = new List<TtPoint>();

            void addFromLinks(TtPoint point)
            {
                if (point.HasQuondamLinks)
                {
                    foreach (String cn in point.LinkedPoints)
                    {
                        QuondamPoint qp = pointsManager.GetPoint(cn) as QuondamPoint;

                        if (qp.PolygonCN != point.PolygonCN && !polyCNs.Contains(qp.PolygonCN))
                        {
                            List<TtPoint> spoints = pointsManager.GetPoints(qp.PolygonCN).DeepCopy().ToList();
                            addPoints.AddRange(spoints);
                            polyCNs.Add(qp.PolygonCN);

                            foreach(TtPoint spoint in spoints)
                            {
                                addFromLinks(spoint);
                            }
                        }
                    }
                }
            }

            foreach (TtPoint point in Points)
            {
                addFromLinks(point);
            }
        }

        public Type DataType => PointProperties.DataType;

        public bool RequireRefresh => true;

        public void Redo()
        {
            pointsManager.RebuildPolygon(Polygon, Reindex);
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
