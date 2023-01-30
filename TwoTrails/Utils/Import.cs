using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TwoTrails.Core;
using TwoTrails.Core.ComponentModel.History;
using TwoTrails.Core.Points;
using TwoTrails.DAL;

namespace TwoTrails.Utils
{
    public static class Import
    {
        public static void DAL(ITtManager manager, IReadOnlyTtDataLayer dal, IEnumerable<string> polyCNs = null,
            bool includeMetadata = true, bool includeGroups = true, bool includeNmea = true, bool convertForeignQuondams = true)
        {
            int existingPointsImported = 0; // points that had a CN already in the project imported

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

            List<TtNmeaBurst> aNmea = new List<TtNmeaBurst>();

            includeMetadata &= iMeta.Count > 0;
            includeGroups &= iGroups.Count > 0;

            foreach (TtPolygon poly in iPolys.Values.DeepCopy())
            {
                poly.Name = $"{poly.Name}_Import";

                if (polygons.ContainsKey(poly.CN))
                {
                    string cn = poly.CN;
                    poly.CN = Guid.NewGuid().ToString();
                    polyMap.Add(cn, poly.CN);
                }

                aPolys.Add(poly.CN, poly);
            }

            aPoints = iPolys.Values.SelectMany(poly => dal.GetPoints(poly.CN, convertForeignQuondams)).ToDictionary(p => p.CN, p => p);


            foreach (string metaCN in aPoints.Values.Select(p => p.MetadataCN).Distinct())
            {
                if (metaCN == null)
                    continue;
                
                TtMetadata meta = iMeta.ContainsKey(metaCN) ? iMeta[metaCN].DeepCopy(): manager.DefaultMetadata;

                if (metadata.ContainsKey(metaCN))
                {
                    if (!meta.Equals(metadata[metaCN]))
                    {
                        meta.Name = $"{meta.Name}_Import";
                        meta.CN = Guid.NewGuid().ToString();
                        metaMap.Add(metaCN, meta.CN);

                        aMeta.Add(meta.CN, meta);
                    }
                }
                else
                {
                    meta.Name = $"{meta.Name}_Import";
                    aMeta.Add(meta.CN, meta);
                }
            }

            foreach (string groupCN in aPoints.Values.Select(p => p.GroupCN).Distinct())
            {
                if (groupCN == null)
                    continue;

                if (includeGroups)
                {
                    TtGroup group = iGroups.ContainsKey(groupCN) ? iGroups[groupCN].DeepCopy() : manager.MainGroup;

                    if (groups.ContainsKey(groupCN))
                    {
                        if (group != groups[groupCN])
                        {
                            group.Name = $"{group.Name}_Import";
                            group.CN = Guid.NewGuid().ToString();
                            groupMap.Add(groupCN, group.CN);

                            aGroups.Add(group.CN, group);
                        }
                    }
                    else
                    {
                        group.Name = $"{group.Name}_Import";
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
                TtPoint cPoint = qpoint.ParentPoint ?? (aPoints.ContainsKey(qpoint.ParentPointCN) ? aPoints[qpoint.ParentPointCN] : qpoint);
                
                GpsPoint gpsPoint = new GpsPoint(cPoint)
                {
                    CN = qpoint.CN,
                    Index = qpoint.Index,
                    PolygonCN = qpoint.PolygonCN,
                    MetadataCN = qpoint.MetadataCN,
                    GroupCN = qpoint.GroupCN,
                    ManualAccuracy = qpoint.ManualAccuracy ?? (cPoint.IsManualAccType() ? (cPoint as IManualAccuracy).ManualAccuracy : null),
                    Comment = string.IsNullOrWhiteSpace(qpoint.Comment) ?
                        (cPoint.OpType == OpType.Quondam ? qpoint.ParentPoint?.Comment ?? cPoint.Comment : cPoint.Comment) : qpoint.Comment,
                    TimeCreated = DateTime.Now
                };

                gpsPoint.SetAccuracy(aPolys[qpoint.PolygonCN].Accuracy);

                return gpsPoint;
            };

            //if (convertForeignQuondams)
            //{
            //    foreach (QuondamPoint qpoint in aPoints.Values
            //        .Where(p => p.OpType == OpType.Quondam && p is QuondamPoint qp && !aPoints.ContainsKey(qp.ParentPointCN)).ToList())
            //    {
            //        aPoints[qpoint.CN] = convertQuondam(qpoint);
            //    }
            //}

            foreach (QuondamPoint qpoint in aPoints.Values
                    .Where(p => p.OpType == OpType.Quondam && p is QuondamPoint qp).ToList())
            {
                if (aPoints.ContainsKey(qpoint.ParentPointCN))
                {
                    TtPoint pp = aPoints[qpoint.ParentPointCN];

                    if (pp is QuondamPoint qp)
                    {
                        aPoints[qpoint.CN] = convertQuondam(qpoint);
                    }
                    else if (qpoint.ParentPoint == null)
                    {
                        if (convertForeignQuondams)
                        {
                            aPoints[qpoint.CN] = convertQuondam(qpoint);
                        }
                        else
                        {
                            throw new ForiegnQuondamException();
                        }
                    }
                    else
                    {
                        qpoint.ParentPoint = pp;
                    }
                }
                else
                {
                    aPoints[qpoint.CN] = convertQuondam(qpoint);
                }
            }

            foreach (TtPoint point in aPoints.Values.Where(p => p.OpType != OpType.Quondam)
                .Concat(aPoints.Values.Where(p => p.OpType == OpType.Quondam)).ToList())
            {
                string oldCN = null;

                if (manager.PointExists(point.CN))
                {
                    oldCN = point.CN;
                    point.CN = Guid.NewGuid().ToString();

                    if (point.HasQuondamLinks)
                    {
                        foreach (string qCN in point.LinkedPoints)
                        {
                            if (aPoints[qCN] is QuondamPoint qpoint)
                            {
                                qpoint.ParentPoint = point;
                            }
                        }
                    }

                    existingPointsImported++;
                }

                if (polyMap.ContainsKey(point.PolygonCN))
                    point.PolygonCN = polyMap[point.PolygonCN];

                point.Polygon = aPolys[point.PolygonCN];

                if (!includeMetadata && point is GpsPoint gps)
                {
                    TtMetadata meta = aMeta.ContainsKey(point.MetadataCN) ? aMeta[point.MetadataCN] : null;

                    if (meta != null && meta.Zone != manager.DefaultMetadata.Zone)
                        TtCoreUtils.ChangeGpsZone(gps, manager.DefaultMetadata.Zone, meta.Zone);

                    gps.MetadataCN = Consts.EmptyGuid;
                }
                else if (metaMap.ContainsKey(point.MetadataCN))
                    point.MetadataCN = metaMap[point.MetadataCN];

                point.Metadata = metadata.ContainsKey(point.MetadataCN) ? metadata[point.MetadataCN] : aMeta[point.MetadataCN];

                if (groupMap.ContainsKey(point.GroupCN))
                    point.GroupCN = groupMap[point.GroupCN];

                point.Group = groups.ContainsKey(point.GroupCN) ? groups[point.GroupCN] : aGroups[point.GroupCN];
                
                point.ClearLinkedPoints();

                if (point.OpType == OpType.Quondam && point is QuondamPoint qp)
                {
                    if (!aPoints.ContainsKey(qp.ParentPointCN))
                    {
                        throw new Exception("Foreign Quondam");
                    }
                }

                if (includeNmea && point.IsGpsType())
                {
                    aNmea.AddRange(oldCN == null ?
                        dal.GetNmeaBursts(point.CN).Select(nmea => manager.NmeaExists(nmea.CN) ? new TtNmeaBurst(nmea, point.CN) : nmea) :
                        dal.GetNmeaBursts(oldCN).Select(nmea => new TtNmeaBurst(nmea, point.CN)));
                }
            }


            //reindex points
            foreach (TtPolygon poly in aPolys.Values)
            {
                int index = 0;
                foreach (TtPoint point in aPoints.Values.Where(p => p.PolygonCN == poly.CN).OrderBy(p => p.Index))
                {
                    point.Index = index++;
                }
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

                if (includeMetadata)
                {
                    foreach (TtMetadata m in aMeta.Values)
                    {
                        manager.AddMetadata(m);
                    }
                }

                if (includeGroups)
                {
                    foreach (TtGroup g in aGroups.Values)
                    {
                        manager.AddGroup(g);
                    }
                }

                manager.AddPoints(aPoints.Values);

                if (includeNmea)
                {
                    manager.AddNmeaBursts(aNmea);
                }

                if (hm != null)
                {
#if DEBUG
                    string filePath = Path.GetFileName(dal.FilePath);
#else
                    string filePath = dal.FilePath;
#endif

                    hm.CommitMultiCommand(
                        new AddDataActionCommand(DataActionType.DataImported, hm.BaseManager,
                        $"{filePath}{(existingPointsImported > 0 ? $" ({existingPointsImported} Point CN Changes)" : String.Empty)}"));
                }
            }
            catch (Exception e)
            {
                if (hm != null)
                    hm.ResetMultiCommand();
                throw e;
            }
        }


        public class ForiegnQuondamException : Exception
        {
            public ForiegnQuondamException() : base("Foriegn Quondam") { }
        }
    }
}
