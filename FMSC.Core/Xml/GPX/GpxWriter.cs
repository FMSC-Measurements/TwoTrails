using System;
using System.Text;
using System.Xml;

namespace FMSC.Core.Xml.GPX
{
    public class GpxWriter : XmlTextWriter
    {
        private string _file;
        
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

        public GpxWriter(string filename)
            : base(filename, null)
        {
            _FileName = filename;
            Formatting = Formatting.Indented;
            Indentation = 4;
        }

        public void WriteStartGpx(string author = null)
        {
            WriteStartElement("gpx");
            WriteAttributeString("version", "1.1");
            WriteAttributeString("creator", author ?? String.Empty);

            _open = true;
        }

        public void WriteEndGpx()
        {
            WriteEndElement();
            _open = false;
        }
        
        public void WriteGpxDocument(GpxDocument doc)
        {
            if (_open && doc != null)
            {
                if (doc.Metadata != null)
                    WriteMetadata(doc.Metadata);

                if (doc.Waypoints != null && doc.Waypoints.Count > 0)
                {
                    foreach (Point point in doc.Waypoints)
                        WritePoint(point, PointType.WayPoint);
                }

                if (doc.Routes != null && doc.Routes.Count > 0)
                {
                    foreach (Route route in doc.Routes)
                        WriteRoute(route);
                }

                if (doc.Tracks != null && doc.Tracks.Count > 0)
                {
                    foreach (Track track in doc.Tracks)
                        WriteTrack(track);
                }

                if (doc.Extensions != null)
                    WriteExtensions(doc.Extensions);
            }
        }

        public void WriteRoute(Route route)
        {
            if (route != null)
            {
                WriteStartElement("rte");

                if (!String.IsNullOrEmpty(route.Name))
                    WriteElementString("name", route.Name);

                if (!String.IsNullOrEmpty(route.Comment))
                    WriteElementString("cmt", route.Comment);

                if (!String.IsNullOrEmpty(route.Description))
                    WriteElementString("desc", route.Description);

                if (!String.IsNullOrEmpty(route.Source))
                    WriteElementString("src", route.Source);

                if (!String.IsNullOrEmpty(route.Link))
                    WriteElementString("link", route.Link);

                if (route.Number != null)
                    WriteElementString("number", route.Number.ToString());

                if (!String.IsNullOrEmpty(route.Type))
                    WriteElementString("type", route.Type);

                if (!String.IsNullOrEmpty(route.Extensions))
                    WriteExtensions(route.Extensions);

                WriteStartElement("rtept");

                foreach (Point point in route.Points)
                    WritePoint(point, PointType.RoutePoint);

                //end of points
                WriteEndElement();
                //end of route
                WriteEndElement();
            }
        }

        public void WriteTrack(Track track)
        {
            if (track != null)
            {
                WriteStartElement("trk");

                if (!String.IsNullOrEmpty(track.Name))
                    WriteElementString("name", track.Name);

                if (!String.IsNullOrEmpty(track.Comment))
                    WriteElementString("cmt", track.Comment);

                if (!String.IsNullOrEmpty(track.Description))
                    WriteElementString("desc", track.Description);

                if (!String.IsNullOrEmpty(track.Source))
                    WriteElementString("src", track.Source);

                if (!String.IsNullOrEmpty(track.Link))
                    WriteElementString("link", track.Link);

                if (track.Number != null)
                    WriteElementString("number", track.Number.ToString());

                if (!String.IsNullOrEmpty(track.Name))
                    WriteElementString("type", track.Type);

                if (!String.IsNullOrEmpty(track.Extensions))
                    WriteExtensions(track.Extensions);

                if (track.Segments != null)
                {
                    foreach (TrackSegment seg in track.Segments)
                        WriteTrackSegment(seg);
                }

                //end of route
                WriteEndElement();
            }
        }

        public void WriteTrackSegment(TrackSegment trackseg)
        {
            if (trackseg != null)
            {
                WriteStartElement("trkseg");

                if (trackseg.Points != null)
                {
                    foreach (Point point in trackseg.Points)
                        WritePoint(point, PointType.TrackPoint);
                }

                WriteExtensions(trackseg.Extensions);

                WriteEndElement();
            }
        }

        public void WritePoint(Point point, PointType type = PointType.WayPoint)
        {
            if (point != null)
            {
                switch (type)
                {
                    case PointType.RoutePoint:
                        WriteStartElement("rtept");
                        break;
                    case PointType.TrackPoint:
                        WriteStartElement("trkpt");
                        break;
                    case PointType.WayPoint:
                    default:
                        WriteStartElement("wpt");
                        break;
                }

                WriteAttributeString("lat", point.Latitude.ToString());
                WriteAttributeString("lon", point.Longitude.ToString());

                if (point.Altitude != null)
                    WriteElementString("ele", point.Altitude.ToString());

                if (point.Time != null)
                    WriteElementString("time", point.Time.ToString());

                if (point.MagVar != null)
                    WriteElementString("magvar", point.MagVar.ToString());

                if (point.GeoidHeight != null)
                    WriteElementString("geoidheight", point.GeoidHeight.ToString());

                if (!String.IsNullOrEmpty(point.Name))
                    WriteElementString("name", point.Name);

                if (!String.IsNullOrEmpty(point.Comment))
                    WriteElementString("cmt", point.Comment);

                if (!String.IsNullOrEmpty(point.Description))
                    WriteElementString("desc", point.Description);

                if (!String.IsNullOrEmpty(point.Source))
                    WriteElementString("src", point.Source);

                if (!String.IsNullOrEmpty(point.Link))
                    WriteElementString("link", point.Link);

                if (!String.IsNullOrEmpty(point.Symmetry))
                    WriteElementString("sym", point.Symmetry);

                if (point.Fix != null)
                    WriteElementString("fix", point.Fix.ToString());

                if (point.SatteliteNum != null)
                    WriteElementString("sat", point.SatteliteNum.ToString());

                if (point.HDOP != null)
                    WriteElementString("hdop", point.HDOP.ToString());

                if (point.VDOP != null)
                    WriteElementString("vdop", point.VDOP.ToString());

                if (point.PDOP != null)
                    WriteElementString("pdop", point.PDOP.ToString());

                if (point.AgeOfData != null)
                    WriteElementString("ageofdata", point.AgeOfData.ToString());

                if (!String.IsNullOrEmpty(point.DGpsID))
                    WriteElementString("dgpsid", point.DGpsID);

                WriteExtensions(point.Extensions);

                WriteEndElement();
            }
        }

        public void WriteMetadata(Metadata meta)
        {
            if (meta != null)
            {
                WriteStartElement("metadata");

                if (!String.IsNullOrEmpty(meta.Name))
                    WriteElementString("name", meta.Name);

                if (!String.IsNullOrEmpty(meta.Description))
                    WriteElementString("desc", meta.Description);

                if (!String.IsNullOrEmpty(meta.Author))
                    WriteElementString("author", meta.Author);

                if (!String.IsNullOrEmpty(meta.Copyright))
                    WriteElementString("copyright", meta.Copyright);

                if (!String.IsNullOrEmpty(meta.Link))
                    WriteElementString("link", meta.Link);

                if (meta.Time != null)
                    WriteElementString("time", meta.Time.ToString());

                if (meta.Keywords != null && meta.Keywords.Count > 0)
                {
                    StringBuilder sb = new StringBuilder();

                    foreach (string keyword in meta.Keywords)
                        sb.Append(keyword + ", ");

                    sb.Remove(sb.Length - 1, 1);

                    WriteElementString("keywords", sb.ToString());
                }

                if (meta.Extensions != null)
                    WriteExtensions(meta.Extensions);

                WriteEndElement();
            }
        }

        public void WriteExtensions(string extenstions)
        {
            if (!String.IsNullOrEmpty(extenstions))
                WriteElementString("extensions", extenstions);
        }


        public static void WriteGpxFile(string filePath, GpxDocument doc)
        {
            using (GpxWriter gw = new GpxWriter(filePath))
            {
                gw.WriteStartDocument();
                gw.WriteStartGpx();
                gw.WriteGpxDocument(doc);
                gw.WriteEndGpx();
                gw.WriteEndDocument();
            }
        }
    }
}
