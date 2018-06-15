using System;

namespace FMSC.Core.Xml.KML
{
    public abstract class KmlProperties
    {
        public string Name { get; set; }
        public string Desctription { get; set; }
        public string StyleUrl { get; set; }
        public bool? Open { get; set; }
        public bool? Visibility { get; set; }

        public string Author { get; set; }
        public string Link { get; set; }
        public string Address { get; set; }
        public string Snippit { get; set; }
        public int? SnippitMaxLines { get; set; }
        public string Region { get; set; }
        public ExtendedData ExtendedData { get; set; }

        public KmlProperties(string name = null, string desc = null)
        {
            Name = name ?? String.Empty;
            Desctription = desc ?? String.Empty;
        }

        public KmlProperties(KmlProperties p)
        {
            Name = p.Name;
            Desctription = p.Desctription ?? String.Empty;
            StyleUrl = p.StyleUrl;
            Open = p.Open;
            Visibility = p.Visibility;

            Author = p.Author;
            Link = p.Link;
            Address = p.Address;
            Snippit = p.Snippit;
            SnippitMaxLines = p.SnippitMaxLines;
            Region = p.Region;
            ExtendedData = p.ExtendedData;
        }
    }

}
