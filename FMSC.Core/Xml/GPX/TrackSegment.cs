using System.Collections.Generic;

namespace FMSC.Core.Xml.GPX
{
    public class TrackSegment
    {
        public List<Point> Points { get; set; } = new List<Point>();
        public string Extensions { get; set; }

        public TrackSegment() { }
    }
}
