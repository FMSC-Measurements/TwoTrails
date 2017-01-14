using System;
using System.Collections.Generic;
using System.Text;

namespace TwoTrails.Core.Media
{
    public class TtImage : TtMedia
    {
        private float? _Azimuth;
        public float? Azimuth
        {
            get { return _Azimuth; }
            set
            {
                SetField(ref _Azimuth, value);
            }
        }

        private float? _Pitch;
        public float? Pitch
        {
            get { return _Pitch; }
            set
            {
                SetField(ref _Pitch, value);
            }
        }

        private float? _Roll;
        public float? Roll
        {
            get { return _Roll; }
            set
            {
                SetField(ref _Roll, value);
            }
        }


        public sealed override MediaType MediaType { get { return MediaType.Picture; } }

        public virtual PictureType PictureType { get { return PictureType.Regular; } }
        

        public TtImage(String cn, String name, String filePath, String comment, DateTime timeCreated, String pointCN)
        : base(cn, name, filePath, comment, timeCreated, pointCN) { }

        public TtImage(String cn, String name, String filePath, String comment, DateTime timeCreated, String pointCN,
            float? azimuth, float? pitch, float? roll) : base(cn, name, filePath, comment, timeCreated, pointCN)
        {
            _Azimuth = azimuth;
            _Pitch = pitch;
            _Roll = roll;
        }

        public TtImage(TtImage picture) : base(picture)
        {
            _Azimuth = picture._Azimuth;
            _Pitch = picture._Pitch;
            _Roll = picture._Roll;
        }
    }
}
