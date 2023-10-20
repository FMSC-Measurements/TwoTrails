using FMSC.Core;
using FMSC.Core.Xml.GPX;
using FMSC.Core.Xml.KML;
using FMSC.GeoSpatial.NMEA.Sentences;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using TwoTrails.Core;
using TwoTrails.Core.Media;
using TwoTrails.Core.Points;
using TwoTrails.DAL;
using TwoTrails.Settings;
using TwoTrails.ViewModels;
using Convert = FMSC.Core.Convert;

namespace TwoTrails.Utils
{
    public static class Export
    {
        public static void All(String projectFilePath, TtSettings settings)
        {
            TtSqliteDataAccessLayer dal = new TtSqliteDataAccessLayer(projectFilePath);
            TtSqliteMediaAccessLayer mal = null;

            string malFilePath = Path.Combine(Path.GetDirectoryName(projectFilePath),
                                $"{Path.GetFileNameWithoutExtension(projectFilePath)}{Consts.FILE_EXTENSION_MEDIA}");

            if (File.Exists(malFilePath))
                mal = new TtSqliteMediaAccessLayer(malFilePath);
            
            TtManager manager = new TtManager(dal, mal, settings);
            
            string outputPath = Path.Combine(Path.GetDirectoryName(projectFilePath), Path.GetFileNameWithoutExtension(dal.FilePath)).Trim();

            All(manager, mal, dal.GetProjectInfo(), dal.FilePath, outputPath);
        }

        public static void All(ITtManager manager, ITtMediaLayer mal, TtProjectInfo projectInfo, String projectFilePath, String folderPath)
        {
            folderPath = folderPath.Trim();
            CheckCreateFolder(folderPath);

            Project(projectInfo, Path.Combine(folderPath, "ProjectInfo"));
            Summary(manager, projectInfo, projectFilePath, Path.Combine(folderPath, $"{projectInfo.Name.ScrubFileName()}_Summary"));
            Points(manager, Path.Combine(folderPath, "Points"));
            DataDictionary(manager, Path.Combine(folderPath, "DataDictionary"));
            Polygons(manager, Path.Combine(folderPath, "Polygons"));
            Metadata(manager, Path.Combine(folderPath, "Metadata"));
            Groups(manager, Path.Combine(folderPath, "Groups"));
            TtNmea(manager, Path.Combine(folderPath, "TTNmea"));
            ImageInfo(manager, Path.Combine(folderPath, "ImageInfo"));
            GPX(manager, projectInfo, Path.Combine(folderPath, projectInfo.Name.ScrubFileName()));
            KMZ(manager, projectInfo, Path.Combine(folderPath, projectInfo.Name.ScrubFileName()));

            Shapes(manager, projectInfo, folderPath);

            if (mal != null)
                MediaFiles(mal, folderPath);
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
            
            using (StreamWriter sw = new StreamWriter($"{fileName}.csv"))
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
                        sb.Append($"{FMSC.Core.Convert.Angle(trav.SlopeAngle, trav.Metadata.Slope, Slope.Percent)},{trav.Metadata.Slope},");
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

        public static void DataDictionary(DataDictionaryTemplate template, IEnumerable<TtPoint> points, String fileName)
        {
            if (template == null)
                return;

            if (!template.Any())
                throw new Exception("Invalid or Empty DataDictionary");

            if (points.Any(p => p.ExtendedData == null))
                throw new Exception("Points missing DataDictionary");

            using (StreamWriter sw = new StreamWriter($"{fileName}.csv"))
            {
                List<string> fieldCNs = template.Select(ddf => ddf.CN).ToList();

                sw.WriteLine(String.Join(",", template.Select(ddf => ddf.Name)));

                foreach (TtPoint point in points)
                {
                    sw.WriteLine(String.Join(",", fieldCNs.Select(fcn =>
                        template[fcn].DataType == DataType.TEXT ? $"\"{point.ExtendedData[fcn].Scrub()}\"" : point.ExtendedData[fcn])));
                }

                sw.Flush();
            }

            using (XmlWriter writer = XmlWriter.Create(fileName.Replace(".csv", ".ddt"), new XmlWriterSettings() { Indent = true }))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("DataDictionaryTemplate");

                foreach (DataDictionaryField ddf in template)
                {
                    writer.WriteStartElement("Field");
                    writer.WriteAttributeString(TwoTrailsSchema.SharedSchema.CN, ddf.CN);
                    writer.WriteAttributeString(TwoTrailsSchema.DataDictionarySchema.DataType, ddf.DataType.ToString());

                    writer.WriteElementString(TwoTrailsSchema.DataDictionarySchema.Name, ddf.Name);
                    writer.WriteElementString(TwoTrailsSchema.DataDictionarySchema.FieldOrder, ddf.Order.ToString());
                    writer.WriteElementString(TwoTrailsSchema.DataDictionarySchema.FieldType, ddf.FieldType.ToString());
                    writer.WriteElementString(TwoTrailsSchema.DataDictionarySchema.Flags, ddf.Flags.ToString());

                    if (ddf.Values != null)
                    {
                        writer.WriteStartElement(TwoTrailsSchema.DataDictionarySchema.FieldValues);
                        foreach (string value in ddf.Values)
                            writer.WriteElementString("Value", ddf.DataType == DataType.TEXT ? $"\"{value.Scrub()}\"" : value);
                        writer.WriteEndElement(); 
                    }
                    
                    writer.WriteElementString(TwoTrailsSchema.DataDictionarySchema.DefaultValue, ddf.DefaultValue?.ToString());
                    writer.WriteElementString(TwoTrailsSchema.DataDictionarySchema.ValueRequired, ddf.ValueRequired.ToString());

                    writer.WriteEndElement();
                }

                writer.WriteEndElement();
                writer.WriteEndDocument();
            }
        }


        public static void Polygons(ITtManager manager, String fileName)
        {
            Polygons(manager.GetPolygons(), fileName);
        }

        public static void Polygons(IEnumerable<TtPolygon> polygons, String fileName)
        {
            using (StreamWriter sw = new StreamWriter($"{fileName}.csv"))
            {
                #region Columns
                StringBuilder sb = new StringBuilder();
                sb.Append("Name,");
                sb.Append("Accuracy_M,");
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
                    sb.AppendFormat("\"{0}\",{1}", poly.Description.Scrub(), poly.CN);

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
            using (StreamWriter sw = new StreamWriter($"{fileName}.csv"))
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
                    sb.AppendFormat("\"{0}\",{1}", group.Description.Scrub(), group.CN);
                    
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
            using (StreamWriter sw = new StreamWriter($"{fileName}.csv"))
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
                    sb.AppendFormat("\"{0}\",{1},", meta.Comment.Scrub(), meta.CN);

                    sw.WriteLine(sb.ToString());
                }

                sw.Flush();
            }
        }


        public static void Project(TtProjectInfo project, String fileName)
        {
            using (StreamWriter sw = new StreamWriter($"{fileName}.txt"))
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

        public static void Summary(ITtManager manager, TtProjectInfo projectInfo, String projectFilePath, String fileName)
        {
            using (StreamWriter sw = new StreamWriter($"{fileName}.txt"))
            {
                using (StreamWriter swcsv = new StreamWriter($"{fileName}.csv"))
                {
                    sw.Write(HaidLogic.GenerateSummaryHeader(projectInfo, projectFilePath));
                    swcsv.WriteLine("Unit,Area_Ac,Perim_Ft,GpsAE_Ac,GpsAE_Ratio,TravAE_ac,TravAE_Ratio");

                    foreach (TtPolygon poly in manager.GetPolygons())
                    {
                        PolygonSummary ps = HaidLogic.GenerateSummary(manager, poly);
                        sw.WriteLine($"{poly.Name}{Environment.NewLine}{new String('-', poly.Name.Length)}");
                        sw.WriteLine(ps.SummaryText);

                        swcsv.Write($"{poly.Name},{poly.AreaAcres},{poly.PerimeterFt},");
                        swcsv.Write($"{Convert.ToAcre(ps.TotalGpsError, Area.MeterSq)},{ps.GpsAreaError},");
                        swcsv.WriteLine($"{Convert.ToAcre(ps.TotalTraverseError, Area.MeterSq)},{ps.TraverseAreaError}");
                    }
                }
            }
        }


        public static void TtNmea(ITtManager manager, String fileName)
        {
            TtNmea(manager.GetNmeaBursts(), fileName, manager.GetPoints().ToDictionary(p => p.CN, p => p));
        }

        public static void TtNmea(IEnumerable<TtNmeaBurst> bursts, String fileName, Dictionary<string, TtPoint> points = null)
        {
            using (StreamWriter sw = new StreamWriter($"{fileName}.csv"))
            {
                #region Columns
                StringBuilder sb = new StringBuilder();

                if (points != null)
                {
                    sb.Append("Point At Exported,");
                }

                sb.Append("PointCN,");
                sb.Append("IsUsed,");

                sb.Append("Time Created,");
                sb.Append("Time Fix,");

                sb.Append("Latitude,");
                sb.Append("Longitude,");
                sb.Append("Elevation (Mt),");

                sb.Append("MagVar,");
                sb.Append("MagVarDir,");

                sb.Append("GroundSpeed,");
                sb.Append("TrackAngle,");
                
                sb.Append("Fix,");
                sb.Append("Fix Quality,");
                sb.Append("Mode,");

                sb.Append("PDOP,");
                sb.Append("HDOP,");
                sb.Append("VDOP,");

                sb.Append("Geoid Height (Mt),");

                sb.Append("Tracked Satellites Count,");
                sb.Append("Satellites In View Count,");
                sb.Append("Used Satellites Count,");
                sb.Append("Used Satellites,");
                sb.Append("Satellites In View Info,");

                sb.Append("CN");
                sw.WriteLine(sb.ToString());
                #endregion

                foreach (TtNmeaBurst burst in bursts)
                {
                    sb = new StringBuilder();

                    if (points != null)
                    {
                        TtPoint point = points.ContainsKey(burst.PointCN) ? points[burst.PointCN] : null;
                        String pinfo = point != null ? $"{point.PID} ({point.Polygon?.Name})" : String.Empty;
                        sb.AppendFormat("{0},", pinfo);
                    }

                    sb.AppendFormat("{0},{1},", burst.PointCN, burst.IsUsed);
                    sb.AppendFormat("{0},{1},", burst.TimeCreated.ToString(Consts.DATE_FORMAT), burst.FixTime.ToString(Consts.DATE_FORMAT));

                    sb.AppendFormat("{0},{1},{0},", burst.Latitude, burst.Longitude, burst.Elevation);

                    sb.AppendFormat("{0},{1},", burst.MagVar, burst.MagVarDir);

                    sb.AppendFormat("{0},{1},", burst.GroundSpeed, burst.TrackAngle);

                    sb.AppendFormat("{0},{1},{2},", burst.Fix.ToStringF(), burst.FixQuality.ToStringF(), burst.Mode.ToStringF());

                    sb.AppendFormat("{0},{1},{2},", burst.HDOP, burst.PDOP, burst.VDOP);

                    sb.AppendFormat("{0},", burst.GeoidHeight);

                    sb.AppendFormat("{0},{1},", burst.TrackedSatellitesCount, burst.SatellitesInViewCount);
                    sb.AppendFormat("{0},{1},{2},", burst.UsedSatelliteIDsCount, burst.UsedSatelliteIDsString, burst.SatellitesInViewString);
                    
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
            using (StreamWriter sw = new StreamWriter($"{fileName}.csv"))
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
                    sb.AppendFormat("{0},\"{1}\",", img.IsExternal, img.Comment.Scrub());
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
                        MediaTools.GetImageData(mal, img).SaveImageToFile(filePath);
                    }
                }
            }
        }


        public static void GPX(ITtManager manager, TtProjectInfo projectInfo, String fileName)
        {
            GpxWriter.WriteGpxFile($"{fileName}.gpx", TtGpxGenerator.Generate(manager, projectInfo.Name.ScrubFileName(), projectInfo.Description));
        }

        public static void KMZ(ITtManager manager, TtProjectInfo projectInfo, String fileName)
        {
            string kmlName = $"{fileName}.kml";
            fileName = $"{fileName}.kmz";

            KmlDocument doc = TtKmlGenerator.Generate(manager, projectInfo.Name.ScrubFileName(), projectInfo.Description);
            
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
            string kmlName = "multiproject.kml";
            fileName = $"{fileName}.kmz";

            KmlDocument doc = TtKmlGenerator.Generate(projects.Select(p => p.HistoryManager), "MultiProject");

            string kmlFile = Path.Combine(Path.GetDirectoryName(fileName), kmlName);

            KmlWriter.WriteKmlFile(kmlFile, doc);

            if (File.Exists(fileName))
                File.Delete(fileName);

            using (ZipArchive kmzFile = ZipFile.Open(fileName, ZipArchiveMode.Create))
                kmzFile.CreateEntryFromFile(kmlFile, kmlName);

            File.Delete(kmlFile);
        }

        public static void Shapes(ITtManager manager, TtProjectInfo projectInfo, String folderPath)
        {
            string shapeFolderPath = Path.Combine(folderPath, $"GIS_{projectInfo.Name.ScrubFileName()}");

            TtShapeFileGenerator.WriteAllPolygons(manager, manager.GetPolygons(), shapeFolderPath);
        }


        public static string ScrubFileName(this string text)
        {
            return new Regex("[^a-zA-Z0-9 _-]").Replace(text, "_").Trim();
        }

        public static string Scrub(this string text)
        {
            if (text != null)
                return text.Replace("\n", "\\n").Replace("\r", "\\r").Replace("\"", "\"\"");
            return String.Empty;
        }

        public static string Scrub(this object obj)
        {
            if (obj != null)
                return obj.ToString().Scrub();
            return String.Empty;
        }
    }
}
