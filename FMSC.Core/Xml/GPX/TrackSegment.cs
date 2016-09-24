using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FMSC.Core.Xml.GPX
{
    public class TrackSegment
    {
        public List<Point> Points { get; set; } = new List<Point>();
        public string Extensions { get; set; }

        public TrackSegment() { }
    }
}
