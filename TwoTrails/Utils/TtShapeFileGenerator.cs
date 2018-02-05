using FMSC.GeoSpatial.UTM;
using GeoAPI.Geometries;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NetTopologySuite.Operation.Polygonize;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwoTrails.Core;
using TwoTrails.Core.Points;

namespace TwoTrails.Utils
{
    public static class TtShapeFileGenerator
    {
        public static void WritePolygon(ITtManager manager, TtPolygon polygon, string folderPath)
        {
            string polyDir = Path.Combine(folderPath, polygon.Name);

            if (!Directory.Exists(polyDir))
            {
                Directory.CreateDirectory(polyDir);
            }

            string baseFileName = Path.Combine(polyDir, polygon.Name);

            IEnumerable<TtPoint> points = manager.GetPoints(polygon.CN).Where(p => p.OpType != OpType.WayPoint);

            if (points.Any())
            {
                CoordinateList bndAdjCoords = new CoordinateList();
                CoordinateList bndUnAdjCoords = new CoordinateList();

                CoordinateList navAdjCoords = new CoordinateList();
                CoordinateList navUnAdjCoords = new CoordinateList();

                int zone = points.First().Metadata.Zone;

                foreach (TtPoint p in points)
                {
                    UTMCoords adj = TtUtils.GetCoords(p, zone, true);
                    UTMCoords unadj = TtUtils.GetCoords(p, zone, false);

                    if (p.IsBndPoint())
                    {
                        bndAdjCoords.Add(new Coordinate(adj.X, adj.Y));
                        bndUnAdjCoords.Add(new Coordinate(unadj.X, unadj.Y));
                    }

                    if (p.IsNavPoint())
                    {
                        navAdjCoords.Add(new Coordinate(adj.X, adj.Y));
                        navUnAdjCoords.Add(new Coordinate(unadj.X, unadj.Y));
                    }
                }

                #region Navigation

                #region Adjusted
                string fileName = baseFileName + "_NavAdj";
                GeometryFactory geoFac = new GeometryFactory();
                ShapefileDataWriter sdw;
                Polygonizer polyizer = new Polygonizer();

                List<IFeature> features = new List<IFeature>();
                AttributesTable attTable = new AttributesTable();

                attTable.AddAttribute("Poly_Name", polygon.Name);
                attTable.AddAttribute("Desc", polygon.Description);
                attTable.AddAttribute("Poly", "Navigation Adjusted");
                attTable.AddAttribute("CN", polygon.CN);
                attTable.AddAttribute("Perim_M", polygon.Perimeter);
                attTable.AddAttribute("PerimLine_M", polygon.PerimeterLine);

                Feature feat = new Feature();
                DbaseFileHeader dbh;

                if (navAdjCoords.Count > 1)
                {
                    sdw = new ShapefileDataWriter(fileName, geoFac);

                    feat.Geometry = new LineString(navAdjCoords.ToArray());

                    feat.Attributes = attTable;

                    features.Add(feat);

                    dbh = ShapefileDataWriter.GetHeader((Feature)features[0], features.Count);

                    sdw.Header = dbh;
                    sdw.Write(features);
                    WriteProjection(fileName, zone);

                    //points
                    fileName = baseFileName + "_NavAdj_Points";
                    geoFac = new GeometryFactory();
                    sdw = new ShapefileDataWriter(fileName, geoFac);
                    features = new List<IFeature>();
                    attTable["Poly"] = "Navigation Adjusted Points";

                    features = GetPointFeatures(points.Where(p => p.IsNavPoint()), true, zone);

                    if (features.Count > 0)
                    {
                        dbh = ShapefileDataWriter.GetHeader((Feature)features[0], features.Count);
                        sdw.Header = dbh;
                        sdw.Write(features);
                        WriteProjection(fileName, zone);
                    }
                }
                #endregion

                #region UnAdj
                if (navUnAdjCoords.Count > 1)
                {
                    fileName = baseFileName + "_NavUnAdj";
                    geoFac = new GeometryFactory();
                    sdw = new ShapefileDataWriter(fileName, geoFac);
                    features = new List<IFeature>();
                    attTable["Poly"] = "Navigation UnAdjusted";
                    feat = new Feature();
                    feat.Geometry = new LineString(navUnAdjCoords.ToArray());
                    feat.Attributes = attTable;

                    features.Add(feat);

                    dbh = ShapefileDataWriter.GetHeader((Feature)features[0], features.Count);
                    sdw.Header = dbh;
                    sdw.Write(features);
                    WriteProjection(fileName, zone);

                    //points
                    fileName = baseFileName + "_NavUnAdj_Points";
                    geoFac = new GeometryFactory();
                    sdw = new ShapefileDataWriter(fileName, geoFac);
                    features = new List<IFeature>();
                    attTable["Poly"] = "Navigation UnAdjusted Points";

                    features = GetPointFeatures(points.Where(p => p.IsNavPoint()), false,zone);

                    if (features.Count > 0)
                    {
                        dbh = ShapefileDataWriter.GetHeader((Feature)features[0], features.Count);
                        sdw.Header = dbh;
                        sdw.Write(features);
                        WriteProjection(fileName, zone);
                    }
                }
                #endregion

                #endregion

                #region Boundary

                #region Adj Line
                if (bndAdjCoords.Count > 1)
                {
                    fileName = baseFileName + "_BndAdjLine";
                    geoFac = new GeometryFactory();
                    sdw = new ShapefileDataWriter(fileName, geoFac);
                    features = new List<IFeature>();
                    attTable["Poly"] = "Boundary Adjusted Line";
                    feat = new Feature();
                    feat.Geometry = new LineString(bndAdjCoords.ToArray());
                    feat.Attributes = attTable;

                    features.Add(feat);

                    dbh = ShapefileDataWriter.GetHeader((Feature)features[0], features.Count);
                    sdw.Header = dbh;
                    sdw.Write(features);
                    WriteProjection(fileName, zone);
                }
                #endregion

                #region UnAdj Line
                if (bndUnAdjCoords.Count > 1)
                {
                    fileName = baseFileName + "_BndUnAdjLine";
                    geoFac = new GeometryFactory();
                    sdw = new ShapefileDataWriter(fileName, geoFac);
                    features = new List<IFeature>();
                    attTable["Poly"] = "Boundary UnAdjusted Line";
                    feat = new Feature();
                    feat.Geometry = new LineString(bndUnAdjCoords.ToArray());
                    feat.Attributes = attTable;

                    features.Add(feat);

                    dbh = ShapefileDataWriter.GetHeader((Feature)features[0], features.Count);
                    sdw.Header = dbh;
                    sdw.Write(features);
                    WriteProjection(fileName,zone);
                }
                #endregion

                #region Adj
                if (bndAdjCoords.Count > 3)
                {
                    fileName = baseFileName + "_BndAdj";
                    geoFac = new GeometryFactory();
                    sdw = new ShapefileDataWriter(fileName, geoFac);
                    features = new List<IFeature>();
                    attTable.AddAttribute("Area_MtSq", polygon.Area);
                    attTable["Poly"] = "Boundary Adjusted";
                    feat = new Feature();

                    if (bndAdjCoords[0] != bndAdjCoords[bndAdjCoords.Count - 1])
                        bndAdjCoords.Add(bndAdjCoords[0]);

                    feat.Geometry = new Polygon(new LinearRing(bndAdjCoords.ToArray()));
                    feat.Attributes = attTable;

                    features.Add(feat);

                    dbh = ShapefileDataWriter.GetHeader((Feature)features[0], features.Count);
                    sdw.Header = dbh;
                    sdw.Write(features);
                    WriteProjection(fileName, zone);

                    //points
                    fileName = baseFileName + "_BndAdj_Points";
                    geoFac = new GeometryFactory();
                    sdw = new ShapefileDataWriter(fileName, geoFac);
                    features = new List<IFeature>();
                    attTable["Poly"] = "Boundary Adjusted Points";

                    features = GetPointFeatures(points.Where(p => p.IsBndPoint()), true, zone);

                    if (features.Count > 0)
                    {
                        dbh = ShapefileDataWriter.GetHeader((Feature)features[0], features.Count);
                        sdw.Header = dbh;
                        sdw.Write(features);
                        WriteProjection(fileName, zone);
                    }
                }
                #endregion

                #region UnAdj
                if (bndUnAdjCoords.Count > 3)
                {
                    fileName = baseFileName + "_BndUnAdj";
                    geoFac = new GeometryFactory();
                    sdw = new ShapefileDataWriter(fileName, geoFac);
                    features = new List<IFeature>();
                    attTable["Poly"] = "Boundary UnAdjusted";
                    feat = new Feature();

                    if (bndUnAdjCoords[0] != bndUnAdjCoords[bndUnAdjCoords.Count - 1])
                        bndUnAdjCoords.Add(bndUnAdjCoords[0]);

                    feat.Geometry = new Polygon(new LinearRing(bndUnAdjCoords.ToArray()));
                    feat.Attributes = attTable;

                    features.Add(feat);

                    dbh = ShapefileDataWriter.GetHeader((Feature)features[0], features.Count);
                    sdw.Header = dbh;
                    sdw.Write(features);
                    WriteProjection(fileName, zone);

                    //points
                    fileName = baseFileName + "_BndUnAdj_Points";
                    geoFac = new GeometryFactory();
                    sdw = new ShapefileDataWriter(fileName, geoFac);
                    features = new List<IFeature>();
                    attTable["Poly"] = "Boundary UnAdjusted Points";

                    features = GetPointFeatures(points.Where(p => p.IsBndPoint()), false, zone);

                    if (features.Count > 0)
                    {
                        dbh = ShapefileDataWriter.GetHeader((Feature)features[0], features.Count);
                        sdw.Header = dbh;
                        sdw.Write(features);
                        WriteProjection(fileName, zone);
                    }
                }
                #endregion
                #endregion
            }

            IEnumerable<TtPoint> wayPoints = manager.GetPoints(polygon.CN).Where(p => p.OpType == OpType.WayPoint);

            if (wayPoints.Any())
                WriteWayPointsFile(baseFileName, polygon, wayPoints.Cast<WayPoint>());
        }


        private static void WriteProjection(string fileName, int zone)
        {
            string _Projection = String.Format("PROJCS[\"NAD_1983_UTM_Zone_{0}N\",GEOGCS[\"GCS_North_American_1983\",DATUM[\"D_North_American_1983\",SPHEROID[\"GRS_1980\",6378137.0,298.257222101]],PRIMEM[\"Greenwich\",0.0],UNIT[\"Degree\",0.0174532925199433]],PROJECTION[\"Transverse_Mercator\"],PARAMETER[\"False_Easting\",500000.0],PARAMETER[\"False_Northing\",0.0],PARAMETER[\"Central_Meridian\",{1}],PARAMETER[\"Scale_Factor\",0.9996],PARAMETER[\"Latitude_Of_Origin\",0.0],UNIT[\"Meter\",1.0],AUTHORITY[\"EPSG\",269{0}]]",
                zone, (zone * 6 - 183));
            
            if (fileName != null && fileName.Length > 0)
            {
                using (TextWriter tw = new StreamWriter($"{fileName}.prj", false))
                {
                    tw.WriteLine(_Projection);
                }
            }
        }
        
        private static List<IFeature> GetPointFeatures(IEnumerable<TtPoint> points, bool adjusted, int zone)
        {
            List<IFeature> features = new List<IFeature>();
            Feature feat;

            AttributesTable attPointTable = new AttributesTable();

            foreach (TtPoint p in points)
            {
                attPointTable = new AttributesTable();
                attPointTable.AddAttribute("PID", p.PID);
                attPointTable.AddAttribute("Op", p.OpType.ToString());
                attPointTable.AddAttribute("Index", p.Index);
                attPointTable.AddAttribute("PolyName", p.Polygon.Name);
                attPointTable.AddAttribute("DateTime", p.TimeCreated.ToString("MM/dd/yyyy hh:mm:ss tt"));
                
                attPointTable.AddAttribute("OnBnd", p.OnBoundary);
                attPointTable.AddAttribute("AdjX", p.AdjX);
                attPointTable.AddAttribute("AdjY", p.AdjY);
                attPointTable.AddAttribute("AdjZ", p.AdjZ);
                attPointTable.AddAttribute("UnAdjX", p.UnAdjX);
                attPointTable.AddAttribute("UnAdjY", p.UnAdjY);
                attPointTable.AddAttribute("UnAdjZ", p.UnAdjZ);


                if (p.IsGpsType())
                {
                    GpsPoint gps = p as GpsPoint;
                    attPointTable.AddAttribute("Latitude", gps.Latitude.ToStringEx());
                    attPointTable.AddAttribute("Longitude", gps.Longitude.ToStringEx());
                    attPointTable.AddAttribute("Elevation", gps.Elevation.ToStringEx());
                    attPointTable.AddAttribute("RMSEr", gps.RMSEr.ToStringEx());
                    attPointTable.AddAttribute("ManAcc", gps.ManualAccuracy.ToStringEx());
                }
                else
                {
                    attPointTable.AddAttribute("Latitude", String.Empty);
                    attPointTable.AddAttribute("Longitude", String.Empty);
                    attPointTable.AddAttribute("Elevation", String.Empty);
                    attPointTable.AddAttribute("RMSEr", String.Empty);
                    attPointTable.AddAttribute("ManAcc", String.Empty);
                }

                if (p.OpType == OpType.Quondam)
                {
                    QuondamPoint q = (QuondamPoint)p;
                    attPointTable.AddAttribute("ParentName", q.ParentPoint.PID);
                }
                else
                {
                    attPointTable.AddAttribute("ParentName", String.Empty);
                }


                attPointTable.AddAttribute("Comment", p.Comment == null ? String.Empty : p.Comment);

                attPointTable.AddAttribute("CN", p.CN);

                feat = new Feature();
                UTMCoords c = TtUtils.GetCoords(p, zone, adjusted);
                feat.Geometry = new Point(c.X, c.Y);

                feat.Attributes = attPointTable;

                features.Add(feat);
            }

            return features;
        }

        private static void WriteWayPointsFile(string filePath, TtPolygon polygon, IEnumerable<WayPoint> points)
        {
            string fileName = filePath + "_WayPoints";
            int zone = points.First().Metadata.Zone;
            ShapefileDataWriter sdw = new ShapefileDataWriter(fileName, new GeometryFactory());

            List<IFeature> features = GetPointFeatures(points, false, zone);

            sdw.Header = ShapefileDataWriter.GetHeader((Feature)features[0], features.Count);
            sdw.Write(features);
            WriteProjection(fileName, zone);
        }
        

        private static string ToStringEx<T>(this T? obj) where T : struct
        {
            return obj == null ? String.Empty : obj.ToString();
        }
    }
}
