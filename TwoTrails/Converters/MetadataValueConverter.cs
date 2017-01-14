﻿using TwoTrails;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using TwoTrails.Core;
using FMSC.Core;

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
                            return FMSC.Core.Convert.Distance((double)value, Metadata.Elevation, Distance.Meters);
                        case MetadataPropertyName.Distance:
                            return FMSC.Core.Convert.Distance((double)value, Metadata.Distance, Distance.Meters);
                        case MetadataPropertyName.SlopeAngle:
                            return FMSC.Core.Convert.Angle((double)value, Metadata.Slope, Slope.Percent);
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
                    double dval;

                    if (Double.TryParse(value as string, out dval))
                    {
                        switch (MetadataPropertyName)
                        {
                            case MetadataPropertyName.Elevation:
                                return FMSC.Core.Convert.Distance(dval, Distance.Meters, Metadata.Elevation);
                            case MetadataPropertyName.Distance:
                                return FMSC.Core.Convert.Distance(dval, Distance.Meters, Metadata.Distance);
                            case MetadataPropertyName.SlopeAngle:
                                return FMSC.Core.Convert.Angle(dval, Slope.Percent, Metadata.Slope);
                        }
                    }
                }
            }

            return null;
        }
    }
}
