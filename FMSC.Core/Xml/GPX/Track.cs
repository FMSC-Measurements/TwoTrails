using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FMSC.Core.Xml.GPX
{
    public class Track : BaseTrack
    {
        public List<TrackSegment> Segments { get; set; } = new List<TrackSegment>();

        public Track(string name = null, string desc = null) : base(name, desc) { }
    }
}
