using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml.Linq;

namespace FMSC.GeoSpatial.MTDC
{
    public class GpsAccuracyReport
    {
        public const string GpsTestsUrl = @"https://www.fs.fed.us/database/gps/mtdcrept/accuracy/gpsTests.xml";

        public ReadOnlyCollection<Manufacturer> Manufacturers { get; }

        public GpsAccuracyReport(string data)
        {
            XDocument doc = XDocument.Parse(data);

            Manufacturers = new ReadOnlyCollection<Manufacturer>(
                doc.Root.Elements().Select(e => new Manufacturer(e))
                .ToList());
        }

        public static GpsAccuracyReport Retrieve()
        {
            byte[] data;
            using (WebClient webClient = new WebClient())
                data = webClient.DownloadData(GpsTestsUrl);

            return new GpsAccuracyReport(Encoding.GetEncoding("Windows-1252").GetString(data));
        }

        public static void DownloadGpsTests(string fileName)
        {
            using (WebClient webClient = new WebClient())
                webClient.DownloadFile(GpsTestsUrl, fileName);
        }

        public static GpsAccuracyReport Load(string file)
        {
            return new GpsAccuracyReport(File.ReadAllText(file));
        }
    }

    public class Manufacturer
    {
        public string ID { get; }
        public string Name { get; }
        public ReadOnlyCollection<Model> Models { get; }

        public Manufacturer(XElement elem)
        {
            ID = elem.Element("ID").Value;
            Name = elem.Element("Manufacturer").Value;

            Models = new ReadOnlyCollection<Model>(
                elem.Elements("Models").Select(e => new Model(e))
                .ToList());
        }
    }

    public class Model
    {
        public string ID { get; }
        public string ManufacturerID { get; }
        public string Name { get; }
        public ReadOnlyCollection<Test> Tests { get; }

        public Model(XElement elem)
        {
            ID = elem.Element("ID").Value;
            Name = elem.Element("Model").Value;
            ManufacturerID = elem.Element("Make_ID").Value;

            Tests = new ReadOnlyCollection<Test>(
                elem.Elements("Tests").Select(e => new Test(e))
                .ToList());
        }
    }

    public class Test
    {
        public string ModelID { get; }
        public int Positions { get; }
        public bool Glonass { get; }
        public SBASType SBAS { get; }
        public bool ExternalAntenna { get; }
        public bool PostProcessed { get; }
        public double? OpenAcc { get; }
        public double? MedAcc { get; }
        public double? HeavyAcc { get; }

        public Test(XElement elem)
        {
            ModelID = elem.Element("Model_ID").Value;

            if (int.TryParse(elem.Element("position").Value, out int tmpI))
                Positions = tmpI;
            
            Glonass = ParseBool(elem.Element("glonass").Value);

            SBAS = ParseSBAS(elem.Element("SBAS").Value);
            
            ExternalAntenna = ParseBool(elem.Element("ext_ant").Value);

            PostProcessed = ParseBool(elem.Element("post_proc")?.Value);
            
            if (double.TryParse(elem.Element("open_accu")?.Value, out double tmpD))
                OpenAcc = tmpD;

            if (double.TryParse(elem.Element("med_accu")?.Value, out tmpD))
                MedAcc = tmpD;

            if (double.TryParse(elem.Element("heavy_accu")?.Value, out tmpD))
                HeavyAcc = tmpD;
        }

        private SBASType ParseSBAS(string type)
        {
            switch (type.ToLower())
            {
                case "none": return SBASType.None;
                case "waas": return SBASType.WAAS;
            }

            return SBASType.Unknown;
        }

        private bool ParseBool(string value)
        {
            if (value != null)
            {
                switch (value.ToLower())
                {
                    case "1":
                    case "t":
                    case "yes":
                    case "true":
                        return true;
                } 
            }

            return false;
        }
    }

    public enum SBASType
    {
        Unknown = 0,
        None = 1,
        WAAS = 2
    }
}
