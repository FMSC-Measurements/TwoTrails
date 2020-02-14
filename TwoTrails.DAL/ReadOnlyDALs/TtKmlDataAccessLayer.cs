using FMSC.Core.Xml.KML;
using FMSC.GeoSpatial.Types;
using FMSC.GeoSpatial.UTM;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TwoTrails.Core;
using TwoTrails.Core.Media;
using TwoTrails.Core.Points;

namespace TwoTrails.DAL
{
    public class TtKmlDataAccessLayer : IReadOnlyTtDataLayer
    {
        private KmlDocument _KDoc;

        private Dictionary<string, TtPoint> _Points = new Dictionary<string, TtPoint>();
        private Dictionary<string, TtPolygon> _Polygons = new Dictionary<string, TtPolygon>();

        private TtProjectInfo _ProjectInfo;
        private ParseOptions _Options;

        public bool RequiresUpgrade => false;

        public string FilePath => _Options.FilePath;

        public bool HandlesAllPointTypes => false;

        private bool parsed = false;
        private object locker = new object();

        private int polyCount = 0;

        
        public TtKmlDataAccessLayer(ParseOptions options)
        {
            _Options = options;
            _KDoc = KmlDocument.Load(_Options.FilePath);
            polyCount = options.StartPolygonNumber;
        }


        private void Parse(bool reparse = false)
        {
            lock (locker)
            {
                if (!parsed || reparse)
                {
                    if (reparse)
                    {
                        _Points.Clear();
                        _Polygons.Clear();
                    }

                    polyCount = 0;

                    ParseFolder(_KDoc, null);

                    _ProjectInfo = new TtProjectInfo(
                        _KDoc.Name,
                        _KDoc.Desctription,
                        String.Empty,
                        String.Empty,
                        String.Empty,
                        String.Empty,
                        String.Empty,
                        new Version("0.0.0"),
                        String.Empty,
                        File.GetCreationTime(_Options.FilePath)
                    );

                    parsed = true;
                }
            }
        }

        private void ParseFolder(KmlFolder folder, string parentFolderName)
        {
            parentFolderName = $"{parentFolderName}{(parentFolderName != null ? "_" : "")}{folder.Name}";

            if (_Options.ParseOnlyPointFolders && folder.SubFolders.Count == 0 &&
                folder.Placemarks.Count(x => x.Polygons.Count == 0 && x.Points.Count > 0) > 2)
            {
                ParseSinglePolyFromPoints(folder, parentFolderName);
            }
            else
            {
                foreach (KmlFolder sf in folder.SubFolders)
                    ParseFolder(sf, parentFolderName);

                foreach (Placemark placemark in folder.Placemarks.Where(p => p.Polygons.Count > 0))
                    ParsePolygonsInPlacemarks(placemark, parentFolderName);
            }
        }

        private void ParseSinglePolyFromPoints(KmlFolder folder, string parentFolderName)
        {
            polyCount++;

            TtPolygon poly = new TtPolygon();
            poly.Name = String.IsNullOrWhiteSpace(folder.Name) ? $"Poly {polyCount} ({parentFolderName})" : folder.Name;
            poly.PointStartIndex = polyCount * 1000 + Consts.DEFAULT_POINT_INCREMENT;
            poly.Description = folder.Desctription;
            poly.TimeCreated = DateTime.Now.AddMilliseconds(polyCount);

            GpsPoint point, prev = null;

            foreach (Placemark pm in folder.Placemarks)
            {
                if (pm.Points.Any())
                {
                    point = CreatePoint(pm.Points[0].Coordinates, poly, prev, pm.Desctription);

                    _Points.Add(point.CN, point);

                    prev = point;
                }
            }

            _Polygons.Add(poly.CN, poly);
        }

        private void ParsePolygonsInPlacemarks(Placemark placemark, string parentFolderName)
        {
            foreach (Polygon kpoly in placemark.Polygons)
            {
                if (kpoly.HasOuterBoundary || kpoly.HasInnerBoundary)
                {
                    polyCount++;

                    TtPolygon poly = new TtPolygon();
                    poly.Name = String.IsNullOrWhiteSpace(kpoly.Name) ? $"Poly {polyCount} ({parentFolderName})" : kpoly.Name;
                    poly.PointStartIndex = polyCount * 1000 + Consts.DEFAULT_POINT_INCREMENT;
                    poly.Description = placemark.Desctription;
                    poly.TimeCreated = DateTime.Now.AddMilliseconds(polyCount);

                    GpsPoint point, prev = null;

                    foreach (Coordinates coord in kpoly.HasOuterBoundary ? kpoly.OuterBoundary : kpoly.InnerBoundary)
                    {
                        point = CreatePoint(coord, poly, prev);

                        _Points.Add(point.CN, point);

                        prev = point;
                    }

                    _Polygons.Add(poly.CN, poly);
                } 
            }
        }

        private GpsPoint CreatePoint(Coordinates coords, TtPolygon poly, TtPoint prevPoint = null, string desc = null)
        {
            GpsPoint point = new GpsPoint();

            UTMCoords utmCoords = UTMTools.ConvertLatLonSignedDecToUTM(coords.Latitude, coords.Longitude, _Options.TargetZone);

            point.SetUnAdjLocation(utmCoords.X, utmCoords.Y, coords.Altitude != null ? (double)coords.Altitude : 0);

            point.PID = PointNamer.NamePoint(poly, prevPoint);
            point.OnBoundary = true;
            point.Comment = desc;
            point.GroupCN = Consts.EmptyGuid;
            point.MetadataCN = Consts.EmptyGuid;
            point.Polygon = poly;

            return point;
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

        public IEnumerable<TtPoint> GetPoints(String polyCN = null, bool linked = false)
        {
            Parse();

            return (polyCN == null ? _Points.Values : _Points.Values.Where(p => p.PolygonCN == polyCN))
                .OrderBy(p => p.Index).DeepCopy();
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

        public IEnumerable<TtNmeaBurst> GetNmeaBursts(IEnumerable<String> pointCNs)
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

            public bool ParseOnlyPointFolders { get; }

            public int StartPolygonNumber { get; }


            public ParseOptions(string filePath, int targetZone, bool useElevation = false,
                UomElevation uomElevation = UomElevation.Feet, bool parseOnlyPointFolders = false, int startPolyNumber = 0)
            {
                FilePath = filePath;
                TargetZone = targetZone;
                UseElevation = useElevation;
                UomElevation = uomElevation;
                ParseOnlyPointFolders = parseOnlyPointFolders;
                StartPolygonNumber = startPolyNumber;
            }
        }
    }
}