using System;
using System.Collections.Generic;

namespace FMSC.Core.Xml.GPX
{
    public class Metadata
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Author { get; set; }
        public string Copyright { get; set; }
        public string Link { get; set; }
        public DateTime? Time { get; set; }
        public List<string> Keywords { get; set; }
        public string Extensions { get; set; }
        

        public Metadata(string name = null, string desc = null)
        {
            Name = name;
            Description = desc;
        }
    }
}
