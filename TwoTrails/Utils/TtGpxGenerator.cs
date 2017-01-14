using FMSC.Core.Xml.GPX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwoTrails.Core;
using TwoTrails.Core.Points;

namespace TwoTrails.Utils
{
    public static class TtGpxGenerator
    {
        public static GpxDocument Generate(ITtManager manager, string name, string description = null)
        {
            return Generate(new ITtManager[] { manager }, name, description);
        }

        public static GpxDocument Generate(IEnumerable<ITtManager> managers, string name, string description = null)
        {
            GpxDocument doc = new GpxDocument("USFS TwoTrails - http://www.fs.fed.us/fmsc/measure/geospatial/twotrails/");

            doc.Metadata = new Metadata(name)
            {
                Time = DateTime.Now,
                Link = @"http://www.fs.fed.us/fmsc/measure/geospatial/twotrails/"
            };

            #region Create Polygons

            foreach (ITtManager manager in managers)
            {

                foreach (TtPolygon poly in manager.GetPolygons())
                {
                    Route AdjRoute = new Route(poly.Name + " - Adj Boundary", poly.Description);
                    Track AdjTrack = new Track(poly.Name + " - Adj Navigation", poly.Description);

                    Route UnAdjRoute = new Route(poly.Name + " - UnAdj Boundary", poly.Description);
                    Track UnAdjTrack = new Track(poly.Name + " - UnAdj Navigation", poly.Description);

                    AdjTrack.Segments.Add(new TrackSegment());
                    UnAdjTrack.Segments.Add(new TrackSegment());

                    List<TtPoint> points = manager.GetPoints(poly.CN);

                    if (points != null && points.Count > 0)
                    {
                        foreach (TtPoint point in points)
                        {
                            System.Windows.Point adj = TtUtils.GetLatLon(point, true);
                            Point adjpoint = new Point(adj.Y, adj.X, point.AdjZ)
                            {
                                Name = point.PID.ToString(),
                                Time = point.TimeCreated,
                                Comment = point.Comment,
                                Description = String.Format("Point Operation: {0}<br>UtmX: {1}<br>UtmY: {2}", point.OpType, point.AdjX, point.AdjY)
                            };

                            System.Windows.Point unadj = TtUtils.GetLatLon(point, true);
                            Point unAdjpoint = new Point(unadj.Y, unadj.X, point.UnAdjZ)
                            {
                                Name = point.PID.ToString(),
                                Time = point.TimeCreated,
                                Comment = point.Comment,
                                Description = String.Format("Point Operation: {0}<br>UtmX: {1}<br>UtmY: {2}", point.OpType, point.UnAdjX, point.UnAdjY)
                            };

                            #region Add points to lists
                            if (point.IsBndPoint())
                            {
                                AdjRoute.Points.Add(adjpoint);
                                UnAdjRoute.Points.Add(unAdjpoint);
                            }

                            if (point.IsNavPoint())
                            {
                                AdjTrack.Segments[0].Points.Add(adjpoint);
                                UnAdjTrack.Segments[0].Points.Add(unAdjpoint);
                            }
                            else if (point.OpType == OpType.Quondam)
                            {
                                QuondamPoint p = (QuondamPoint)point;

                                if (p.IsNavPoint())
                                {
                                    AdjTrack.Segments[0].Points.Add(adjpoint);
                                    UnAdjTrack.Segments[0].Points.Add(unAdjpoint);
                                }
                            }

                            if (point.OpType == OpType.WayPoint)
                            {
                                doc.Waypoints.Add(unAdjpoint);
                            }
                            #endregion
                        }
                    }


                    doc.Routes.Add(AdjRoute);
                    doc.Routes.Add(UnAdjRoute);
                    doc.Tracks.Add(AdjTrack);
                    doc.Tracks.Add(UnAdjTrack);
                } 
            }

            #endregion

            return doc;
        }
    }
}
