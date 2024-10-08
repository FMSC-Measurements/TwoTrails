﻿using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using TwoTrails.Core;
using FMSC.Core;
using FSConvert = FMSC.Core.Convert;

namespace TwoTrails.Converters
{
    public enum MetadataPropertyName
    {
        Elevation,
        Distance,
        SlopeAngle
    }

    public class MetadataValueConverter : DependencyObject, IValueConverter
    {
        public static DependencyProperty MetadataProperty =
             DependencyProperty.Register("Metadata", typeof(TtMetadata),
             typeof(MetadataValueConverter));

        public TtMetadata Metadata
        {
            get { return (TtMetadata)GetValue(MetadataProperty); }
            set { SetValue(MetadataProperty, value); }
        }


        public static DependencyProperty MetadataPropertyNameProperty =
             DependencyProperty.Register("MetadataPropertyName", typeof(MetadataPropertyName),
             typeof(MetadataValueConverter));

        public MetadataPropertyName MetadataPropertyName
        {
            get { return (MetadataPropertyName)GetValue(MetadataPropertyNameProperty); }
            set { SetValue(MetadataPropertyNameProperty, value); }
        }


        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (Metadata != null)
            {
                if (value != null)
                {
                    switch (MetadataPropertyName)
                    {
                        case MetadataPropertyName.Elevation:
                            return FSConvert.Distance((double)value, Metadata.Elevation, Distance.Meters);
                        case MetadataPropertyName.Distance:
                            return FSConvert.Distance((double)value, Metadata.Distance, Distance.Meters);
                        case MetadataPropertyName.SlopeAngle:
                            return FSConvert.Angle((double)value, Metadata.Slope, Slope.Percent);
                    }
                }
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (Metadata != null)
            {
                if (value != null)
                {
                    if (Double.TryParse(value as string, out double dval))
                    {
                        switch (MetadataPropertyName)
                        {
                            case MetadataPropertyName.Elevation:
                                return FSConvert.Distance(dval, Distance.Meters, Metadata.Elevation);
                            case MetadataPropertyName.Distance:
                                return FSConvert.Distance(dval, Distance.Meters, Metadata.Distance);
                            case MetadataPropertyName.SlopeAngle:
                                return FSConvert.Angle(dval, Slope.Percent, Metadata.Slope);
                        }
                    }
                }
            }

            return null;
        }
    }
}
