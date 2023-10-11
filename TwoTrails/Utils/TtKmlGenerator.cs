using FMSC.Core;
using FMSC.Core.Xml.KML;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using TwoTrails.Core;
using TwoTrails.Core.Points;
using Point = FMSC.Core.Point;

namespace TwoTrails.Utils
{
    public static class TtKmlGenerator
    {
        const int AdjLineSize = 5;
        const int UnAdjLineSize = 7;

        public static KmlDocument Generate(ITtManager manager, string name, string description = null)
        {
            return Generate(new ITtManager[] { manager }, name, description);
        }

        public static KmlDocument Generate(IEnumerable<ITtManager> managers, string name, string description = null)
        {
            KmlDocument doc = new KmlDocument(name, description ?? String.Empty);

            List<PolygonStyle> polyStyles = new List<PolygonStyle>();
            Dictionary<Color, Tuple<Style, Style>> styles = new Dictionary<Color, Tuple<Style, Style>>();
            Dictionary<Color, StyleMap> styleMaps = new Dictionary<Color, StyleMap>();
            
            List<PolygonGraphicOptions> pgos = new List<PolygonGraphicOptions>();

            foreach (ITtManager manager in managers)
            {
                Dictionary<string, TtMetadata> metadata = manager.GetMetadata().ToDictionary(m => m.CN, m => m);
                pgos.AddRange(manager.GetPolygonGraphicOptions().Where(p => !pgos.Any(ip => ip.CN == p.CN)));

                foreach (TtPolygon poly in manager.GetPolygons())
                {
                    PolygonGraphicOptions pgo = pgos.FirstOrDefault(p => p.CN == poly.CN);

                    PolygonStyle style = (pgo != null && polyStyles.Count > 0) ? polyStyles.FirstOrDefault(ps => ps.Key == pgo) : null;

                    //Get/Generate Polygon Style
                    if (style == null)
                    {
                        if (pgo == null)
                        {
                            pgo = new PolygonGraphicOptions(Consts.EmptyGuid,
                                -769226,
                                -15064194,
                                -12627531,
                                -15064194,
                                -6543440,
                                -11922292,
                                -1683200,
                                AdjLineSize, UnAdjLineSize);
                        }

                        style = new PolygonStyle(pgo,
                            GetStyle(styleMaps, styles, MediaTools.GetColor(pgo.AdjBndColor), true),
                            GetStyle(styleMaps, styles, MediaTools.GetColor(pgo.UnAdjBndColor), false),
                            GetStyle(styleMaps, styles, MediaTools.GetColor(pgo.AdjNavColor), true),
                            GetStyle(styleMaps, styles, MediaTools.GetColor(pgo.UnAdjNavColor), false),
                            GetStyle(styleMaps, styles, MediaTools.GetColor(pgo.UnAdjPtsColor), false),
                            GetStyle(styleMaps, styles, MediaTools.GetColor(pgo.WayPtsColor), false));

                        polyStyles.Add(style);
                    }

                    List<TtPoint> points = manager.GetPoints(poly.CN);

                    #region Create Poly Folder and Point Folders
                    KmlFolder polyFolder = new KmlFolder(poly.Name, poly.Description)
                    {

                        Open = false,
                        Visibility = true,
                        Snippit = poly.Description,
                        ExtendedData = new ExtendedData
                        {
                            DataItems = new List<ExtendedData.Data>()
                            {
                                new ExtendedData.Data("Accuracy (M)", poly.Accuracy),
                                new ExtendedData.Data("Perimeter (M)", poly.Perimeter),
                                new ExtendedData.Data("Perimeter (Ft)", poly.PerimeterFt),
                                new ExtendedData.Data("Area (Acres)", poly.AreaAcres),
                                new ExtendedData.Data("Area (HectaAcres)", poly.AreaHectaAcres)
                            }
                        }
                    };

                    KmlFolder fAdjBound = new KmlFolder("AdjBound", "Adjusted Boundary Polygon")
                    {
                        Visibility = true,
                        StyleUrl = style.AdjBndStyle.StyleUrl,
                        Open = false
                    };
                    KmlFolder fUnAdjBound = new KmlFolder("UnAdjBound", "UnAdjusted Boundary Polygon")
                    {
                        Visibility = false,
                        StyleUrl = style.UnAdjBndStyle.StyleUrl,
                        Open = false
                    };
                    KmlFolder fAdjNav = new KmlFolder("AdjNav", "Adjusted Navigation Polygon")
                    {
                        Visibility = false,
                        StyleUrl = style.AdjNavStyle.StyleUrl,
                        Open = false
                    };
                    KmlFolder fUnAdjNav = new KmlFolder("UnAdjNav", "UnAdjusted Navigation Polygon")
                    {
                        Visibility = false,
                        StyleUrl = style.UnAdjNavStyle.StyleUrl,
                        Open = false
                    };
                    KmlFolder fMiscPoints = new KmlFolder("Misc", "Misc Points")
                    {
                        Visibility = false,
                        StyleUrl = style.UnAdjMiscStyle.StyleUrl,
                        Open = false
                    };
                    KmlFolder fWayPoints = new KmlFolder("Waypoints", "Waypoints")
                    {
                        Visibility = false,
                        StyleUrl = style.WayPtsStyle.StyleUrl,
                        Open = false
                    };
                    #endregion


                    #region Create Folders for Bound, Nav, and Misc
                    KmlFolder fAdjBoundPoints = new KmlFolder("Points", "Adjusted Boundary Points")
                    {
                        Visibility = true,
                        Open = true
                    };
                    KmlFolder fUnAdjBoundPoints = new KmlFolder("Points", "UnAdjusted Boundary Points")
                    {
                        Visibility = false,
                        Open = false
                    };
                    KmlFolder fAdjNavPoints = new KmlFolder("Points", "Adjusted Navigation Points")
                    {
                        Visibility = false,
                        Open = false
                    };
                    KmlFolder fUnAdjNavPoints = new KmlFolder("Points", "UnAdjusted Navigation Points")
                    {
                        Visibility = false,
                        Open = false
                    };
                    KmlFolder fAdjMiscPoints = new KmlFolder("Adj Points", "Adjusted Misc Points")
                    {
                        Visibility = false,
                        Open = false
                    };
                    KmlFolder fUnAdjMiscPoints = new KmlFolder("UnAdj Points", "UnAdjusted Misc Points")
                    {
                        Visibility = false,
                        Open = false
                    };
                    #endregion


                    #region Create Geometry
                    Polygon AdjBoundPoly = new Polygon("AdjBoundPoly")
                    {
                        AltMode = AltitudeMode.ClampToGround,
                        IsPath = false
                    };
                    Polygon UnAdjBoundPoly = new Polygon("UnAdjBoundPoly")
                    {
                        AltMode = AltitudeMode.ClampToGround,
                        IsPath = false
                    };
                    List<Coordinates> AdjBoundPointList = new List<Coordinates>();
                    List<Coordinates> UnAdjBoundPointList = new List<Coordinates>();

                    Polygon AdjNavPoly = new Polygon("AdjNavPoly")
                    {
                        AltMode = AltitudeMode.ClampToGround,
                        IsPath = true
                    };
                    Polygon UnAdjNavPoly = new Polygon("UnAdjNavPoly")
                    {
                        AltMode = AltitudeMode.ClampToGround,
                        IsPath = true
                    };
                    List<Coordinates> AdjNavPointList = new List<Coordinates>();
                    List<Coordinates> UnAdjNavPointList = new List<Coordinates>();
                    #endregion


                    #region Add Placemarks
                    foreach (TtPoint point in points)
                    {
                        #region Create Placemarks
                        Point pos = point.GetLatLon();
                        Coordinates adjCoords = new Coordinates(pos.Y, pos.X);

                        Coordinates unadjCoords;

                        if (point.IsGpsAtBase())
                            unadjCoords = adjCoords;
                        else
                        {
                            pos = point.GetLatLon(false);
                            unadjCoords = new Coordinates(pos.Y, pos.X);
                        }

                        string snippit = $"Point Operation: {point.OpType}";

                        Placemark adjPm = new Placemark(point.PID.ToString(),
                            $"Point Operation: {point.OpType}<br><div>\t     Adjusted<br>UtmX: {point.AdjX}<br>UtmY: {point.AdjY}</div><br>{point.Comment}")
                        {
                            View = new View()
                            {
                                TimeStamp = point.TimeCreated,
                                Coordinates = adjCoords,
                                Tilt = 15,
                                AltMode = AltitudeMode.ClampToGround,
                                Range = 150
                            },
                            Snippit = snippit,
                            StyleUrl = style.AdjBndStyle.StyleUrl,
                            Open = false,
                            Visibility = true
                        };

                        adjPm.Points.Add(new KmlPoint(adjCoords));


                        Placemark unadjPm = new Placemark(point.PID.ToString(),
                            $"Point Operation: {point.OpType}<br><div>\t     Unadjusted<br>UtmX: {point.UnAdjX}<br>UtmY: {point.UnAdjY}</div><br>{point.Comment}")
                        {
                            View = new View()
                            {
                                TimeStamp = point.TimeCreated,
                                Coordinates = adjCoords,
                                Tilt = 15,
                                AltMode = AltitudeMode.ClampToGround,
                                Range = 150
                            },
                            Snippit = snippit,
                            StyleUrl = style.AdjBndStyle.StyleUrl,
                            Open = false,
                            Visibility = false
                        };

                        unadjPm.Points.Add(new KmlPoint(unadjCoords));
                        #endregion

                        #region Add Placemarks to Lists
                        if (point.OnBoundary)
                        {
                            AdjBoundPointList.Add(adjCoords);
                            UnAdjBoundPointList.Add(unadjCoords);
                            fAdjBoundPoints.Placemarks.Add(adjPm);
                            fUnAdjBoundPoints.Placemarks.Add(unadjPm);
                        }

                        if (point.IsNavPoint())
                        {
                            adjPm = new Placemark(adjPm);
                            adjPm.StyleUrl = string.Copy(style.AdjNavStyle.StyleUrl);
                            adjPm.Visibility = false;

                            unadjPm = new Placemark(unadjPm);
                            unadjPm.StyleUrl = string.Copy(style.UnAdjNavStyle.StyleUrl);

                            AdjNavPointList.Add(adjCoords);
                            UnAdjNavPointList.Add(unadjCoords);
                            fAdjNavPoints.Placemarks.Add(adjPm);
                            fUnAdjNavPoints.Placemarks.Add(unadjPm);
                        }

                        if (point.IsWayPointAtBase())
                        {
                            unadjPm = new Placemark(unadjPm);
                            unadjPm.StyleUrl = style.WayPtsStyle.StyleUrl;
                            fWayPoints.Placemarks.Add(unadjPm);
                        }
                        #endregion
                    }

                    #region Create Poly Placemarks
                    if (AdjBoundPointList.Count > 2 && AdjBoundPointList[0] != AdjBoundPointList[AdjBoundPointList.Count - 1])
                        AdjBoundPointList.Add(AdjBoundPointList[0]);

                    if (UnAdjBoundPointList.Count > 2 && UnAdjBoundPointList[0] != UnAdjBoundPointList[UnAdjBoundPointList.Count - 1])
                        UnAdjBoundPointList.Add(UnAdjBoundPointList[0]);

                    AdjBoundPoly.OuterBoundary = AdjBoundPointList;
                    UnAdjBoundPoly.OuterBoundary = UnAdjBoundPointList;

                    AdjNavPoly.OuterBoundary = AdjNavPointList;
                    UnAdjNavPoly.OuterBoundary = UnAdjNavPointList;

                    TimeSpan pointTimespan = points.Count > 0 ? TtUtils.GetPolyCreationPeriod(points) : new TimeSpan();

                    CoordinalExtent adjExtent = AdjBoundPoly.GetOuterDimensions();
                    CoordinalExtent unadjExtent = UnAdjBoundPoly.GetOuterDimensions();

                    double adjRange = 1000;
                    double unAdjRange = 1000;

                    if (adjExtent != null)
                    {
                        double height = MathEx.Distance(0, adjExtent.North, 0, adjExtent.South);
                        double width = MathEx.Distance(adjExtent.East, 0, adjExtent.West, 0);

                        adjRange = (width > height ? width : height) * 1.1;
                    }

                    if (unadjExtent != null)
                    {
                        double height = MathEx.Distance(0, unadjExtent.North, 0, unadjExtent.South);
                        double width = MathEx.Distance(unadjExtent.East, 0, unadjExtent.West, 0);

                        unAdjRange = (width > height ? width : height) * 1.1;
                    }

                    //AdjBoundPlacemark
                    Placemark AdjBoundPlacemark = new Placemark("AdjBoundPoly", "Adjusted Boundary Polygon")
                    {
                        View = AdjBoundPoly.HasOuterBoundary ? new View()
                        {
                            AltMode = AltitudeMode.ClampToGround,
                            Coordinates = AdjBoundPoly.GetOBAveragedCoords(),
                            Range = adjRange,
                            Tilt = 5,
                            TimeSpan = pointTimespan
                        } : null,
                        Snippit = poly.Description,
                        Open = false,
                        Visibility = true,
                        StyleUrl = style.AdjBndStyle.StyleUrl
                    };

                    AdjBoundPlacemark.Polygons.Add(AdjBoundPoly);

                    //UnAdjBoundPlacemark
                    Placemark UnAdjBoundPlacemark = new Placemark("UnAdjBoundPoly", "UnAdjusted Boundary Polygon")
                    {
                        View = UnAdjBoundPoly.HasOuterBoundary ? new View()
                        {
                            AltMode = AltitudeMode.ClampToGround,
                            Coordinates = UnAdjBoundPoly.GetOBAveragedCoords(),
                            Range = unAdjRange,
                            Tilt = 5,
                            TimeSpan = pointTimespan
                        } : null,
                        Snippit = poly.Description,
                        Open = false,
                        Visibility = false,
                        StyleUrl = style.UnAdjBndStyle.StyleUrl
                    };

                    UnAdjBoundPlacemark.Polygons.Add(UnAdjBoundPoly);

                    //AdjNavPlacemark
                    Placemark AdjNavPlacemark = new Placemark("AdjNavPoly", "Adjusted Navigation Path")
                    {
                        View = AdjNavPoly.HasOuterBoundary ? new View()
                        {
                            AltMode = AltitudeMode.ClampToGround,
                            Coordinates = AdjNavPoly.GetOBAveragedCoords(),
                            Range = adjRange,
                            Tilt = 5,
                            TimeSpan = pointTimespan
                        } : null,
                        Snippit = poly.Description,
                        Open = false,
                        Visibility = false,
                        StyleUrl = style.AdjNavStyle.StyleUrl
                    };

                    AdjNavPlacemark.Polygons.Add(AdjNavPoly);

                    //UnAdjNavPlacemark
                    Placemark UnAdjNavPlacemark = new Placemark("UnAdjNavPoly", "UnAdjusted Navigation Path")
                    {
                        View = UnAdjNavPoly.HasOuterBoundary ? new View()
                        {
                            AltMode = AltitudeMode.ClampToGround,
                            Coordinates = UnAdjNavPoly.GetOBAveragedCoords(),
                            Range = unAdjRange,
                            Tilt = 5,
                            TimeSpan = pointTimespan
                        } : null,
                        Snippit = poly.Description,
                        Open = false,
                        Visibility = false,
                        StyleUrl = style.UnAdjNavStyle.StyleUrl
                    };

                    UnAdjNavPlacemark.Polygons.Add(UnAdjNavPoly);

                    //add placemarks
                    fAdjBound.Placemarks.Add(AdjBoundPlacemark);
                    fUnAdjBound.Placemarks.Add(UnAdjBoundPlacemark);
                    fAdjNav.Placemarks.Add(AdjNavPlacemark);
                    fUnAdjNav.Placemarks.Add(UnAdjNavPlacemark);

                    #endregion
                    #endregion
                    

                    #region Add Folders To eachother
                    //added point folders to bound/nav/misc folders
                    fAdjBound.SubFolders.Add(fAdjBoundPoints);
                    fUnAdjBound.SubFolders.Add(fUnAdjBoundPoints);
                    fAdjNav.SubFolders.Add(fAdjNavPoints);
                    fUnAdjNav.SubFolders.Add(fUnAdjNavPoints);
                    fMiscPoints.SubFolders.Add(fAdjMiscPoints);
                    fMiscPoints.SubFolders.Add(fUnAdjMiscPoints);

                    //add bound/nav/misc/way folders to root polygon folder
                    polyFolder.SubFolders.Add(fAdjBound);
                    polyFolder.SubFolders.Add(fUnAdjBound);
                    polyFolder.SubFolders.Add(fAdjNav);
                    polyFolder.SubFolders.Add(fUnAdjNav);
                    polyFolder.SubFolders.Add(fMiscPoints);
                    polyFolder.SubFolders.Add(fWayPoints);

                    //add polygon root to KmlDoc
                    doc.SubFolders.Add(polyFolder);
                    #endregion
                }
            }


            doc.Styles.AddRange(styles.SelectMany(s => new Style[] { s.Value.Item1, s.Value.Item2 }));
            doc.StyleMaps.AddRange(styleMaps.Values);

            return doc;
        }


        private static StyleMap GetStyle(Dictionary<Color, StyleMap> styleMaps, Dictionary<Color, Tuple<Style, Style>> styles, Color color, bool adjsuted)
        {
            if (styleMaps.ContainsKey(color))
            {
                return styleMaps[color];
            }
            else
            {
                KmlColor c = new KmlColor(color.R, color.G, color.B, color.A);
                Style s = new Style($"style_{styles.Count}");
                s.SetColorsILP(c);
                s.IconScale = 1;
                s.IconColor = c;
                s.IconColorMode = ColorMode.Normal;
                s.LineWidth = adjsuted ? AdjLineSize : UnAdjLineSize;
                s.LineLabelVisibility = adjsuted;
                s.PolygonFill = false;
                s.PolygonOutline = true;
                s.BalloonDisplayMode = DisplayMode.Default;

                Style hs = new Style($"styleH_{styles.Count}", s);
                hs.IconScale = 1.1;
                hs.IconColor = new KmlColor(255, 255, 255, 255);

                Tuple<Style, Style> sp = Tuple.Create(s, hs);
                styles.Add(color, sp);


                StyleMap map = new StyleMap($"styleMap_{styleMaps.Count}", s.StyleUrl, hs.StyleUrl);

                styleMaps.Add(color, map);

                return map;
            }
        }


        private class PolygonStyle
        {
            public PolygonGraphicOptions Key { get; }

            public StyleMap AdjBndStyle { get; }
            public StyleMap UnAdjBndStyle { get; }
            public StyleMap AdjNavStyle { get; }
            public StyleMap UnAdjNavStyle { get; }
            public StyleMap UnAdjMiscStyle { get; }
            public StyleMap WayPtsStyle { get; }

            public PolygonStyle(PolygonGraphicOptions baseStyle,
                StyleMap adjBnd, StyleMap unadjBnd,
                StyleMap adjNav, StyleMap unadjNav,
                StyleMap unadjMisc, StyleMap waypts)
            {
                Key = baseStyle;

                AdjBndStyle = adjBnd;
                UnAdjBndStyle = unadjBnd;
                AdjNavStyle = adjNav;
                UnAdjNavStyle = unadjNav;
                UnAdjMiscStyle = unadjMisc;
                WayPtsStyle = waypts;
            }
        }
    }
}
