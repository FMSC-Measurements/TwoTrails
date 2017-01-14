using System;
using System.Collections.Generic;
using System.Text;

namespace TwoTrails.Core.Media
{
    public class TtPhotoShpere : TtImage
    {
        public override PictureType PictureType { get { return PictureType.PhotoSphere; } }


        public TtPhotoShpere(String cn, String name, String filePath, String comment, DateTime timeCreated, String pointCN)
        : base(cn, name, filePath, comment, timeCreated, pointCN) { }

        public TtPhotoShpere(String cn, String name, String filePath, String comment, DateTime timeCreated, String pointCN,
            float? azimuth, float? pitch, float? roll) : base(cn, name, filePath, comment, timeCreated, pointCN,
                azimuth, pitch, roll)
        { }

        public TtPhotoShpere(TtImage picture) : base(picture) { }
    }
}
