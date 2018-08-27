using System;
using System.Collections.Generic;
using System.Linq;
using TwoTrails.Core;
using TwoTrails.Core.Points;
using TwoTrails.DAL;

namespace TwoTrails.Utils
{
    public static class Import
    {
        public static void DAL(ITtManager manager, IReadOnlyTtDataLayer dal, IEnumerable<string> polyCNs = null,
            bool includeMetadata = true, bool includeGroups = true, bool includeNmea = true, bool convertForeignQuondams = true)
        {
            Dictionary<string, TtPolygon> polygons = manager.GetPolygons().ToDictionary(p => p.CN, p => p);
            Dictionary<string, TtMetadata> metadata = manager.GetMetadata().ToDictionary(m => m.CN, m => m);
            Dictionary<string, TtGroup> groups = manager.GetGroups().ToDictionary(g => g.CN, g => g);

            Dictionary<string, TtPoint> aPoints;
            Dictionary<string, TtPolygon> aPolys = new Dictionary<string, TtPolygon>();
            Dictionary<string, TtMetadata> aMeta = new Dictionary<string, TtMetadata>();
            Dictionary<string, TtGroup> aGroups = new Dictionary<string, TtGroup>();

            Dictionary<string, TtPolygon> iPolys = dal.GetPolygons().Where(p => polyCNs == null || polyCNs.Contains(p.CN)).ToDictionary(p => p.CN, p => p);
            Dictionary<string, string> polyMap = new Dictionary<string, string>();
            Dictionary<string, TtMetadata> iMeta = dal.GetMetadata().ToDictionary(m => m.CN, m => m);
            Dictionary<string, string> metaMap = new Dictionary<string, string>();
            Dictionary<string, TtGroup> iGroups = dal.GetGroups().ToDictionary(g => g.CN, g => g);
            Dictionary<string, string> groupMap = new Dictionary<string, string>();
            
            includeMetadata &= iMeta.Count > 0;
            includeGroups &= iGroups.Count > 0;

            foreach (TtPolygon poly in iPolys.Values)
            {
                poly.Name = $"{poly.Name} (Import)";

                if (polygons.ContainsKey(poly.CN))
                {
                    string cn = poly.CN;
                    poly.CN = Guid.NewGuid().ToString();
                    polyMap.Add(cn, poly.CN);
                }

                aPolys.Add(poly.CN, poly);
            }

            aPoints = aPolys.Values.SelectMany(p => dal.GetPoints(p.CN)).ToDictionary(p => p.CN, p => p);

            foreach (string metaCN in aPoints.Values.Select(p => p.MetadataCN).Distinct())
            {
                if (metaCN == null)
                    continue;

                if (includeMetadata)
                {
                    TtMetadata meta = iMeta.ContainsKey(metaCN) ? iMeta[metaCN]: manager.DefaultMetadata;

                    if (metadata.ContainsKey(metaCN))
                    {
                        if (!meta.Equals(metadata[metaCN]))
                        {
                            meta.Name = $"{meta.Name} (Import)";
                            meta.CN = Guid.NewGuid().ToString();
                            metaMap.Add(metaCN, meta.CN);

                            aMeta.Add(meta.CN, meta);
                        }
                    }
                    else
                    {
                        meta.Name = $"{meta.Name} (Import)";
                        aMeta.Add(meta.CN, meta);
                    }
                }
                else
                {
                    if (!metadata.ContainsKey(metaCN))
                        metaMap.Add(metaCN, Consts.EmptyGuid);
                }
            }

            foreach (string groupCN in aPoints.Values.Select(p => p.GroupCN).Distinct())
            {
                if (groupCN == null)
                    continue;

                if (includeGroups)
                {
                    TtGroup group = iGroups.ContainsKey(groupCN) ? iGroups[groupCN] : manager.MainGroup;

                    if (groups.ContainsKey(groupCN))
                    {
                        if (group != groups[groupCN])
                        {
                            group.Name = $"{group.Name} (Import)";
                            group.CN = Guid.NewGuid().ToString();
                            groupMap.Add(groupCN, group.CN);

                            aGroups.Add(group.CN, group);
                        }
                    }
                    else
                    {
                        group.Name = $"{group.Name} (Import)";
                        aGroups.Add(group.CN, group);
                    }
                }
                else
                {
                    if (!groups.ContainsKey(groupCN))
                        groupMap.Add(groupCN, Consts.EmptyGuid);
                }
            }

            Func<QuondamPoint, GpsPoint> convertQuondam = (qpoint) =>
            {
                GpsPoint gpsPoint = new GpsPoint(qpoint)
                {
                    Comment = string.IsNullOrWhiteSpace(qpoint.Comment) ? qpoint.ParentPoint.Comment : qpoint.Comment,
                    TimeCreated = DateTime.Now
                };

                gpsPoint.SetAccuracy(aPolys[qpoint.PolygonCN].Accuracy);

                return gpsPoint;
            };

            if (convertForeignQuondams)
            {
                foreach(QuondamPoint qpoint in aPoints.Values
                    .Where(p => p.OpType == OpType.Quondam && !aPolys.ContainsKey((p as QuondamPoint).ParentPoint.PolygonCN))
                            .Cast<QuondamPoint>().ToList())
                {
                    aPoints[qpoint.CN] = convertQuondam(qpoint);
                }
            }

            foreach (TtPoint p in aPoints.Values.ToList())
            {
                if (polyMap.ContainsKey(p.PolygonCN))
                    p.PolygonCN = polyMap[p.PolygonCN];

                p.Polygon = iPolys[p.PolygonCN];

                if (metaMap.ContainsKey(p.MetadataCN))
                    p.MetadataCN = metaMap[p.MetadataCN];

                p.Metadata = metadata.ContainsKey(p.MetadataCN) ? metadata[p.MetadataCN] : aMeta[p.MetadataCN];

                if (groupMap.ContainsKey(p.GroupCN))
                    p.GroupCN = groupMap[p.GroupCN];

                p.Group = groups.ContainsKey(p.GroupCN) ? groups[p.GroupCN] : aGroups[p.GroupCN];
                
                p.ClearLinks();

                if (p.OpType == OpType.Quondam && p is QuondamPoint qp)
                {
                    if (aPoints.ContainsKey(qp.ParentPointCN))
                    {
                        qp.ParentPoint = aPoints[qp.ParentPointCN];
                    }
                    else
                    {
                        throw new Exception("Foriegn Quondam");

                        //TtPoint parent = qp.ParentPoint;

                        //if (parent.IsTravType())
                        //    parent = new GpsPoint(parent);

                        //parent.Index = qp.Index;
                        //parent.Polygon = qp.Polygon;
                        //parent.Group = qp.Group;
                        //parent.Metadata = qp.Metadata;
                        //parent.CN = qp.CN;

                        //aPoints[qp.CN] = convertQuondam(qp);
                    }
                }
            }

            foreach (QuondamPoint qp in aPoints.Values.Where(p => p.OpType == OpType.Quondam))
            {
                aPoints[qp.ParentPointCN].AddLinkedPoint(qp);
            }

            TtHistoryManager hm = manager as TtHistoryManager;
            if (hm != null)
                hm.StartMultiCommand();

            try
            {
                foreach (TtPolygon p in aPolys.Values)
                {
                    manager.AddPolygon(p);
                }

                foreach (TtMetadata m in aMeta.Values)
                {
                    manager.AddMetadata(m);
                }

                foreach (TtGroup g in aGroups.Values)
                {
                    manager.AddGroup(g);
                }

                manager.AddPoints(aPoints.Values);

                if (includeNmea)
                {
                    manager.AddNmeaBursts(aPoints.Values.Where(p => p.IsGpsType()).SelectMany(p => dal.GetNmeaBursts(p.CN)));
                }

                manager.UpdateDataAction(DataActionType.DataImported, dal.FilePath);

                if (hm != null)
                    hm.CommitMultiCommand();
            }
            catch (Exception e)
            {
                if (hm != null)
                    hm.RevertMultiCommand();
                throw e;
            }
        }
    }
}
