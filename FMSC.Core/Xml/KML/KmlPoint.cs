using System;

namespace FMSC.Core.Xml.KML
{
    public class KmlPoint
    {
        public string CN { get; }
        public string Name { get; set; }
        public Coordinates Coordinates { get; set; }
        public int? Extrude { get; set; }

        public AltitudeMode? _AltMode;
        public AltitudeMode? AltMode
        {
            get { return _AltMode; }
            set
            {
                _AltMode = value;

                if (_AltMode != null &&
                    (_AltMode == AltitudeMode.ClampToSeaFloor || _AltMode == AltitudeMode.ClampToGround))
                {
                    Extrude = 0;
                }
                else
                {
                    Extrude = 1;
                }
            }
        }

        
        public KmlPoint(Coordinates coordinates = new Coordinates(), AltitudeMode altMode = AltitudeMode.ClampToGround)
        {
            CN = Guid.NewGuid().ToString();
            Coordinates = coordinates;
            AltMode = altMode;
        }
    }
}
