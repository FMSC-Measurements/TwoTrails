using System;
using System.Collections.Generic;
using System.Text;

namespace TwoTrails.Core.Media
{
    public class TtPanorama : TtImage
    {
        public override PictureType PictureType { get { return PictureType.Panorama; } }


        public TtPanorama(String cn, String name, String filePath, String comment, DateTime timeCreated, String pointCN)
        : base(cn, name, filePath, comment, timeCreated, pointCN) { }

        public TtPanorama(String cn, String name, String filePath, String comment, DateTime timeCreated, String pointCN,
            float? azimuth, float? pitch, float? roll) : base(cn, name, filePath, comment, timeCreated, pointCN,
                azimuth, pitch, roll)
        { }

        public TtPanorama(TtImage picture) : base(picture) { }
    }
}
