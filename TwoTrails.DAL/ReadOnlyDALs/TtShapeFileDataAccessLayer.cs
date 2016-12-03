using FMSC.GeoSpatial.Types;
using FMSC.GeoSpatial.UTM;
using GeoAPI.Geometries;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwoTrails.Core;
using TwoTrails.Core.Media;
using TwoTrails.Core.Points;

namespace TwoTrails.DAL
{
    public class TtShapeFileDataAccessLayer : IReadOnlyTtDataLayer
    {
        public Boolean RequiresUpgrade { get; } = false;

        private Dictionary<string, TtPoint> _Points = new Dictionary<string, TtPoint>();
        private Dictionary<string, TtPolygon> _Polygons = new Dictionary<string, TtPolygon>();

        private TtProjectInfo _ProjectInfo;

        private readonly ParseOptions _Options;
        private readonly int _Zone;
        private bool parsed;
        private int hoursInc = 0;


        public TtShapeFileDataAccessLayer(ParseOptions options, int projectZone)
        {
            _Options = options;
            _Zone = projectZone;
        }


        private void Parse()
        {
            if (!parsed)
            {
                GeometryFactory factory;
                ShapefileDataReader shapeFileDataReader;
                ArrayList features;
                Feature feature;
                AttributesTable attributesTable;
                string[] keys;
                Geometry geometry;
                DbaseFieldDescriptor fldDescriptor;
                TtPolygon poly;

                GpsPoint gps;
                int index = 0;

                int totalImports = _Options.TotalImports;

                foreach (ShapeFilePackage file in _Options.ImportingShapeFiles)
                {
                    factory = new GeometryFactory();
                    shapeFileDataReader = new ShapefileDataReader(file.ShapeFilePath, factory);
                    DbaseFileHeader header = shapeFileDataReader.DbaseHeader;
                    TtPoint lastPoint = null;

                    features = new ArrayList();
                    while (shapeFileDataReader.Read())
                    {
                        feature = new Feature();
                        attributesTable = new AttributesTable();
                        keys = new string[header.NumFields];
                        geometry = (Geometry)shapeFileDataReader.Geometry;

                        for (int i = 0; i < header.NumFields; i++)
                        {
                            fldDescriptor = header.Fields[i];
                            keys[i] = fldDescriptor.Name;
                            attributesTable.AddAttribute(fldDescriptor.Name, shapeFileDataReader.GetValue(i));
                        }

                        feature.Geometry = geometry;
                        feature.Attributes = attributesTable;
                        features.Add(feature);
                    }

                    bool areAllPoints = true;
                    foreach (Feature feat in features)
                    {
                        if (feat.Geometry.GeometryType.ToLower() != "point")
                        {
                            areAllPoints = false;
                            break;
                        }
                    }

                    //if all features are points
                    if (areAllPoints)
                    {
                        poly = new TtPolygon()
                        {
                            PointStartIndex = 1000 * _Polygons.Count + 1010,
                            Name = file.Name
                        };

                        index = 0;

                        foreach (Feature feat in features)
                        {
                            //if features is only a point there should only be 1 coord
                            foreach (Coordinate coord in feat.Geometry.Coordinates)
                            {
                                gps = new GpsPoint()
                                {
                                    OnBoundary = true,
                                    Index = index++,
                                    MetadataCN = Consts.EmptyGuid,
                                    PID = PointNamer.NamePoint(poly, lastPoint),
                                    Polygon = poly
                                };

                                if (file.Zone != _Zone && _Options.ConvertInvalidZones)
                                {
                                    UTMCoords c = UTMTools.ShiftZones(coord.X, coord.Y, _Zone, file.Zone);

                                    gps.UnAdjX = c.X;
                                    gps.UnAdjY = c.Y;
                                }
                                else
                                {
                                    gps.UnAdjX = coord.X;
                                    gps.UnAdjY = coord.Y;
                                }

                                if (_Options.UseElevation)
                                {
                                    if (coord.Z != double.NaN)
                                    {
                                        gps.UnAdjZ = _Options.Elevation == UomElevation.Feet ?
                                            FMSC.Core.Convert.Distance(coord.Z, FMSC.Core.Distance.Meters, FMSC.Core.Distance.FeetTenths) :
                                            coord.Z;
                                    }
                                    else
                                        gps.UnAdjZ = 0;
                                }
                                else
                                    gps.UnAdjZ = 0;
                                
                                _Points.Add(gps.CN, gps);
                                lastPoint = gps;
                            }
                        }

                        _Polygons.Add(poly.CN, poly);
                    }
                    else //else get points out of each features
                    {
                        int fidInc = 0;

                        foreach (Feature feat in features)
                        {
                            lastPoint = null;

                            poly = new TtPolygon()
                            {
                                PointStartIndex = 1000 * _Polygons.Count + 1010,
                                Name = features.Count < 2 ? file.Name :
                                    String.Format("{0}-{1}", fidInc++, file.Name)
                            };

                            #region Shape Desc Properties
                            object[] objs = feat.Attributes.GetValues();
                            string[] names = feat.Attributes.GetNames();
                            string objv;

                            for (int i = 0; i < feat.Attributes.Count; i++)
                            {
                                if (objs[i] is string)
                                {
                                    objv = (string)objs[i];

                                    if (String.IsNullOrWhiteSpace(objv))
                                        continue;

                                    switch (names[i].ToLower())
                                    {
                                        case "description":
                                        case "comment":
                                        case "poly":
                                            if (String.IsNullOrEmpty(poly.Description))
                                                poly.Description = objv;
                                            else
                                                poly.Description = String.Format("{0} | {1}", poly.Description, objv);
                                            break;
                                        case "name":
                                        case "unit":
                                            poly.Name = objv;
                                            break;
                                    }
                                }
                            }
                            #endregion


                            index = 0;

                            foreach (Coordinate coord in feat.Geometry.Coordinates)
                            {
                                gps = new GpsPoint()
                                {
                                    OnBoundary = true,
                                    PID = PointNamer.NamePoint(poly, lastPoint),
                                    Index = index++,
                                    MetadataCN = Consts.EmptyGuid,
                                    Polygon = poly
                                };

                                if (file.Zone != _Zone && _Options.ConvertInvalidZones)
                                {
                                    UTMCoords c = UTMTools.ShiftZones(coord.X, coord.Y, _Zone, file.Zone);

                                    gps.UnAdjX = c.X;
                                    gps.UnAdjY = c.Y;
                                }
                                else
                                {
                                    gps.UnAdjX = coord.X;
                                    gps.UnAdjY = coord.Y;
                                }

                                if (_Options.UseElevation)
                                {
                                    if (coord.Z != double.NaN)
                                    {
                                        gps.UnAdjZ = _Options.Elevation == UomElevation.Feet ?
                                            FMSC.Core.Convert.Distance(coord.Z, FMSC.Core.Distance.Meters, FMSC.Core.Distance.FeetTenths) :
                                            coord.Z;
                                    }
                                    else
                                        gps.UnAdjZ = 0;
                                }
                                else
                                    gps.UnAdjZ = 0;

                                _Points.Add(gps.CN, gps);
                                lastPoint = gps;
                            }
                            
                            _Polygons.Add(poly.CN, poly);
                        }
                    }

                    //Close and free up any resources
                    shapeFileDataReader.Close();
                    shapeFileDataReader.Dispose();
                }
                
                parsed = true;
            }
        }



        public List<TtPoint> GetPoints(String polyCN = null)
        {
            Parse();

            return (polyCN == null ? _Points.Values : _Points.Values.Where(p => p.PolygonCN == polyCN)).OrderBy(p => p.Index).ToList();
        }

        public bool HasPolygons()
        {
            Parse();

            return _Polygons.Count > 0;
        }

        public List<TtPolygon> GetPolygons()
        {
            Parse();

            return _Polygons.Values.ToList();
        }

        public List<TtMetadata> GetMetadata()
        {
            return new List<TtMetadata>();
        }

        public List<TtGroup> GetGroups()
        {
            return new List<TtGroup>();
        }

        public List<TtNmeaBurst> GetNmeaBursts(String pointCN = null)
        {
            return new List<TtNmeaBurst>();
        }

        public TtProjectInfo GetProjectInfo()
        {
            Parse();

            return new TtProjectInfo(_ProjectInfo);
        }

        public List<PolygonGraphicOptions> GetPolygonGraphicOptions()
        {
            return new List<PolygonGraphicOptions>();
        }

        public List<TtImage> GetPictures(String pointCN)
        {
            return new List<TtImage>();
        }

        public List<TtUserActivity> GetUserActivity()
        {
            return new List<TtUserActivity>();
        }



        public static ShapeFileValidityResult ValidateShapePackage(string file, int targetZone)
        {
            if (String.IsNullOrWhiteSpace(file)) return ShapeFileValidityResult.MissingFiles;

            string basename = Path.GetFileNameWithoutExtension(file);
            string path = Path.GetDirectoryName(file);

            if (File.Exists(Path.Combine(path, String.Format("{0}.shx", basename))) &&
                File.Exists(Path.Combine(path, String.Format("{0}.prj", basename))) &&
                File.Exists(Path.Combine(path, String.Format("{0}.dbf", basename))))
            {
                int zone = GetShapeFileUtmZone(Path.Combine(path, String.Format("{0}.prj", basename)));

                if (zone < 0)
                {
                    return ShapeFileValidityResult.NotNAD83;
                }
                else if (zone != targetZone)
                {
                    return ShapeFileValidityResult.MismatchZone;
                }

                return ShapeFileValidityResult.Valid;
            }
            else
            {
                return ShapeFileValidityResult.MissingFiles;
            }
        }

        public enum ShapeFileValidityResult
        {
            Valid,
            MissingFiles,
            NotNAD83,
            MismatchZone
        }


        public static bool CheckShapeIsNAD83(string file)
        {
            if (!File.Exists(file))
                throw new FileNotFoundException(file);

            return CultureInfo.CurrentCulture.CompareInfo.IndexOf(
                File.ReadAllText(file), "nad_1983", CompareOptions.IgnoreCase) >= 0;
        }

        public static int GetShapeFileUtmZone(string file)
        {
            string tmp = File.ReadAllText(file).ToLower();

            if (tmp.Contains("nad"))
            {
                tmp = tmp.Substring(tmp.IndexOf("nad"), 21);
                if (tmp.Contains("zone"))
                {
                    tmp = tmp.Substring(tmp.IndexOf("zone") + 5, 2);

                    int zone;

                    if (Int32.TryParse(tmp, out zone))
                        return zone;
                }
            }

            return -1;
        }


        public class ParseOptions
        {
            public IEnumerable<ShapeFilePackage> ShapeFiles { get; }
            public IEnumerable<ShapeFilePackage> ImportingShapeFiles
            {
                get
                {
                    return ShapeFiles.Where(s => 
                        s.Result == ShapeFileValidityResult.Valid ||
                        (ConvertInvalidZones && s.Result == ShapeFileValidityResult.NotNAD83));
                }
            }

            public bool ConvertInvalidZones { get; set; } = false;
            public bool UseElevation { get; set; } = true;

            public UomElevation Elevation { get; set; } = UomElevation.Feet;

            public int TotalImports
            {
                get
                {
                    return ShapeFiles.Where(s => {
                        return s.Result == ShapeFileValidityResult.Valid ||
                            (ConvertInvalidZones && s.Result == ShapeFileValidityResult.NotNAD83);
                        }).Count();
                }
            }


            public ParseOptions(string shapeFile, int targetZone) : this(new string[] { shapeFile }, targetZone) { }

            public ParseOptions(IEnumerable<string> shapeFiles, int targetZone)
            {
                ShapeFiles = shapeFiles.Select(f => new ShapeFilePackage(f, targetZone));
            }


        }

        public class ShapeFilePackage
        {
            public ShapeFileValidityResult Result { get; }

            public string Name { get; }
            public int Zone { get; }

            public string ShapeFilePath { get; }
            public string ShxFilePath { get; }
            public string PrjFilePath { get; }
            public string DbfFilePath { get; }

            public ShapeFilePackage(string file, int targetZone)
            {
                ShapeFilePath = file;

                if (!String.IsNullOrWhiteSpace(file))
                {
                    Name = Path.GetFileNameWithoutExtension(file);
                    string path = Path.GetDirectoryName(file);

                    ShxFilePath = Path.Combine(path, String.Format("{0}.shx", Name));
                    PrjFilePath = Path.Combine(path, String.Format("{0}.prj", Name));
                    DbfFilePath = Path.Combine(path, String.Format("{0}.dbf", Name));

                    if (File.Exists(PrjFilePath) && File.Exists(ShxFilePath) && File.Exists(DbfFilePath))
                    {
                        Zone = GetShapeFileUtmZone(PrjFilePath);

                        if (Zone < 0)
                            Result = ShapeFileValidityResult.NotNAD83;
                        else if (Zone != targetZone)
                            Result = ShapeFileValidityResult.MismatchZone;

                        Result = ShapeFileValidityResult.Valid;
                    }
                    else
                        Result = ShapeFileValidityResult.MissingFiles;
                }
                else
                    Result = ShapeFileValidityResult.MissingFiles;
            }
        }
    }
}
