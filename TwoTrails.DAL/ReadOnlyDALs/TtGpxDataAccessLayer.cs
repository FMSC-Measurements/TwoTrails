using FMSC.GeoSpatial;
using FMSC.GeoSpatial.Types;
using FMSC.GeoSpatial.UTM;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml;
using TwoTrails.Core;
using TwoTrails.Core.Media;
using TwoTrails.Core.Points;

namespace TwoTrails.DAL
{
    public class TtGpxDataAccessLayer : IReadOnlyTtDataLayer
    {
        public Boolean RequiresUpgrade => false;

        public string FilePath => _Options.FilePath;

        public bool HandlesAllPointTypes => false;

        private Dictionary<string, TtPoint> _Points = new Dictionary<string, TtPoint>();
        private Dictionary<string, TtPolygon> _Polygons = new Dictionary<string, TtPolygon>();

        private TtProjectInfo _ProjectInfo;

        private readonly ParseOptions _Options;
        private bool parsed;
        private int secondsInc = 0;
        private int polyInc = 0;

        private static object locker = new object();


        public TtGpxDataAccessLayer(ParseOptions options)
        {
            _Options = options;

            polyInc = options.StartPolygonNumber;

            _ProjectInfo = new TtProjectInfo(Path.GetFileName(options.FilePath),
                String.Empty,
                String.Empty,
                String.Empty,
                String.Empty,
                String.Empty,
                String.Empty,
                new Version("0.0.0"),
                String.Empty,
                File.GetCreationTime(options.FilePath));
        }

        public void Parse(bool reparse = false)
        {
            lock(locker)
            {
                if (!parsed || reparse)
                {
                    if (reparse)
                    {
                        _Points.Clear();
                        _Polygons.Clear();
                    }

                    using (XmlTextReader r = new XmlTextReader(new StreamReader(_Options.FilePath)))
                    {
                        bool inPoly = false, inPoint = false, inElev = false;

                        double lat = 0, lon = 0, elev = 0;

                        GpsPoint point = new GpsPoint(), lastPoint = null;
                        TtPolygon poly = null;

                        int index = 0;
                        string tmp;

                        try
                        {
                            try
                            {
                                r.Namespaces = false;

                                while (r.Read())
                                {
                                    switch (r.NodeType)
                                    {
                                        case XmlNodeType.Element:
                                            {
                                                if (!inPoly && (r.Name == "rtept" || r.Name == "trkseg"))
                                                {
                                                    inPoly = true;
                                                    poly = new TtPolygon()
                                                    {
                                                        Name = $"Poly {++polyInc}",
                                                        PointStartIndex = 1000 * polyInc + 10,
                                                        TimeCreated = DateTime.Now.AddSeconds(secondsInc++)
                                                    };

                                                    lastPoint = null;
                                                    index = 0;
                                                }
                                                else if (inPoly && (r.Name == "rtept" || r.Name == "trkpt"))
                                                {
                                                    inPoint = true;
                                                    point = new GpsPoint();

                                                    tmp = r.GetAttribute("lat");
                                                    if (!Double.TryParse(tmp, out lat))
                                                        throw new Exception("Invalid Latitude Value");

                                                    tmp = r.GetAttribute("lon");
                                                    if (!Double.TryParse(tmp, out lon))
                                                        throw new Exception("Invalid Latitude Value");
                                                }
                                                else if (r.Name == "ele")
                                                    inElev = true;
                                            }
                                            break;
                                        case XmlNodeType.EndElement:
                                            {
                                                if (!inPoint && (r.Name == "rtept" || r.Name == "trkseg"))
                                                {
                                                    inPoly = false;
                                                    _Polygons.Add(poly.CN, poly);
                                                }
                                                else if (inPoint && (r.Name == "rtept" && inPoint || r.Name == "trkpt"))
                                                {
                                                    inPoint = false;
                                                    
                                                    point.PID = PointNamer.NamePoint(poly, lastPoint);

                                                    point.Polygon = poly;
                                                    point.MetadataCN = Consts.EmptyGuid;
                                                    point.GroupCN = Consts.EmptyGuid;
                                                    point.Index = index++;
                                                    point.OnBoundary = true;

                                                    UTMCoords coords = UTMTools.ConvertLatLonSignedDecToUTM(lat, lon, _Options.TargetZone);

                                                    point.SetUnAdjLocation(
                                                        coords.X,
                                                        coords.Y,
                                                        _Options.UseElevation ?
                                                            GeoTools.ConvertElevation(elev, UomElevation.Meters, _Options.UomElevation) :
                                                            0
                                                        );

                                                    _Points.Add(point.CN, point);

                                                    lastPoint = point;
                                                }
                                                else if (r.Name == "ele")
                                                    inElev = false;
                                            }
                                            break;
                                        case XmlNodeType.Text:
                                            {
                                                if (inElev)
                                                {
                                                    elev = 0;
                                                    if (_Options.UseElevation && !Double.TryParse(r.Value, out elev))
                                                        throw new Exception("Invalid Elevation Value");
                                                }
                                            }
                                            break;
                                        default:
                                            break;
                                    }
                                }
                            }
                            catch (XmlException)
                            {
                                //
                            }
                        }
                        catch (Exception ex)
                        {
                            Trace.WriteLine(ex.Message, "DataImport:ImportGpx");
                            return;
                        }
                    }

                    parsed = true;
                }
            }
        }

        private IEnumerable<TtPoint> GetLinkedPoints(IEnumerable<TtPoint> points)
        {
            foreach (TtPoint point in _Points.Values)
            {
                if (point.OpType == OpType.Quondam)
                {
                    QuondamPoint qp = new QuondamPoint(point);

                    if (_Points.ContainsKey(qp.ParentPointCN))
                        qp.ParentPoint = _Points[qp.ParentPointCN].DeepCopy();

                    yield return qp;
                }

                yield return point.DeepCopy();
            }
        }


        public IEnumerable<TtPoint> GetPoints(String polyCN = null, bool linked = false)
        {
            Parse();
            
            IEnumerable<TtPoint> points = (polyCN == null ? _Points.Values : _Points.Values.Where(p => p.PolygonCN == polyCN)).OrderBy(p => p.Index);

            return linked ? GetLinkedPoints(points) : points.DeepCopy(); ;
        }

        public bool HasPolygons()
        {
            Parse();

            return _Polygons.Count > 0;
        }

        public IEnumerable<TtPolygon> GetPolygons()
        {
            Parse();

            return _Polygons.Values.DeepCopy();
        }

        public IEnumerable<TtMetadata> GetMetadata()
        {
            return new List<TtMetadata>();
        }

        public IEnumerable<TtGroup> GetGroups()
        {
            return new List<TtGroup>();
        }

        public IEnumerable<TtNmeaBurst> GetNmeaBursts(String pointCN = null)
        {
            return new List<TtNmeaBurst>();
        }

        public IEnumerable<TtNmeaBurst> GetNmeaBursts(IEnumerable<string> pointCNs)
        {
            return new List<TtNmeaBurst>();
        }

        public TtProjectInfo GetProjectInfo()
        {
            Parse();

            return new TtProjectInfo(_ProjectInfo);
        }

        public IEnumerable<PolygonGraphicOptions> GetPolygonGraphicOptions()
        {
            return new List<PolygonGraphicOptions>();
        }

        public IEnumerable<TtImage> GetPictures(String pointCN)
        {
            return new List<TtImage>();
        }

        public IEnumerable<TtUserAction> GetUserActivity()
        {
            return new List<TtUserAction>();
        }


        public DataDictionaryTemplate GetDataDictionaryTemplate()
        {
            throw new NotImplementedException();
        }

        public DataDictionary GetExtendedDataForPoint(string pointCN)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<DataDictionary> GetExtendedData()
        {
            throw new NotImplementedException();
        }
        

        public class ParseOptions
        {
            public string FilePath { get; }
            public bool UseElevation { get; set; }
            public UomElevation UomElevation { get; set; }

            public int TargetZone { get; }

            public int StartPolygonNumber { get; }

            public ParseOptions(string filePath, int targetZone, bool useElevation = false,
                UomElevation uomElevation = UomElevation.Feet, int startPolyNumber = 0)
            {
                FilePath = filePath;
                TargetZone = targetZone;
                UseElevation = useElevation;
                UomElevation = uomElevation;
                StartPolygonNumber = startPolyNumber;
            }
        }
    }
}
