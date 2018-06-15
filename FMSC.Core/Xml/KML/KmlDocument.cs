using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Xml;

namespace FMSC.Core.Xml.KML
{
    public class KmlDocument : KmlFolder
    {
        private const string ATTR_ID = "id";
        private const string ATTR_X = "x";
        private const string ATTR_Y = "y";
        private const string ATTR_XU = "xunits";
        private const string ATTR_YU = "yunits";


        private const string VAL_ICON = "icon";
        private const string VAL_COLOR = "color";
        private const string VAL_COLOR_MODE = "colormode";
        private const string VAL_SCALE = "scale";
        private const string VAL_HOTSPOT = "hotspot";
        private const string VAL_HEADING = "heading";
        private const string VAL_WIDTH = "width";
        private const string VAL_HEIGHT = "height";
        private const string VAL_VISIBILITY = "visibility";
        private const string VAL_OUTER_WIDTH = "gx:outerwidth";
        private const string VAL_OUTER_HEIGHT = "gx:outerheight";
        private const string VAL_OUTER_COLOR = "gx:outercolor";
        private const string VAL_PHYSICAL_WIDTH = "gx:outerphysicalwidth";
        private const string VAL_OUTLINE = "outline";
        private const string VAL_FILL = "fill";
        private const string VAL_BGCOLOR = "bgcolor";
        private const string VAL_TEXT_COLOR = "textcolor";
        private const string VAL_TEXT = "text";
        private const string VAL_STATE = "state";
        private const string VAL_DISPLAY_MODE = "displaymode";
        private const string VAL_LIST_ITEM_TYPE = "listitemtype";
        private const string VAL_ITEM_ICON = "itemicon";
        private const string VAL_HREF = "href";

        #region Properties
        public List<Style> Styles { get; } = new List<Style>();
        public List<StyleMap> StyleMaps { get; } = new List<StyleMap>();
        #endregion
        

        public KmlDocument(string name = null, string desc = null) :
            base(name ?? String.Empty, desc ?? String.Empty) { }
       
        public KmlDocument(KmlFolder doc) : base(doc) { }

        public KmlDocument(KmlDocument doc) : base(doc)
        {
            Styles.AddRange(doc.Styles);
            StyleMaps.AddRange(doc.StyleMaps);
        }


        public void RemoveStyleById(string id)
        {
            for (int i = Styles.Count - 1; i > -1; i--)
            {
                if (Styles[i].ID == id)
                {
                    Styles.RemoveAt(i);
                    return;
                }
            }
        }

        public Style GetStyleById(string id) => Styles.FirstOrDefault(s => s.ID == id);
        
        public void RemoveStyleMapById(string id)
        {
            for (int i = StyleMaps.Count - 1; i > -1; i--)
            {
                if (StyleMaps[i].ID == id)
                {
                    StyleMaps.RemoveAt(i);
                    return;
                }
            }
        }

        public StyleMap GetStyleMapById(string id) => StyleMaps.FirstOrDefault(s => s.ID == id);



        public static KmlDocument Load(String path)
        {
            XmlDocument doc = new XmlDocument();
            
            if (path.EndsWith(".kmz"))
            {
                string tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

                if (Directory.Exists(tempDir))
                    Directory.Delete(tempDir, true);

                Directory.CreateDirectory(tempDir);

                ZipFile.ExtractToDirectory(path, tempDir);

                foreach (string file in Directory.GetFiles(tempDir))
                {
                    if (file.EndsWith(".kml"))
                    {
                        path = file;
                        break;
                    }
                }

                if (!path.EndsWith(".kml"))
                    throw new Exception("Unable to extract KML from KMZ");

                doc.Load(path);

                Directory.Delete(tempDir, true);
            }
            else
                doc.Load(path);

            return Load(doc);
        }

        public static KmlDocument LoadXml(String xml)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);

            return Load(doc);
        }

        public static KmlDocument Load(XmlDocument xmlDocument)
        {
            KmlDocument kdoc = null;
            XmlNodeList list = xmlDocument.GetElementsByTagName("Document");

            if (list.Count > 0)
            {
                XmlNode head = list[0];
                kdoc = new KmlDocument(ParseFolder(head));
                
                foreach (XmlNode node in head)
                {
                    string name = node.Name.ToLower();
                    switch (name)
                    {
                        case "style": kdoc.Styles.Add(ParseStyle(node)); break;
                        case "stylemap": kdoc.StyleMaps.Add(ParseStyleMap(node)); break;
                    }
                }
            }

            return kdoc;
        }


        private static KmlFolder ParseFolder(XmlNode xnode)
        {
            KmlFolder folder = new KmlFolder();

            foreach (XmlNode node in xnode.ChildNodes)
            {
                string name = node.Name.ToLower();
                switch (name)
                {
                    case "folder": folder.SubFolders.Add(ParseFolder(node)); break;
                    case "placemark": folder.Placemarks.Add(ParsePlacemark(node)); break;
                    default: ParseProperties(folder, name, node); break;
                }
            }

            return folder;
        }

        private static ExtendedData ParseExtendedData(XmlNode xnode)
        {
            ExtendedData data = new ExtendedData();

            foreach (XmlNode node in xnode.ChildNodes)
            {
                string name = node.Attributes["name"]?.FirstChild.Value;
                if (name != null)
                {
                    data.AddData(new ExtendedData.Data(name, node.FirstChild.Value));
                }
            }

            return data;
        }

        private static Placemark ParsePlacemark(XmlNode xnode)
        {
            Placemark placemark = new Placemark(xnode.Attributes[ATTR_ID]?.FirstChild.Value);

            foreach (XmlNode node in xnode.ChildNodes)
            {
                string name = node.Name.ToLower();
                switch (name)
                {
                    case "lookat": placemark.View = ParseView(node); break;
                    case "point":
                        {
                            KmlPoint point = ParsePoint(node);
                            if (point != null) placemark.Points.Add(point);
                            break;
                        }
                    case "polygon":
                    case "linestring":
                        {
                            Polygon poly = ParsePolygon(node);
                            if (poly != null) placemark.Polygons.Add(poly);
                            break;
                        }
                    case "multigeometry":
                        {
                            foreach (XmlNode inode in node.ChildNodes)
                            {
                                switch (inode.Name.ToLower())
                                {
                                    case "point":
                                        {
                                            KmlPoint point = ParsePoint(node);
                                            if (point != null) placemark.Points.Add(point);
                                            break;
                                        }
                                    case "polygon":
                                    case "linestring":
                                        {
                                            Polygon poly = ParsePolygon(node);
                                            if (poly != null) placemark.Polygons.Add(poly);
                                            break;
                                        }
                                }
                            }
                            break;
                        }
                    default: ParseProperties(placemark, name, node); break;
                }
            }

            return placemark;
        }

        private static Polygon ParsePolygon(XmlNode xnode)

        {
            Polygon poly = new Polygon(xnode.Attributes[ATTR_ID]?.FirstChild.Value);
            poly.IsPath = xnode.Name.Equals("linestring", StringComparison.InvariantCultureIgnoreCase);
            
            Func<string, List<Coordinates>> parseCoods = (coordsStr) =>
            {
                if (coordsStr == null)
                    return null;
                string[] coordVals = coordsStr.Split(new char[] { '\n', '\r', ' ' }, StringSplitOptions.RemoveEmptyEntries);

                List<Coordinates> coords = new List<Coordinates>();

                foreach (string c in coordVals)
                {
                    string[] v = c.Split(',').Select(y => y.Trim()).ToArray();

                    if (v.Length > 1)
                    {
                        if (Double.TryParse(v[0], out double lon))
                        {
                            if (Double.TryParse(v[1], out double lat))
                            {
                                if (v.Length > 2 && double.TryParse(v[2], out double alt))
                                    coords.Add(new Coordinates(lat, lon, alt));
                                else
                                    coords.Add(new Coordinates(lat, lon));
                            }
                        }
                    }
                }

                return coords;
            };

            foreach (XmlNode node in xnode.ChildNodes)
            {
                switch (node.Name.ToLower())
                {
                    case "tessellate":
                        if (Int32.TryParse(node.FirstChild.Value, out int tes))
                            poly.Tessellate = tes;
                        break;
                    case "extrude":
                        if (Int32.TryParse(node.FirstChild.Value, out int ext))
                            poly.Extrude = ext;
                        break;
                    case "altitudeMode":
                        if (Enum.TryParse(node.FirstChild.Value, out AltitudeMode mode))
                            poly.AltMode = mode;
                        break;
                    case "outerboundaryis": poly.OuterBoundary = parseCoods(node.FirstChild?.FirstChild?.FirstChild?.Value); break;
                    case "innerboundaryis": poly.InnerBoundary = parseCoods(node.FirstChild?.FirstChild?.FirstChild?.Value); break;
                }
            }

            return poly;
        }

        private static KmlPoint ParsePoint(XmlNode xnode)
        {
            KmlPoint point = new KmlPoint();
            if (xnode.Attributes.Count > 0)
                point.Name = xnode.Attributes[ATTR_ID]?.FirstChild.Value;

            foreach (XmlNode node in xnode.ChildNodes)
            {
                switch (node.Name.ToLower())
                {
                    case "extrude":
                        if (Int32.TryParse(node.FirstChild.Value, out int ext))
                            point.Extrude = ext;
                        break;
                    case "altitudeMode":
                        if (Enum.TryParse(node.FirstChild.Value, out AltitudeMode mode))
                            point.AltMode = mode;
                        break;
                    case "coordinates":
                        {
                            string[] c = node.FirstChild.Value.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                            if (c.Length > 0)
                            {
                                string[] v = c[0].Split(',').Select(y => y.Trim()).ToArray();
                                if (v.Length > 1)
                                {
                                    if (Double.TryParse(v[0], out double lon))
                                    {
                                        if (Double.TryParse(v[1], out double lat))
                                        {
                                            if (v.Length > 2 && double.TryParse(v[2], out double alt))
                                                point.Coordinates = new Coordinates(lat, lon, alt);
                                            else
                                                point.Coordinates = new Coordinates(lat, lon);
                                        }
                                    }
                                }
                            }
                            break;
                        }
                }
            }

            return point;
        }

        private static Style ParseStyle(XmlNode xnode)
        {
            Style style = new Style(xnode.Attributes[ATTR_ID]?.FirstChild.Value);
            
            foreach (XmlNode node in xnode.ChildNodes)
            {
                switch (node.Name.ToLower())
                {
                    #region IconStyle
                    case "iconstyle":
                        {
                            style.IconID = node.Attributes[ATTR_ID]?.FirstChild.Value;

                            foreach (XmlNode inode in node.ChildNodes)
                            {
                                switch (inode.Name.ToLower())
                                {
                                    case VAL_ICON: style.IconUrl = inode.ChildNodes[0]?.FirstChild.Value; break;
                                    case VAL_COLOR: style.IconColor = new KmlColor(inode.FirstChild.Value, true); break;
                                    case VAL_COLOR_MODE:style.IconColorMode = (ColorMode)Enum.Parse(typeof(ColorMode), inode.FirstChild.Value, true); break;
                                    case VAL_SCALE: style.IconScale = double.Parse(inode.FirstChild.Value); break;
                                    case VAL_HEADING: style.IconHeading = double.Parse(inode.FirstChild.Value); break;
                                    case VAL_HOTSPOT:
                                        {
                                            if (double.TryParse(inode.Attributes[ATTR_X]?.FirstChild.Value, out double x))
                                            {
                                                if (double.TryParse(inode.Attributes[ATTR_Y]?.FirstChild.Value, out double y))
                                                {
                                                    if (Enum.TryParse(inode.Attributes[ATTR_XU]?.FirstChild.Value, out XYUnitType xu))
                                                    {
                                                        if (Enum.TryParse(inode.Attributes[ATTR_YU]?.FirstChild.Value, out XYUnitType yu))
                                                        {
                                                            style.IconHotSpot = new HotSpot(x, y, xu, yu);
                                                        }
                                                    }
                                                }
                                            }
                                            break;
                                        }
                                }
                            }
                        }
                        break;
                    #endregion
                    #region LabelStyle
                    case "labelstyle":
                        {
                            style.LabelID = node.Attributes[ATTR_ID]?.FirstChild.Value;

                            foreach (XmlNode inode in node.ChildNodes)
                            {
                                switch (inode.Name.ToLower())
                                {
                                    case VAL_COLOR: style.LabelColor = new KmlColor(inode.FirstChild.Value, true); break;
                                    case VAL_COLOR_MODE: style.LabelColorMode = (ColorMode)Enum.Parse(typeof(ColorMode), inode.FirstChild.Value, true); break;
                                    case VAL_SCALE: style.LabelScale = double.Parse(inode.FirstChild.Value); break;
                                }
                            }
                        }
                        break;
                    #endregion
                    #region LineStyle
                    case "linestyle":
                        {
                            style.LineID = node.Attributes[ATTR_ID]?.FirstChild.Value;

                            foreach (XmlNode inode in node.ChildNodes)
                            {
                                switch (inode.Name.ToLower())
                                {
                                    case VAL_COLOR: style.LineColor = new KmlColor(inode.FirstChild.Value, true); break;
                                    case VAL_COLOR_MODE: style.LineColorMode = (ColorMode)Enum.Parse(typeof(ColorMode), inode.FirstChild.Value, true); break;
                                    case VAL_WIDTH: style.LineWidth = double.Parse(inode.FirstChild.Value); break;
                                    case VAL_OUTER_COLOR: style.LineColorMode = (ColorMode)Enum.Parse(typeof(ColorMode), inode.FirstChild.Value, true); break;
                                    case VAL_OUTER_WIDTH: style.LineOuterWidth = double.Parse(inode.FirstChild.Value); break;
                                    case VAL_PHYSICAL_WIDTH: style.LinePhysicalWidth = double.Parse(inode.FirstChild.Value); break;
                                    case VAL_VISIBILITY: style.LineLabelVisibility = bool.Parse(inode.FirstChild.Value); break; 
                                }
                            }
                        }
                        break;
                    #endregion
                    #region PolygonStyle
                    case "polygonstyle":
                        {
                            style.PolygonID = node.Attributes[ATTR_ID]?.FirstChild.Value;

                            foreach (XmlNode inode in node.ChildNodes)
                            {
                                switch (inode.Name.ToLower())
                                {
                                    case VAL_COLOR: style.PolygonColor = new KmlColor(inode.FirstChild.Value, true); break;
                                    case VAL_COLOR_MODE: style.PolygonColorMode = (ColorMode)Enum.Parse(typeof(ColorMode), inode.FirstChild.Value, true); break;
                                    case VAL_FILL: style.PolygonFill = bool.Parse(inode.FirstChild.Value); break;
                                    case VAL_OUTLINE: style.PolygonOutline = bool.Parse(inode.FirstChild.Value); break;
                                }
                            }
                        }
                        break;
                    #endregion
                    #region BalloonStyle
                    case "balloonstyle":
                        {
                            style.BalloonID = node.Attributes[ATTR_ID]?.FirstChild.Value;

                            foreach (XmlNode inode in node.ChildNodes)
                            {
                                switch (inode.Name.ToLower())
                                {
                                    case VAL_BGCOLOR: style.BalloonBgColor = new KmlColor(inode.FirstChild.Value, true); break;
                                    case VAL_TEXT_COLOR: style.BalloonTextColor = new KmlColor(inode.FirstChild.Value, true); break;
                                    case VAL_TEXT: style.BalloonText = inode.FirstChild.Value; break;
                                    case VAL_DISPLAY_MODE: style.BalloonDisplayMode = (DisplayMode)Enum.Parse(typeof(DisplayMode), inode.FirstChild.Value, true); break;
                                }
                            }
                        }
                        break;
                    #endregion
                    #region ListStyle
                    case "liststyle":
                        {
                            style.ListID = node.Attributes[ATTR_ID]?.FirstChild.Value;

                            foreach (XmlNode inode in node.ChildNodes)
                            {
                                switch (inode.Name.ToLower())
                                {
                                    case VAL_LIST_ITEM_TYPE: style.ListListItemType = (ListItemType)Enum.Parse(typeof(ListItemType), inode.FirstChild.Value, true); break;
                                    case VAL_BGCOLOR: style.ListBgColor = new KmlColor(inode.FirstChild.Value, true); break;

                                    case VAL_ITEM_ICON:
                                        {
                                            foreach (XmlNode znode in inode.ChildNodes)
                                            {
                                                switch (znode.Name.ToLower())
                                                {
                                                    case VAL_STATE: style.ListItemState = (State)Enum.Parse(typeof(State), znode.FirstChild.Value, true); break;
                                                    case VAL_HREF: style.ListItemIconUrl = znode.ChildNodes[0]?.FirstChild.Value; break;
                                                }
                                            }
                                            break;
                                        }
                                }
                            }
                        }
                        break; 
                        #endregion
                }
            }

            return style;
        }

        private static StyleMap ParseStyleMap(XmlNode xnode)
        {
            StyleMap map = new StyleMap(xnode.Attributes[ATTR_ID]?.FirstChild.Value);

            foreach (XmlNode node in xnode.ChildNodes)
            {
                if (node.Name.ToLower() == "pair")
                {
                    bool isHighlight = false;
                    string url = null;

                    foreach (XmlNode inode in node.ChildNodes)
                    {
                        switch (inode.Name.ToLower())
                        {
                            case "key": isHighlight = inode.FirstChild.Value.ToLower() != "normal"; break;
                            case "styleurl": url = inode.FirstChild.Value; break;
                        }
                    }

                    if (url != null)
                    {
                        if (isHighlight)
                            map.HightLightedStyleUrl = url;
                        else
                            map.NormalStyleUrl = url;
                    }
                }
            }

            return map;
        }

        private static View ParseView(XmlNode xnode)
        {
            View view = new View();
            double? lat = null, lon = null, alt = null;

            foreach (XmlNode node in xnode.ChildNodes)
            {
                switch (node.Name.ToLower())
                {
                    case "latitude":
                        {
                            if (double.TryParse(node.FirstChild.Value, out double xlat))
                                lat = xlat;
                            break;
                        }
                    case "longitude":
                        {
                            if (double.TryParse(node.FirstChild.Value, out double xlon))
                                lon = xlon;
                            break;
                        }
                    case "altitude":
                        {
                            if (double.TryParse(node.FirstChild.Value, out double xalt))
                                alt = xalt;
                            break;
                        }
                    case "heading":
                        {
                            if (double.TryParse(node.FirstChild.Value, out double head))
                                view.Heading = head;
                            break;
                        }
                    case "tilt":
                        {
                            if (double.TryParse(node.FirstChild.Value, out double tilt))
                                view.Tilt = tilt;
                            break;
                        }
                    case "range":
                        {
                            if (double.TryParse(node.FirstChild.Value, out double range))
                                view.Range = range;
                            break;
                        }
                    case "altitudemode":
                    case "gx:altitudemode":
                        {
                            if (Enum.TryParse(node.FirstChild.Value, out AltitudeMode mode))
                                view.AltMode = mode;
                            break;
                        }
                    case "gx:timespan":
                        {
                            string name;
                            DateTime? start = null, end = null;
                            foreach (XmlNode inode in node.ChildNodes)
                            {
                                name = inode.Name.ToLower();
                                if (name == "begin")
                                {
                                    start = ParseKmlDate(inode.FirstChild.Value);
                                }
                                else if (name == "end")
                                {
                                    end = ParseKmlDate(inode.FirstChild.Value);
                                }
                            }

                            if (start != null && end != null)
                            {
                                view.StartTime = (DateTime)start;
                                view.TimeSpan = new TimeSpan(((DateTime)end).Subtract(view.StartTime).Ticks);
                            }
                            break;
                        }
                    case "gx:timestamp": view.TimeStamp = ParseKmlDate(node.FirstChild.Value); break;
                } 
            }

            if (lat != null && lon != null)
                view.Coordinates = new Coordinates((double)lat, (double)lon, alt);

            return view;
        }

        private static void ParseProperties(KmlProperties properties, string name, XmlNode xnode)
        {
            switch (name)
            {
                case "name": properties.Name = xnode.FirstChild.Value; break;
                case "description": properties.Desctription = ParseCData(xnode.FirstChild.Value); break;
                case "styleurl": properties.StyleUrl = xnode.FirstChild.Value; break;
                case "visibility":
                    if (Boolean.TryParse(xnode.FirstChild.Value, out bool vis))
                        properties.Visibility = vis;
                    break;
                case "open":
                    if (Boolean.TryParse(xnode.FirstChild.Value, out bool open))
                        properties.Open = open;
                    break;

                case "author": properties.Author = xnode.FirstChild.Value; break;
                case "link": properties.Link = xnode.FirstChild.Value; break;
                case "address": properties.Address = xnode.FirstChild.Value; break;
                case "snippit":
                    {
                        properties.Snippit = xnode.FirstChild.Value;
                        if (Int32.TryParse(xnode.Attributes["maxLines"]?.FirstChild.Value, out int ml))
                            properties.SnippitMaxLines = ml;
                        break;
                    }
                case "region": properties.Region = xnode.FirstChild.Value; break;
                case "extendeddata": properties.ExtendedData = ParseExtendedData(xnode); break;
            }
        }

        private static string ParseCData(string text)
        {
            if (String.IsNullOrWhiteSpace(text))
                return String.Empty;
            else if (text.StartsWith("<!"))
                return text.Remove(text.Length - 3, 3).Remove(0, 9);
            else
                return text;
        }

        private static DateTime? ParseKmlDate(string kmldate)
        {
            if (kmldate != null && DateTime.TryParseExact(kmldate, "yyyy-MM-ddTHH:mm:ssZ", CultureInfo.CurrentCulture, DateTimeStyles.None, out DateTime dt))
                return dt;
            return null;
        }
    }

}
