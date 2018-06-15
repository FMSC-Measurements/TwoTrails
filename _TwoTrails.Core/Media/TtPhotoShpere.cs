using System;

namespace TwoTrails.Core.Media
{
    public class TtPhotoShpere : TtImage
    {
        public override ImageType PictureType { get { return ImageType.PhotoSphere; } }


        public TtPhotoShpere(String cn, String name, String filePath, String comment, DateTime timeCreated, String pointCN, bool isExternal)
        : base(cn, name, filePath, comment, timeCreated, pointCN, isExternal) { }

        public TtPhotoShpere(String cn, String name, String filePath, String comment, DateTime timeCreated, String pointCN, bool isExternal,
            float? azimuth, float? pitch, float? roll) : base(cn, name, filePath, comment, timeCreated, pointCN, isExternal,
                azimuth, pitch, roll)
        { }

        public TtPhotoShpere(TtImage picture) : base(picture) { }
    }
}
