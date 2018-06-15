using System;
using System.Text;
using System.Xml;

namespace FMSC.Core.Xml.KML
{
    public class KmlWriter : XmlTextWriter
    {
        #region Members
        private string _file;
        #endregion

        #region Properties

        private bool _open;
        private string _FileName
        {
            get { return _file; }
            set
            {
                _file = value;
                _open = !String.IsNullOrEmpty(_file);
            }
        }
        #endregion

        
        public KmlWriter(string filename) : base(filename, Encoding.UTF8)
        {
            _FileName = filename;
            Formatting = Formatting.Indented;
            Indentation = 4;
        }

        public void WriteStartKml()
        {
            WriteStartElement("kml");
            WriteAttributeString("xmlns", "http://www.opengis.net/kml/2.2");
            WriteAttributeString("xmlns:gx", "http://www.google.com/kml/ext/2.2");
            _open = true;
        }

        public void WriteEndKml()
        {
            WriteEndElement();
            _open = false;
        }
        
        public void WriteKmlDocument(KmlDocument doc)
        {
            if (_open && doc != null)
            {
                //start document
                WriteStartElement("Document");

                //kml name
                WriteStartElement("name");
                WriteValue(doc.Name ?? String.Empty);
                WriteEndElement();

                //description
                WriteKmlDescription(doc.Desctription);

                if (doc.Visibility != null)
                    WriteElementString("visibility", ConvertBool((bool)doc.Visibility).ToString());
                if (doc.Open != null)
                    WriteElementString("open", ConvertBool((bool)doc.Open).ToString());

                WriteProperties(doc);

                foreach (Style style in doc.Styles)
                    WriteStyle(style);

                foreach (StyleMap map in doc.StyleMaps)
                    WriteStyleMap(map);

                foreach (KmlFolder subFolder in doc.SubFolders)
                    WriteKmlFolder(subFolder);

                foreach (Placemark pm in doc.Placemarks)
                    WritePlacemark(pm);

                //end document
                WriteEndElement();
            }
            else if (!_open)
            {
                throw new Exception("KmlStart not written.");
            }
            else
            {
                throw new Exception("Document is null.");
            }
        }

        public void WriteKmlFolder(KmlFolder folder)
        {
            if (folder != null)
            {
                WriteStartElement("Folder");
                WriteComment("Folder Guid: " + folder.CN);

                WriteElementString("name", folder.Name);
                WriteKmlDescription(folder.Desctription);

                if (folder.StyleUrl != null && folder.StyleUrl != "")
                    WriteElementString("styleUrl", folder.StyleUrl);

                if (folder.Visibility != null)
                    WriteElementString("visibility", ConvertBool((bool)folder.Visibility).ToString());

                if (folder.Open != null)
                    WriteElementString("open", ConvertBool((bool)folder.Open).ToString());

                WriteProperties(folder);

                foreach (KmlFolder subFolder in folder.SubFolders)
                    WriteKmlFolder(subFolder);

                foreach (Placemark pm in folder.Placemarks)
                    WritePlacemark(pm);

                //end folder
                WriteEndElement();
            }
        }

        public void WritePlacemark(Placemark pm)
        {
            if (pm != null)
            {
                WriteStartElement("Placemark");
                WriteComment("Placemark Guid: " + pm.CN);

                WriteElementString("name", pm.Name);

                WriteKmlDescription(pm.Desctription);

                if (pm.View != null && pm.View.Coordinates != null)
                    WriteView(pm.View);
                else
                {
                    View v = new View();

                    if (pm.Polygons != null && pm.Polygons.Count > 0)
                    {
                        Polygon poly = pm.Polygons[0];

                        if (poly.HasOuterBoundary)
                        {
                            v.Coordinates = poly.GetOBAveragedCoords();
                            v.AltMode = poly.AltMode;
                            WriteView(v);
                        }
                        else if (poly.HasInnerBoundary)
                        {
                            v.Coordinates = poly.GetIBAveragedCoords();
                            v.AltMode = poly.AltMode;
                            WriteView(v);
                        }
                    }
                    else if (pm.Points != null && pm.Points.Count > 0)
                    {
                        v.Coordinates = pm.Points[0].Coordinates;
                        v.AltMode = pm.Points[0]._AltMode;
                        WriteView(v);
                    }
                }

                if (pm.StyleUrl != null && pm.StyleUrl != "")
                    WriteElementString("styleUrl", pm.StyleUrl);

                if (pm.Visibility != null)
                    WriteElementString("visibility", ConvertBool((bool)pm.Visibility).ToString());
                if (pm.Open != null)
                    WriteElementString("open", ConvertBool((bool)pm.Open).ToString());

                WriteProperties(pm);


                if ((pm.Points.Count + pm.Polygons.Count) > 1)
                {
                    WriteStartElement("MultiGeometry");

                    foreach (Polygon poly in pm.Polygons)
                    {
                        WritePolygon(poly);
                    }
                    foreach (KmlPoint point in pm.Points)
                    {
                        WritePoint(point);
                    }

                    //end MultiGeo
                    WriteEndElement();
                }
                else
                {
                    if (pm.Polygons.Count > 0)
                    {
                        WritePolygon(pm.Polygons[0]);
                    }
                    else if (pm.Points.Count > 0)
                    {
                        WritePoint(pm.Points[0]);
                    }
                }

                //end placemark
                WriteEndElement();
            }
        }

        public void WritePolygon(Polygon poly)
        {
            try
            {
                if (poly != null)
                {
                    if (!poly.IsPath)
                        WriteStartElement("Polygon");
                    else
                        WriteStartElement("LineString");

                    if (poly.Name != null)
                        WriteAttributeString("id", poly.Name);
                    WriteComment("Poly Guid: " + poly.CN);

                    if (poly.Extrude == 1)
                        WriteElementString("extrude", poly.Extrude.ToString());
                    if (poly.Tessellate == 1)
                        WriteElementString("tessellate", poly.Tessellate.ToString());

                    if (poly.AltMode != null)
                    {
                        if (poly.AltMode == AltitudeMode.ClampToGround || poly.AltMode == AltitudeMode.RelativeToGround || poly.AltMode == AltitudeMode.Absolute)
                            WriteElementString("altitudeMode", poly.AltMode.ToString());
                        else
                            WriteElementString("gx:altitudeMode", poly.AltMode.ToString());
                    }

                    if (!poly.IsPath)
                    {
                        WriteStartElement("outerBoundaryIs");
                        WriteStartElement("LinearRing");
                        WriteStartElement("coordinates");

                        foreach (Coordinates c in poly.OuterBoundary)
                        {
                            StringBuilder sb = new StringBuilder();
                            sb.Append($"{c.Longitude},{c.Latitude}");
                            sb.Append(c.Altitude != null ? $",{c.Altitude} " : " ");
                            WriteValue(sb.ToString());
                        }

                        WriteEndElement();
                        WriteEndElement();
                        WriteEndElement();

                        if (poly.InnerBoundary != null && poly.InnerBoundary.Count > 0)
                        {
                            WriteStartElement("innerBoundaryIs");
                            WriteStartElement("LinearRing");
                            WriteStartElement("coordinates");

                            foreach (Coordinates c in poly.InnerBoundary)
                            {
                                StringBuilder sb = new StringBuilder();
                                sb.Append($"{c.Longitude},{c.Latitude}");
                                sb.Append(c.Altitude != null ? $",{c.Altitude} " : " ");
                                WriteValue(sb.ToString());
                            }

                            WriteEndElement();
                            WriteEndElement();
                            WriteEndElement();
                        }
                    }
                    else
                    {
                        WriteStartElement("coordinates");

                        foreach (Coordinates c in poly.OuterBoundary)
                        {
                            StringBuilder sb = new StringBuilder();
                            sb.Append($"{c.Longitude},{c.Latitude}");
                            sb.Append(c.Altitude != null ? $",{c.Altitude} " : " ");
                            WriteValue(sb.ToString());
                        }

                        WriteEndElement();
                    }

                    //end poly
                    WriteEndElement();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("WriteKmlPoly Error: " + ex.Message);
            }
        }

        public void WritePoint(KmlPoint point)
        {
            try
            {
                if (point != null)
                {
                    WriteStartElement("Point");

                    if (point.Name != null)
                        WriteAttributeString("id", point.Name);
                    WriteComment("Point Guid: " + point.CN);

                    if (point.Extrude != null)
                        WriteElementString("extrude", point.Extrude.ToString());

                    if (point.AltMode != null)
                    {
                        if (point.AltMode == AltitudeMode.ClampToGround || point.AltMode == AltitudeMode.RelativeToGround || point.AltMode == AltitudeMode.Absolute)
                            WriteElementString("altitudeMode", point.AltMode.ToString());
                        else
                            WriteElementString("gx:altitudeMode", point.AltMode.ToString());
                    }

                    WriteStartElement("coordinates");
                    StringBuilder sb = new StringBuilder();
                    sb.Append($"{point.Coordinates.Longitude},{point.Coordinates.Latitude}");
                    sb.Append(point.Coordinates.Altitude != null ? $",{point.Coordinates.Altitude} " : " ");
                    WriteValue(sb.ToString());
                    WriteEndElement();

                    //end poly
                    WriteEndElement();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("WritePoint Error: " + ex.Message);
            }
        }

        public void WriteStyle(Style style)
        {
            try
            {
                //start style
                WriteStartElement("Style");
                WriteAttributeString("id", style.ID);

                #region Icon Style
                if (style.IconUrl != null || style.IconColor != null || style.IconColorMode != null || style.IconScale != null ||
                    style.IconHeading != null || style.IconHotSpot != null)
                {
                    WriteStartElement("IconStyle");
                    if (style.IconID != null)
                        WriteAttributeString("id", style.IconID);

                    if (style.IconColor != null)
                        WriteElementString("color", style.IconColor.ToString());
                    if (!String.IsNullOrEmpty(style.IconUrl))
                    {
                        WriteStartElement("Icon");
                        WriteElementString("href", style.IconUrl);
                        WriteEndElement();
                    }
                    if (style.IconScale != null)
                        WriteElementString("scale", style.IconScale.ToString());
                    if (style.IconColorMode != null)
                        WriteElementString("colorMode", ConvertColorMode(style.IconColorMode));
                    if (style.IconHeading != null)
                        WriteElementString("heading", style.IconHeading.ToString());
                    if (style.IconHotSpot != null)
                    {
                        WriteStartElement("hotSpot");
                        WriteAttributeString("x", style.IconHotSpot.Value.X.ToString());
                        WriteAttributeString("y", style.IconHotSpot.Value.Y.ToString());
                        WriteAttributeString("xunits", ConvertXYUnits(style.IconHotSpot.Value.XUnits));
                        WriteAttributeString("yunits", ConvertXYUnits(style.IconHotSpot.Value.YUnits));
                        WriteEndElement();
                    }

                    WriteEndElement();
                }
                #endregion

                #region Label Style

                if (style.LabelColor != null || style.LabelColorMode != null || style.LabelScale != null)
                {
                    WriteStartElement("LabelStyle");
                    if (style.LabelID != null)
                        WriteAttributeString("id", style.LabelID);

                    if (style.LabelColor != null)
                        WriteElementString("color", style.LabelColor.ToString());
                    if (style.LabelColorMode != null)
                        WriteElementString("colorMode", ConvertColorMode(style.LabelColorMode));
                    if (style.LabelScale != null)
                        WriteElementString("scale", style.LabelScale.ToString());

                    WriteEndElement();
                }
                #endregion

                #region Line Style

                if (style.LineColor != null || style.LineColorMode != null || style.LineLabelVisibility != null || style.LineOuterColor != null ||
                    style.LineOuterWidth != null || style.LinePhysicalWidth != null || style.LineWidth != null)
                {
                    WriteStartElement("LineStyle");
                    if (style.LineID != null)
                        WriteAttributeString("id", style.LineID);

                    if (style.LineColor != null)
                        WriteElementString("color", style.LineColor.ToString());
                    if (style.LineColorMode != null)
                        WriteElementString("colorMode", ConvertColorMode(style.LineColorMode));
                    if (style.LineWidth != null)
                        WriteElementString("width", style.LineWidth.ToString());

                    if (style.LineOuterColor != null)
                        WriteElementString("gx:outerColor", style.LineOuterColor.ToString());
                    if (style.LineOuterWidth != null)
                        WriteElementString("gx:outerWidth", style.LineOuterWidth.ToString());
                    if (style.LinePhysicalWidth != null)
                        WriteElementString("gx:physicalWidth", style.LinePhysicalWidth.ToString());
                    if (style.LineLabelVisibility != null)
                        WriteElementString("gx:labelVisibility", ConvertBool((bool)style.LineLabelVisibility).ToString());

                    WriteEndElement();
                }

                #endregion

                #region Poly Style

                if (style.PolygonColor != null || style.PolygonColorMode != null || style.PolygonFill != null || style.PolygonOutline != null)
                {
                    WriteStartElement("PolyStyle");
                    if (style.PolygonID != null)
                        WriteAttributeString("id", style.PolygonID);

                    if (style.PolygonColor != null)
                        WriteElementString("color", style.PolygonColor.ToString());
                    if (style.PolygonColorMode != null)
                        WriteElementString("colorMode", ConvertColorMode(style.PolygonColorMode));
                    if (style.PolygonFill != null)
                        WriteElementString("fill", ConvertBool((bool)style.PolygonFill).ToString());
                    if (style.PolygonOutline != null)
                        WriteElementString("outline", ConvertBool((bool)style.PolygonOutline).ToString());

                    WriteEndElement();
                }
                #endregion

                #region Balloon Style

                if (style.BalloonBgColor != null || style.BalloonTextColor != null || style.BalloonText != null || style.BalloonDisplayMode != null)
                {
                    WriteStartElement("BalloonStyle");
                    if (style.BalloonID != null)
                        WriteAttributeString("id", style.BalloonID);

                    if (style.BalloonBgColor != null)
                        WriteElementString("bgColor", style.BalloonBgColor.ToString());
                    if (style.BalloonTextColor != null)
                        WriteElementString("textColor", style.BalloonTextColor.ToString());
                    if (style.BalloonText != null && style.BalloonText != "")
                        WriteElementString("text", style.BalloonText);
                    if (style.BalloonDisplayMode != null)
                        WriteElementString("displayMode", ConvertDisplayMode(style.BalloonDisplayMode));

                    WriteEndElement();
                }
                #endregion

                #region List Style

                if (style.ListBgColor != null || style.ListItemIconUrl != null || style.ListItemState != null || style.ListListItemType != null)
                {
                    WriteStartElement("listItemType");
                    if (style.ListID != null)
                        WriteAttributeString("id", style.ListID);

                    if (style.ListListItemType != null)
                        WriteElementString("listItemType", ConvertListItemType(style.ListListItemType));
                    if (style.ListBgColor != null)
                        WriteElementString("bgColor", style.ListBgColor.ToString());

                    if (style.ListItemState != null || style.ListItemIconUrl != null)
                    {
                        WriteStartElement("ItemIcon");

                        if (style.ListItemState != null)
                            WriteElementString("state", ConvertState(style.ListItemState));
                        if (style.ListItemIconUrl != null && style.ListItemIconUrl != "")
                            WriteElementString("href", style.ListItemIconUrl);

                        WriteEndElement();
                    }
                    WriteEndElement();
                }
                #endregion

                //end style
                WriteEndElement();
            }
            catch (Exception ex)
            {
                throw new Exception("WriteStyle Error: " + ex.Message);
            }
        }

        public void WriteStyleMap(StyleMap map)
        {
            try
            {
                if (map != null && map.HightLightedStyleUrl != null && map.NormalStyleUrl != null)
                {
                    WriteStartElement("StyleMap");
                    WriteAttributeString("id", map.ID);

                    WriteStartElement("Pair");
                    WriteElementString("key", "normal");
                    WriteElementString("styleUrl", map.NormalStyleUrl);
                    WriteEndElement();

                    WriteStartElement("Pair");
                    WriteElementString("key", "highlight");
                    WriteElementString("styleUrl", map.HightLightedStyleUrl);
                    WriteEndElement();

                    WriteEndElement();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("WriteStyleMap Error: " + ex.Message);
            }
        }

        public void WriteProperties(KmlProperties prop)
        {
            try
            {
                if (prop.Author != null && prop.Author != "")
                    WriteElementString("atom:author", prop.Author);
                if (prop.Link != null && prop.Link != "")
                    WriteElementString("atom:link", prop.Link);
                if (prop.Address != null && prop.Address != "")
                    WriteElementString("address", prop.Address);
                if (prop.Snippit != null && prop.Snippit != "")
                {
                    WriteStartElement("Snippit");

                    if (prop.SnippitMaxLines != null)
                        WriteAttributeString("maxLines", prop.SnippitMaxLines.ToString());
                    else
                        WriteAttributeString("maxLines", "2");

                    WriteValue(prop.Snippit);
                    WriteEndElement();
                }

                if (prop.Region != null && prop.Region != "")
                    WriteElementString("region", prop.Region);

                //write extended data
                if (prop.ExtendedData != null)
                {
                    if (prop.ExtendedData.DataItems.Count > 0)
                    {
                        WriteStartElement("ExtendedData");

                        foreach (ExtendedData.Data data in prop.ExtendedData.DataItems)
                        {
                            if (data.Name != null && data.Value != null)
                            {
                                WriteStartElement("Data");
                                WriteAttributeString("name", data.Name);
                                WriteElementString("value", data.Value);
                                WriteEndElement();
                            }
                        }

                        WriteEndElement();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("WriteProperties Error: " + ex.Message);
            }
        }

        private void WriteView(View v)
        {
            WriteStartElement("LookAt");

            if (v.TimeSpan != null && v.StartTime != null)
            {
                WriteStartElement("gx:TimeSpan");
                WriteElementString("begin", ConvertToKmlDateTime(v.StartTime));
                WriteElementString("end", ConvertToKmlDateTime(v.EndTime));
                WriteEndElement();
            }
            else if (v.TimeStamp != null)
            {
                WriteStartElement("gx:TimeStamp");
                WriteElementString("when", ConvertToKmlDateTime((DateTime)v.TimeStamp));
                WriteEndElement();
            }

            if (v.Coordinates != null)
            {
                Coordinates coords = (Coordinates)v.Coordinates;
                if (coords.Latitude != 0 || coords.Longitude != 0)
                {
                    WriteElementString("longitude", coords.Longitude.ToString());
                    WriteElementString("latitude", coords.Latitude.ToString());
                    if (v.Coordinates.Value.Altitude != null)
                        WriteElementString("altitude", coords.Altitude.ToString());
                }
            }

            WriteElementString("range", v.Range.ToString());

            if (v.Tilt != null)
                WriteElementString("tilt", v.Tilt.ToString());
            if (v.Heading != null)
                WriteElementString("heading", v.Heading.ToString());
            if (v.AltMode != null)
            {
                if (v.AltMode == AltitudeMode.Absolute || v.AltMode == AltitudeMode.ClampToGround || v.AltMode == AltitudeMode.RelativeToGround)
                    WriteElementString("altitudeMode", v.AltMode.ToString());
                else
                    WriteElementString("gx:altitudeMode", v.AltMode.ToString());
            }

            WriteEndElement();
        }

        private void WriteKmlDescription(string desc)
        {
            if (!String.IsNullOrEmpty(desc))
            {
                WriteStartElement("description");
                WriteCData(desc);
                WriteEndElement();
            }
        }


        #region Conversions
        private int ConvertBool(bool b)
        {
            return (b ? 1 : 0);
        }

        private string ConvertColorMode(ColorMode? c)
        {
            switch (c)
            {
                case ColorMode.Random:
                    return "random";
                case ColorMode.Normal:
                default:
                    return "normal";
            }
        }

        private string ConvertXYUnits(XYUnitType? xyu)
        {
            switch (xyu)
            {
                case XYUnitType.InsetPixels:
                    return "insetPixels";
                case XYUnitType.Pixels:
                    return "pixels";
                case XYUnitType.Fraction:
                default:
                    return "fraction";
            }
        }

        private string ConvertDisplayMode(DisplayMode? dm)
        {
            switch (dm)
            {
                case DisplayMode.Hide:
                    return "hide";
                case DisplayMode.Default:
                default:
                    return "default";
            }
        }

        private string ConvertListItemType(ListItemType? lit)
        {
            switch (lit)
            {
                case ListItemType.CheckHideChildern:
                    return "checkHideChildern";
                case ListItemType.CheckOffOnly:
                    return "checkOffOnly";
                case ListItemType.RadioFolder:
                    return "radioFolder";
                case ListItemType.Check:
                default:
                    return "check";
            }
        }

        private string ConvertState(State? s)
        {
            switch (s)
            {
                case State.Closed:
                    return "closed";
                case State.Error:
                    return "error";
                case State.Fetching0:
                    return "fetching0";
                case State.Fetching1:
                    return "fetching1";
                case State.Fetching2:
                    return "fetching2";
                case State.Open:
                default:
                    return "open";
            }
        }

        private string ConvertToKmlDateTime(DateTime dt)
        {
            return $@"{dt.Year.ToString("D4")}-{dt.Month.ToString("D2")}-{dt.Day.ToString("D2")}T{dt.Hour.ToString("D2")}:{dt.Minute.ToString("D2")}:{dt.Second.ToString("D2")}Z";
        }
        #endregion


        public static void WriteKmlFile(string filePath, KmlDocument doc)
        {
            using (KmlWriter kw = new KmlWriter(filePath))
            {
                kw.WriteStartDocument();
                kw.WriteStartKml();
                kw.WriteKmlDocument(doc);
                kw.WriteEndKml();
                kw.WriteEndDocument();
            }
        }
    }

}
