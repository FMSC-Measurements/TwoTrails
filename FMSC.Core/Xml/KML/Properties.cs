using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FMSC.Core.Xml.KML
{
    public class Properties
    {
        public string Author { get; set; }
        public string Link { get; set; }
        public string Address { get; set; }
        public string Snippit { get; set; }
        public int? SnippitMaxLines { get; set; }
        public string Region { get; set; }
        public ExtendedData ExtendedData { get; set; }

        public Properties()
        {
            Author = null;
            Link = null;
            Address = null;
            Snippit = null;
            SnippitMaxLines = null;
            Region = null;
            ExtendedData = null;
        }

        public Properties(Properties p)
        {
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
