using System.Collections.Generic;

namespace FMSC.Core.Xml.GPX
{
    public class Route : BaseTrack
    {
        public List<Point> Points { get; set; } = new List<Point>();
        
        public Route(string name = null, string desc = null) : base(name, desc) { }
    }
}
