using System;

namespace FMSC.Core.Xml.KML
{
    public class StyleMap
    {
        public string ID { get; }
        public string StyleUrl { get { return $"#{ID}"; } }

        public string NormalStyleUrl { get; set; }
        public string HightLightedStyleUrl { get; set; }
        
        public StyleMap(string id = null, string norm = null, string high = null)
        {
            ID = id??Guid.NewGuid().ToString();
            NormalStyleUrl = norm;
            HightLightedStyleUrl = high;
        }
    }
}
