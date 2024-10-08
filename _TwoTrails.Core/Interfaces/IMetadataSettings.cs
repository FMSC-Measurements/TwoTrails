﻿using FMSC.Core;
using FMSC.GeoSpatial;
using System;

namespace TwoTrails.Core
{
    public interface IMetadataSettings
    {
        Int32 Zone { get; }
        
        DeclinationType DecType { get; }
        Double MagDec { get; }
        
        Datum Datum { get; }
        
        Distance Distance { get; }
        Distance Elevation { get; }
        
        Slope Slope { get; }

        void SetMetadataSettings(TtMetadata metadata);

        TtMetadata CreateDefaultMetadata();
    }
}
