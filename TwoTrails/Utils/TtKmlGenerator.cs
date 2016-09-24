using FMSC.Core.Xml.KML;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwoTrails.Core;
using TwoTrails.Core.Points;
using TwoTrails.DAL;

namespace TwoTrails.Utils
{
    public static class TtKmlGenerator
    {
        const int AdjLineSize = 5;
        const int UnAdjLineSize = 7;
        
        public static KmlDocument Generate(IEnumerable<ITtManager> managers, string name, string description = null)
        {
            KmlDocument doc = new KmlDocument(name, description ?? String.Empty);

            List<PolygonStyle> polyStyles = new List<PolygonStyle>();
            Dictionary<int, Tuple<Style, Style>> styles = new Dictionary<int, Tuple<Style, Style>>();
            Dictionary<int, StyleMap> styleMaps = new Dictionary<int, StyleMap>();
            
            List<PolygonGraphicOptions> pgos = new List<PolygonGraphicOptions>();

            foreach (ITtManager manager in managers)
            {
                Dictionary<string, TtMetadata> metadata = manager.GetMetadata().ToDictionary(m => m.CN, m => m);
                pgos.AddRange(manager.GetPolygonGraphicOptions().Where(p => !pgos.Any(ip => ip.CN == p.CN)));

                foreach (TtPolygon poly in manager.GetPolygons())
                {
                    PolygonGraphicOptions pgo = pgos.FirstOrDefault(p => p.CN == poly.CN);
                    PolygonStyle style = pgo != null ? polyStyles.FirstOrDefault(ps => ps.Key == pgo) : null;

                    //Get/Generate Polygon Style
                    if (style == null)
                    {
                        if (pgo == null)
                        {
                            //pgo = default
                        }

                        style = new PolygonStyle(pgo,
                            GetStyle(styleMaps, styles, pgo.AdjBndColor, true),
                            GetStyle(styleMaps, styles, pgo.UnAdjBndColor, false),
                            GetStyle(styleMaps, styles, pgo.AdjNavColor, true),
                            GetStyle(styleMaps, styles, pgo.UnAdjNavColor, false),
                            GetStyle(styleMaps, styles, pgo.UnAdjPtsColor, false),
                            GetStyle(styleMaps, styles, pgo.WayPtsColor, false));

                        polyStyles.Add(style);
                    }


                    List<TtPoint> points = manager.GetPoints(poly.CN);

                    #region Create Poly Folder and Point Folders
                    Folder polyFolder = new Folder(poly.Name, poly.Description)
                    {

                        Open = false,
                        Visibility = true,
                        Properties = new FMSC.Core.Xml.KML.Properties()
                        {
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
                        }
                    };

                    Folder fAdjBound = new Folder("AdjBound", "Adjusted Boundary Polygon")
                    {
                        Visibility = true,
                        StyleUrl = style.AdjBndStyle.StyleUrl,
                        Open = false
                    };
                    Folder fUnAdjBound = new Folder("UnAdjBound", "UnAdjusted Boundary Polygon")
                    {
                        Visibility = false,
                        StyleUrl = style.UnAdjBndStyle.StyleUrl,
                        Open = false
                    };
                    Folder fAdjNav = new Folder("AdjNav", "Adjusted Navigation Polygon")
                    {
                        Visibility = false,
                        StyleUrl = style.AdjNavStyle.StyleUrl,
                        Open = false
                    };
                    Folder fUnAdjNav = new Folder("UnAdjNav", "UnAdjusted Navigation Polygon")
                    {
                        Visibility = false,
                        StyleUrl = style.UnAdjNavStyle.StyleUrl,
                        Open = false
                    };
                    Folder fMiscPoints = new Folder("Misc", "Misc Points")
                    {
                        Visibility = false,
                        StyleUrl = style.UnAdjMiscStyle.StyleUrl,
                        Open = false
                    };
                    Folder fWayPoints = new Folder("Waypoints", "Waypoints")
                    {
                        Visibility = false,
                        StyleUrl = style.WayPtsStyle.StyleUrl,
                        Open = false
                    };
                    #endregion


                    #region Create Folders for Bound, Nav, and Misc
                    Folder fAdjBoundPoints = new Folder("Points", "Adjusted Boundary Points")
                    {
                        Visibility = true,
                        Open = true
                    };
                    Folder fUnAdjBoundPoints = new Folder("Points", "UnAdjusted Boundary Points")
                    {
                        Visibility = false,
                        Open = false
                    };
                    Folder fAdjNavPoints = new Folder("Points", "Adjusted Navigation Points")
                    {
                        Visibility = false,
                        Open = false
                    };
                    Folder fUnAdjNavPoints = new Folder("Points", "UnAdjusted Navigation Points")
                    {
                        Visibility = false,
                        Open = false
                    };
                    Folder fAdjMiscPoints = new Folder("Adj Points", "Adjusted Misc Points")
                    {
                        Visibility = false,
                        Open = false
                    };
                    Folder fUnAdjMiscPoints = new Folder("UnAdj Points", "UnAdjusted Misc Points")
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

                    #endregion
                }
            }


            doc.Styles.AddRange(styles.SelectMany(s => new Style[] { s.Value.Item1, s.Value.Item2 }));
            doc.StyleMaps.AddRange(styleMaps.Values);

            return doc;
        }


        private static StyleMap GetStyle(Dictionary<int, StyleMap> styleMaps, Dictionary<int, Tuple<Style, Style>> styles, int color, bool adjsuted)
        {
            if (styleMaps.ContainsKey(color))
            {
                return styleMaps[color];
            }
            else
            {
                Color c = new Color(color);
                Style s = new Style(String.Format("style_{0}", styles.Count));
                s.SetColorsILP(c);
                s.IconScale = 1;
                s.IconColor = c;
                s.IconColorMode = ColorMode.Normal;
                s.LineWidth = adjsuted ? AdjLineSize : UnAdjLineSize;
                s.LineLabelVisibility = adjsuted;
                s.PolygonFill = false;
                s.PolygonOutline = true;
                s.BalloonDisplayMode = DisplayMode.Default;

                Style hs = new Style(String.Format("styleH_{0}", styles.Count), s);
                hs.IconScale = 1.1;
                hs.IconColor = new Color(255, 255, 255, 255);

                Tuple<Style, Style> sp = Tuple.Create(s, hs);
                styles.Add(color, sp);


                StyleMap map = new StyleMap(String.Format("styleMap_{0}", styleMaps.Count), s.StyleUrl, hs.StyleUrl);

                styleMaps.Add(color, map);

                return map;
            }
        }


        public class PolygonStyle
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
