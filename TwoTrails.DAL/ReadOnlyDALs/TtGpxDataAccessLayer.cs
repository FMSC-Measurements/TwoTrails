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
using TwoTrails.Core.Units;

namespace TwoTrails.DAL
{
    public class TtGpxDataAccessLayer : IReadOnlyTtDataLayer
    {
        public Boolean RequiresUpgrade => false;

        public string FilePath => _Options.FilePath;

        public bool HandlesAllPointTypes => false;

        private Dictionary<string, TtPoint> _Points = new Dictionary<string, TtPoint>();
        private Dictionary<string, TtUnit> _Units = new Dictionary<string, TtUnit>();

        private TtProjectInfo _ProjectInfo;

        private readonly ParseOptions _Options;
        private bool parsed;
        private int milliSecondsInc = 0;
        private int unitInc = 0;

        private static object locker = new object();


        public TtGpxDataAccessLayer(ParseOptions options)
        {
            _Options = options;

            unitInc = options.StartUnitNumber;

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
                        _Units.Clear();
                    }

                    using (XmlTextReader r = new XmlTextReader(new StreamReader(_Options.FilePath)))
                    {
                        bool inUnit = false, inPoint = false, inElev = false;

                        double lat = 0, lon = 0, elev = 0;

                        GpsPoint point = new GpsPoint(), lastPoint = null;
                        TtUnit unit = null;

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
                                                if (!inUnit && (r.Name == "rtept" || r.Name == "trkseg"))
                                                {
                                                    inUnit = true;
                                                    unit = new PolygonUnit()
                                                    {
                                                        Name = $"Unit {++unitInc}",
                                                        PointStartIndex = 1000 * unitInc + Consts.DEFAULT_POINT_INCREMENT,
                                                        TimeCreated = DateTime.Now.AddMilliseconds(milliSecondsInc++)
                                                    };

                                                    lastPoint = null;
                                                    index = 0;
                                                }
                                                else if (inUnit && (r.Name == "rtept" || r.Name == "trkpt"))
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
                                                    inUnit = false;
                                                    _Units.Add(unit.CN, unit);
                                                }
                                                else if (inPoint && (r.Name == "rtept" && inPoint || r.Name == "trkpt"))
                                                {
                                                    inPoint = false;
                                                    
                                                    point.PID = PointNamer.NamePoint(unit, lastPoint);

                                                    point.Unit = unit;
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


        public TtPoint GetPoint(String cn, bool linked = false)
        {
            Parse();

            return _Points.ContainsKey(cn) ? (linked ? GetLinkedPoints(new TtPoint[] { _Points[cn] }).First() : _Points[cn]) : null;
        }

        public IEnumerable<TtPoint> GetPoints(String unitCN = null, bool linked = false)
        {
            Parse();
            
            IEnumerable<TtPoint> points = (unitCN == null ? _Points.Values : _Points.Values.Where(p => p.UnitCN == unitCN)).OrderBy(p => p.Index);

            return linked ? GetLinkedPoints(points) : points.DeepCopy(); ;
        }

        public int GetPointCount(params string[] unitCNs)
        {
            if (unitCNs == null || !unitCNs.Any())
            {
                return _Points.Count;
            }
            else
            {
                return _Points.Values.Count(p => unitCNs.Contains(p.UnitCN));
            }
        }

        public bool HasUnits()
        {
            Parse();

            return _Units.Count > 0;
        }

        public IEnumerable<TtUnit> GetUnits()
        {
            Parse();

            return _Units.Values.DeepCopy();
        }

        public IEnumerable<TtMetadata> GetMetadata()
        {
            return new List<TtMetadata>();
        }

        public IEnumerable<TtGroup> GetGroups()
        {
            return new List<TtGroup>();
        }

        public TtNmeaBurst GetNmeaBurst(string nmeaCN)
        {
            throw new NotImplementedException(nameof(GetNmeaBurst));
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

        public IEnumerable<UnitGraphicOptions> GetUnitGraphicOptions()
        {
            return new List<UnitGraphicOptions>();
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

            public int StartUnitNumber { get; }

            public ParseOptions(string filePath, int targetZone, bool useElevation = false,
                UomElevation uomElevation = UomElevation.Feet, int startUnitNumber = 0)
            {
                FilePath = filePath;
                TargetZone = targetZone;
                UseElevation = useElevation;
                UomElevation = uomElevation;
                StartUnitNumber = startUnitNumber;
            }
        }
    }
}
