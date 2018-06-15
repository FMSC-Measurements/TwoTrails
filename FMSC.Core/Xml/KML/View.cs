using System;

namespace FMSC.Core.Xml.KML
{
    public class View
    {
        public DateTime StartTime { get; set; }
        public TimeSpan TimeSpan { get; set; }
        public DateTime EndTime => StartTime.Add(TimeSpan);
        public DateTime? TimeStamp { get; set; }
        public Coordinates? Coordinates { get; set; }
        public double? Heading { get; set; }
        public double? Tilt { get; set; }
        public double Range { get; set; } = 1000;
        public AltitudeMode? AltMode { get; set; }


        public View() { }

        public View(Coordinates coordinates, double? heading = null, double? tilt = null, double range = 1000)
        {
            Coordinates = coordinates;
            Heading = heading;
            Tilt = tilt;
            Range = range;
        }

        public View(View v)
        {
            StartTime = v.StartTime;
            TimeSpan = v.TimeSpan;
            TimeStamp = v.TimeStamp;
            Coordinates = v.Coordinates;
            Heading = v.Heading;
            Tilt = v.Tilt;
            Range = v.Range;
        }
    }
}
