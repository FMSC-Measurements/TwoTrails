using FMSC.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwoTrails.Core;
using TwoTrails.Core.Points;
using CSUtil;
using TwoTrails.ViewModels;
using FMSC.Core.Xml.KML;
using System.IO.Compression;
using FMSC.GeoSpatial.Types;
using FMSC.GeoSpatial.NMEA.Sentences;
using FMSC.Core.Xml.GPX;
using TwoTrails.Core.Media;
using TwoTrails.DAL;
using System.Windows.Media.Imaging;

namespace TwoTrails.Utils
{
    public static class Export
    {
        public static void All(TtProject project, String folderPath)
        {
            folderPath = folderPath.Trim();
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            Project(project.ProjectInfo, Path.Combine(folderPath, "ProjectInfo.txt"));
            Summary(project.Manager, Path.Combine(folderPath, "Summary.txt"));
            Points(project.Manager, Path.Combine(folderPath, "Points.csv"));
            DataDictionary(project.Manager, Path.Combine(folderPath, "DataDictionary.csv"));
            Polygons(project.Manager, Path.Combine(folderPath, "Polygons.csv"));
            Metadata(project.Manager, Path.Combine(folderPath, "Metadata.csv"));
            Groups(project.Manager, Path.Combine(folderPath, "Groups.csv"));
            TtNmea(project.Manager, Path.Combine(folderPath, "NmeaBursts.csv"));
            ImageInfo(project.Manager, Path.Combine(folderPath, "ImageInfo.csv"));
            GPX(project, Path.Combine(folderPath, $"{project.ProjectName.Trim()}.gpx"));
            KMZ(project, Path.Combine(folderPath, $"{project.ProjectName.Trim()}.kmz"));

            Shapes(project, folderPath);
            if (project.MAL != null)
                MediaFiles(project.MAL, folderPath);
        }

        public static void CheckCreateFolder(string folderPath)
        {
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);
        }


        public static void Points(ITtManager manager, String fileName)
        {
            Points(manager.GetPoints(), fileName);
        }

        public static void Points(List<TtPoint> points, String fileName)
        {
            if (points.Any(p => p.Polygon == null || p.Metadata == null || p.Group == null))
                throw new Exception("Points missing Polygons, Metadata or Groups");

            points.Sort();
            
            using (StreamWriter sw = new StreamWriter(fileName))
            {
                #region Columns
                StringBuilder sb = new StringBuilder();
                sb.Append("Point ID,");
                sb.Append("CN,");

                sb.Append("Operation,");
                sb.Append("Index,");

                sb.Append("Polygon,");
                sb.Append("DateTime,");
                sb.Append("Metadata,");
                sb.Append("Group,");

                sb.Append("OnBoundary,");

                sb.Append("AdjX,");
                sb.Append("AdjY,");
                sb.Append("AdjZ,");
                sb.Append("UnAdjX,");
                sb.Append("UnAdjY,");
                sb.Append("UnAdjZ,");

                sb.Append("Man Acc,");

                sb.Append("Latitude,");
                sb.Append("Longitude,");
                sb.Append("Elevation,");
                sb.Append("RMSEr,");

                sb.Append("Fwd Az,");
                sb.Append("Back Az,");
                sb.Append("Horiz Dist,");
                sb.Append("Slope Dist,");
                sb.Append("Dist UOM,");
                sb.Append("Slope Angle,");
                sb.Append("Angle UOM,");

                sb.Append("Parent Name,");
                sb.Append("Parent CN,");

                sb.Append("Comment,");

                sb.Append("Poly CN,");
                sb.Append("Meta CN,");
                sb.Append("Group CN,");
                sb.Append("Linked CNs");

                sw.WriteLine(sb.ToString());
                #endregion

                foreach (TtPoint point in points)
                {
                    sb = new StringBuilder();
                    sb.Append($"{point.PID},{point.CN},");
                    sb.Append($"{point.OpType},{point.Index},");

                    sb.Append($"{point.Polygon.Scrub()},{point.TimeCreated.ToString(Consts.DATE_FORMAT)},");
                    sb.Append($"{point.Metadata.Scrub()},{point.Group.Scrub()},");

                    sb.Append($"{point.OnBoundary},");
                    
                    sb.Append($"{point.AdjX},{point.AdjY},{point.AdjZ},");
                    sb.Append($"{point.UnAdjX},{point.UnAdjY},{point.UnAdjZ},");

                    sb.Append($"{((point.IsGpsType() || point.OpType == OpType.Quondam) ? ((IManualAccuracy)point).ManualAccuracy : null)},");

                    if (point.IsGpsType())
                    {
                        GpsPoint gps = point as GpsPoint;
                        sb.Append($"{gps.Latitude},{gps.Longitude},{gps.Elevation},{gps.RMSEr},");
                    }
                    else
                        sb.Append(",,,,");

                    if (point.IsTravType())
                    {
                        TravPoint trav = point as TravPoint;
                        sb.Append($"{trav.FwdAzimuth},{trav.BkAzimuth},");
                        sb.Append($"{FMSC.Core.Convert.Distance(trav.HorizontalDistance, trav.Metadata.Distance, Distance.Meters)},");
                        sb.Append($"{FMSC.Core.Convert.Distance(trav.SlopeDistance, trav.Metadata.Distance, Distance.Meters)},{trav.Metadata.Distance},");
                        sb.Append($"{FMSC.Core.Convert.Angle(trav.SlopeAngle, trav.Metadata.Slope, Slope.Degrees)},{trav.Metadata.Slope},");
                    }
                    else
                        sb.Append(",,,,,,,");

                    if (point.OpType == OpType.Quondam)
                    {
                        QuondamPoint qp = point as QuondamPoint;
                        sb.Append($"{qp.ParentPoint},{qp.ParentPointCN},");
                    }
                    else
                        sb.Append(",,");

                    if (point.Comment != null)
                        sb.Append($"\"{point.Comment.Scrub()}\",");
                    else
                        sb.Append(",");

                    sb.Append($"{point.PolygonCN},{point.MetadataCN},{point.GroupCN},");
                    sb.Append(point.LinkedPoints.ToStringContents("_"));

                    sw.WriteLine(sb.ToString());
                }

                sw.Flush();
            }
        }


        public static void DataDictionary(ITtManager manager, String fileName)
        {
            DataDictionary(manager.GetDataDictionaryTemplate(), manager.GetPoints(), fileName);
        }

        public static void DataDictionary(DataDictionaryTemplate template, List<TtPoint> points, String fileName)
        {
            if (template == null || !template.Any())
                throw new Exception("Invalid or Empty DataDictionary");

            if (points.Any(p => p.ExtendedData == null))
                throw new Exception("Points missing DataDictionary");

            using (StreamWriter sw = new StreamWriter(fileName))
            {
                #region Columns
                StringBuilder sb = new StringBuilder();
                List<string> fieldCNs = new List<string>();

                foreach (DataDictionaryField ddf in template)
                {
                    sb.Append($"{ ddf.Name },");
                    fieldCNs.Add(ddf.CN);
                }

                sw.WriteLine(sb.ToString());
                #endregion

                foreach (TtPoint point in points)
                {
                    sb.Clear();

                    foreach (string cn in fieldCNs)
                        sb.Append($"{point.ExtendedData[cn]},");

                    sw.WriteLine(sb.ToString());
                }

                sw.Flush();
            }
        }


        public static void Polygons(ITtManager manager, String fileName)
        {
            Polygons(manager.GetPolygons(), fileName);
        }

        public static void Polygons(IEnumerable<TtPolygon> polygons, String fileName)
        {
            using (StreamWriter sw = new StreamWriter(fileName))
            {
                #region Columns
                StringBuilder sb = new StringBuilder();
                sb.Append("Name,");
                sb.Append("Accuracy,");
                sb.Append("Area_MtSq,");
                sb.Append("Perimeter_M,");
                sb.Append("Line_Perimeter_M,");
                sb.Append("Description,");
                sb.Append("CN");

                sw.WriteLine(sb.ToString());
                #endregion

                foreach (TtPolygon poly in polygons)
                {
                    sb = new StringBuilder();
                    sb.AppendFormat("{0},{1},", poly.Name.Scrub(), poly.Accuracy);
                    sb.AppendFormat("{0},{1},{2},", poly.Area, poly.Perimeter, poly.PerimeterLine);
                    sb.AppendFormat("{0},{1}", poly.Description.Scrub(), poly.CN);

                    sw.WriteLine(sb.ToString());
                }

                sw.Flush();
            }
        }
        

        public static void Groups(ITtManager manager, String fileName)
        {
            Groups(manager.GetGroups(), fileName);
        }

        public static void Groups(IEnumerable<TtGroup> groups, String fileName)
        {
            using (StreamWriter sw = new StreamWriter(fileName))
            {
                #region Columns
                StringBuilder sb = new StringBuilder();
                sb.Append("Name,");
                sb.Append("Type,");
                sb.Append("Description,");
                sb.Append("CN");

                sw.WriteLine(sb.ToString());
                #endregion

                foreach (TtGroup group in groups)
                {
                    sb = new StringBuilder();
                    sb.AppendFormat("{0},{1},", group.Name.Scrub(), group.GroupType);
                    sb.AppendFormat("{0},{1}", group.Description.Scrub(), group.CN);
                    
                    sw.WriteLine(sb.ToString());
                }

                sw.Flush();
            }
        }


        public static void Metadata(ITtManager manager, String fileName)
        {
            Metadata(manager.GetMetadata(), fileName);
        }

        public static void Metadata(IEnumerable<TtMetadata> metadata, String fileName)
        {
            using (StreamWriter sw = new StreamWriter(fileName))
            {
                #region Columns
                StringBuilder sb = new StringBuilder();
                sb.Append("Name,");
                sb.Append("Zone,");
                sb.Append("Datum,");
                sb.Append("Distance,");
                sb.Append("Elevation,");
                sb.Append("Slope,");
                sb.Append("Declination Type,");
                sb.Append("Declination,");
                sb.Append("GPS,");
                sb.Append("Range Finder,");
                sb.Append("Compass,");
                sb.Append("Crew,");
                sb.Append("Comment,");
                sb.Append("CN");

                sw.WriteLine(sb.ToString());
                #endregion

                foreach (TtMetadata meta in metadata)
                {
                    sb = new StringBuilder();
                    sb.AppendFormat("{0},{1},{2},", meta.Name, meta.Zone, meta.Datum);
                    sb.AppendFormat("{0},{1},{2},", meta.Distance, meta.Elevation, meta.Slope);

                    sb.AppendFormat("{0},{1},", meta.DecType, meta.MagDec);

                    sb.AppendFormat("{0},{1},", meta.GpsReceiver, meta.RangeFinder);
                    sb.AppendFormat("{0},{1},", meta.Compass, meta.Crew);
                    sb.AppendFormat("{0},{1},", meta.Comment.Scrub(), meta.CN);

                    sw.WriteLine(sb.ToString());
                }

                sw.Flush();
            }
        }


        public static void Project(TtProjectInfo project, String fileName)
        {
            using (StreamWriter sw = new StreamWriter(fileName))
            {
                StringBuilder sb = new StringBuilder();

                sb.AppendFormat("Name: {0}{1}", project.Name, Environment.NewLine);
                sb.AppendFormat("Description: {0}{1}", project.Description, Environment.NewLine);
                sb.AppendFormat("Region: {0}{1}", project.Region, Environment.NewLine);
                sb.AppendFormat("Forest: {0}{1}", project.Forest, Environment.NewLine);
                sb.AppendFormat("District: {0}{1}", project.District, Environment.NewLine);
                sb.AppendFormat("DbVersion: {0}{1}", project.DbVersion, Environment.NewLine);
                sb.AppendFormat("Version: {0}{1}", project.Version, Environment.NewLine);
                sb.AppendFormat("CreationVersion: {0}{1}", project.CreationVersion, Environment.NewLine);
                sb.AppendFormat("CreationDeviceID: {0}{1}", project.CreationDeviceID, Environment.NewLine);
                sb.AppendFormat("CreationDate: {0}", project.CreationDate);

                sw.Write(sb.ToString());
                sw.Flush();
            }
        }

        public static void Summary(ITtManager manager, String fileName)
        {
            using (StreamWriter sw = new StreamWriter(fileName))
            {
                foreach (TtPolygon poly in manager.GetPolygons())
                {
                    sw.WriteLine($"{poly.Name}{Environment.NewLine}{string.Join("", Enumerable.Range(0, poly.Name.Length).Select(x => "-"))}");
                    sw.WriteLine(HaidLogic.GenerateSummary(manager, poly).SummaryText);
                }
            }
        }


        public static void TtNmea(ITtManager manager, String fileName)
        {
            TtNmea(manager.GetNmeaBursts(), fileName);
        }

        public static void TtNmea(IEnumerable<TtNmeaBurst> bursts, String fileName)
        {
            using (StreamWriter sw = new StreamWriter(fileName))
            {
                #region Columns
                StringBuilder sb = new StringBuilder();
                sb.Append("PointCN,");
                sb.Append("IsUsed,");

                sb.Append("BurstTime,");
                sb.Append("FixTime,");

                sb.Append("Latitude,");
                sb.Append("Longitude,");
                sb.Append("Elevation,");
                sb.Append("ElevUom,");

                sb.Append("MagVar,");
                sb.Append("MagVarDir,");

                sb.Append("TrackAngle,");
                sb.Append("GroundSpeed,");
                
                sb.Append("Fix,");
                sb.Append("FixType,");
                sb.Append("Mode,");

                sb.Append("TrackedSatellitesCount,");
                sb.Append("SatellitesInViewCount,");
                sb.Append("UsedSatellites,");
                sb.Append("UsedSatellitesCount,");

                sb.Append("HDOP,");
                sb.Append("PDOP,");
                sb.Append("VDOP,");

                sb.Append("CN");
                sw.WriteLine(sb.ToString());
                #endregion

                foreach (TtNmeaBurst burst in bursts)
                {
                    sb = new StringBuilder();
                    sb.AppendFormat("{0},{1},", burst.PointCN, burst.IsUsed);
                    sb.AppendFormat("{0},{1},", burst.TimeCreated.ToString(Consts.DATE_FORMAT), burst.FixTime.ToString(Consts.DATE_FORMAT));

                    sb.AppendFormat("{0},{1},", burst.Latitude, burst.Longitude);
                    sb.AppendFormat("{0},{1},", burst.Elevation, burst.UomElevation.ToStringAbv());

                    sb.AppendFormat("{0},{1},", burst.MagVar, burst.MagVarDir);
                    sb.AppendFormat("{0},{1},", burst.TrackAngle, burst.GroundSpeed);

                    sb.AppendFormat("{0},{1},{2},", burst.Fix.ToStringF(), burst.FixQuality.ToStringF(), burst.Mode.ToStringF());

                    sb.AppendFormat("{0},{1},", burst.TrackedSatellitesCount, burst.SatellitesInViewCount);
                    sb.AppendFormat("{0},{1},", burst.UsedSatelliteIDsString, burst.UsedSatelliteIDsCount);

                    sb.AppendFormat("{0},{1},{2},", burst.HDOP, burst.PDOP, burst.VDOP);

                    sb.AppendFormat("{0}", burst.CN);

                    sw.WriteLine(sb.ToString());
                }

                sw.Flush();
            }
        }


        public static void ImageInfo(ITtManager manager, String fileName)
        {
            ImageInfo(manager.GetImages(), fileName);
        }

        public static void ImageInfo(IEnumerable<TtImage> images, String fileName)
        {
            using (StreamWriter sw = new StreamWriter(fileName))
            {
                #region Columns
                StringBuilder sb = new StringBuilder();
                sb.Append("Name,");
                sb.Append("Creation Time,");
                sb.Append("IsExternal,");
                sb.Append("Comment,");
                sb.Append("Type,");
                sb.Append("Azimuth,");
                sb.Append("Pitch,");
                sb.Append("Roll,");
                sb.Append("CN,");
                sb.Append("PointCN");

                sw.WriteLine(sb.ToString());
                #endregion

                foreach (TtImage img in images)
                {
                    sb = new StringBuilder();
                    sb.AppendFormat("{0},{1},", img.Name.Scrub(), img.TimeCreated.ToString(Consts.DATE_FORMAT));
                    sb.AppendFormat("{0},{1},", img.IsExternal, img.Comment.Scrub());
                    sb.AppendFormat("{0},{1},", img.PictureType.ToString(), img.Azimuth?.ToString("F2"));
                    sb.AppendFormat("{0},{1},", img.Pitch?.ToString("F2"), img.Roll?.ToString("F2"));
                    sb.AppendFormat("{0},{1}", img.CN, img.PointCN);

                    sw.WriteLine(sb.ToString());
                }

                sw.Flush();
            }
        }

        //todo add option to put media into sub folders with the point name
        public static void MediaFiles(ITtMediaLayer mal, String folderPath)
        {
            IEnumerable<TtImage> images = mal.GetImages();

            if (images.Any())
            {
                string folder = Path.Combine(folderPath, "Media");

                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                foreach (TtImage img in images)
                {
                    String filePath = Path.Combine(folder, Path.GetFileName(img.FilePath));

                    if (img.IsExternal)
                    {
                        if (File.Exists(img.FilePath))
                            File.Copy(img.FilePath, filePath);
                    }
                    else
                    {
                        mal.GetImageData(img).SaveImageToFile(filePath);
                    }
                }
            }
        }


        public static void GPX(TtProject project, String fileName)
        {
            GpxWriter.WriteGpxFile(fileName, TtGpxGenerator.Generate(project.Manager, project.ProjectName.Trim(), project.ProjectInfo.Description));
        }

        public static void KMZ(TtProject project, String fileName)
        {
            KmlDocument doc = TtKmlGenerator.Generate(project.Manager, project.ProjectName.Trim(), project.ProjectInfo.Description);
            
            string kmlName = $"{project.ProjectName.Trim()}.kml";
            string kmlFile = Path.Combine(Path.GetDirectoryName(fileName), kmlName);

            KmlWriter.WriteKmlFile(kmlFile, doc);

            if (File.Exists(fileName))
                File.Delete(fileName);

            using (ZipArchive kmzFile = ZipFile.Open(fileName, ZipArchiveMode.Create))
                kmzFile.CreateEntryFromFile(kmlFile, kmlName);

            File.Delete(kmlFile);
        }

        public static void KMZ(IEnumerable<TtProject> projects, String fileName)
        {
            KmlDocument doc = TtKmlGenerator.Generate(projects.Select(p => p.Manager), "MultiProject");

            string kmlName = "multiproject.kml";
            string kmlFile = Path.Combine(Path.GetDirectoryName(fileName), kmlName);

            KmlWriter.WriteKmlFile(kmlFile, doc);

            if (File.Exists(fileName))
                File.Delete(fileName);

            using (ZipArchive kmzFile = ZipFile.Open(fileName, ZipArchiveMode.Create))
                kmzFile.CreateEntryFromFile(kmlFile, kmlName);

            File.Delete(kmlFile);
        }

        public static void Shapes(TtProject project, String folderPath)
        {
            string shapeFolderPath = Path.Combine(folderPath, project.ProjectName);

            foreach (TtPolygon poly in project.Manager.Polygons)
            {
                TtShapeFileGenerator.WritePolygon(project.Manager, poly, shapeFolderPath);
            }
        }



        private static string Scrub(this string text)
        {
            if (text != null)
                return text.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace(",", ";");
            return String.Empty;
        }   

        private static string Scrub(this object obj)
        {
            if (obj != null)
                return obj.ToString().Scrub();
            return String.Empty;
        }
    }
}
