using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FMSC.Core.Xml.GPX
{
    public class Route : BaseTrack
    {
        public List<Point> Points { get; set; } = new List<Point>();
        
        public Route(string name = null, string desc = null) : base(name, desc) { }
    }
}
