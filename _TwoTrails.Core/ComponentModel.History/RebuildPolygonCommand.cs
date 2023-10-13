using System;
using System.Collections.Generic;
using System.Linq;
using TwoTrails.Core.Points;

namespace TwoTrails.Core.ComponentModel.History
{
    public class RebuildPolygonCommand : ITtPolygonCommand
    {
        private readonly List<TtPoint> _Points;
        private readonly bool _Reindex;


        public RebuildPolygonCommand(TtManager manager, TtPolygon polygon, bool reindex) : base(manager, polygon)
        {
            this._Reindex = reindex;

            _Points = manager.GetPoints(polygon.CN).Select(pt => pt.DeepCopy()).ToList();

            List<string> polyCNs = new List<string>() { polygon.CN };
            List<TtPoint> addPoints = new List<TtPoint>();

            void addFromLinks(TtPoint point)
            {
                if (point.HasQuondamLinks)
                {
                    foreach (String cn in point.LinkedPoints)
                    {
                        QuondamPoint qp = manager.GetPoint(cn) as QuondamPoint;

                        if (qp.PolygonCN != point.PolygonCN && !polyCNs.Contains(qp.PolygonCN))
                        {
                            List<TtPoint> spoints = manager.GetPoints(qp.PolygonCN).DeepCopy().ToList();
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

            foreach (TtPoint point in _Points)
            {
                addFromLinks(point);
            }

            _Points.AddRange(addPoints);
        }

        public override void Redo()
        {
            Manager.RebuildPolygon(Polygon, _Reindex);
        }

        public override void Undo()
        {
            foreach (TtPoint point in _Points)
            {
                Manager.ReplacePoint(point);
            }
        }

        protected override DataActionType GetActionType() => DataActionType.ModifiedPoints;
        protected override string GetCommandInfoDescription() => $"Reindex unit {Polygon.Name}";
    }
}
