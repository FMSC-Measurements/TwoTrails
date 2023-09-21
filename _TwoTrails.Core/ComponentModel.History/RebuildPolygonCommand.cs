using System;
using System.Collections.Generic;
using System.Linq;
using TwoTrails.Core.Points;
using TwoTrails.Core.Units;

namespace TwoTrails.Core.ComponentModel.History
{
    class RebuildUnitCommand : ITtCommand
    {
        private TtManager pointsManager;

        private List<TtPoint> _Points;

        private TtUnit Unit;
        private bool Reindex;


        public RebuildUnitCommand(TtUnit unit, bool reindex, TtManager pointsManager)
        {
            this.pointsManager = pointsManager;
            this.Unit = unit;
            this.Reindex = reindex;

            _Points = pointsManager.GetPoints(unit.CN).Select(pt => pt.DeepCopy()).ToList();

            List<string> polyCNs = new List<string>() { unit.CN };
            List<TtPoint> addPoints = new List<TtPoint>();

            void addFromLinks(TtPoint point)
            {
                if (point.HasQuondamLinks)
                {
                    foreach (String cn in point.LinkedPoints)
                    {
                        QuondamPoint qp = pointsManager.GetPoint(cn) as QuondamPoint;

                        if (qp.UnitCN != point.UnitCN && !polyCNs.Contains(qp.UnitCN))
                        {
                            List<TtPoint> spoints = pointsManager.GetPoints(qp.UnitCN).DeepCopy().ToList();
                            addPoints.AddRange(spoints);
                            polyCNs.Add(qp.UnitCN);

                            foreach(TtPoint spoint in spoints)
                            {
                                addFromLinks(spoint);
                            }
                        }
                    }
                }
            }

            foreach (TtPoint point in _Points)
            {
                addFromLinks(point);
            }

            _Points.AddRange(addPoints);
        }

        public Type DataType => PointProperties.DataType;

        public bool RequireRefresh => true;

        public void Redo()
        {
            pointsManager.RebuildPolygon(Unit, Reindex);
        }

        public void Undo()
        {
            foreach (TtPoint point in _Points)
            {
                pointsManager.ReplacePoint(point);
            }
        }
    }
}
