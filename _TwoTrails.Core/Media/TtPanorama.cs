using System;
using System.Collections.Generic;
using System.Text;

namespace TwoTrails.Core.Media
{
    public class TtPanorama : TtImage
    {
        public override ImageType PictureType { get { return ImageType.Panorama; } }


        public TtPanorama(String cn, String name, String filePath, String comment, DateTime timeCreated, String pointCN, bool isExternal)
        : base(cn, name, filePath, comment, timeCreated, pointCN, isExternal) { }

        public TtPanorama(String cn, String name, String filePath, String comment, DateTime timeCreated, String pointCN, bool isExternal,
            float? azimuth, float? pitch, float? roll) : base(cn, name, filePath, comment, timeCreated, pointCN, isExternal,
                azimuth, pitch, roll)
        { }

        public TtPanorama(TtImage picture) : base(picture) { }
    }
}
