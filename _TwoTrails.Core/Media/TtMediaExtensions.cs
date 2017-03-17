using System;
using System.Collections.Generic;
using System.Text;

namespace TwoTrails.Core.Media
{
    public static class TtMediaExtensions
    {
        public static TtImage DeepCopy(this TtImage image)
        {
            switch (image.PictureType)
            {
                case ImageType.Regular: return new TtImage(image);
                case ImageType.Panorama: return new TtPanorama(image);
                case ImageType.PhotoSphere: return new TtPhotoShpere(image);
                default:
                    throw new Exception("Unknown Image Type");
            }
        }
    }
}
