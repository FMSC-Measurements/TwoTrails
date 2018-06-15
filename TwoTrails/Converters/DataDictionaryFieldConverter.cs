using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using TwoTrails.Core;
using TwoTrails.ViewModels;

namespace TwoTrails.Converters
{
    public class DataDictionaryFieldConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values != null && values.Length > 1 &&
                values[0] is IEnumerable<DataDictionaryField> ddfs && values[1] is PointEditorModel dem)
            {
                return ddfs.Select<DataDictionaryField, ExtendedDataField>(ddf =>
                {
                    switch (ddf.FeildType)
                    {
                        case FeildType.ComboBox: return new ComboBoxExtendedDataField(ddf, dem);
                        case FeildType.CheckBox: return new CheckBoxExtendedDataField(ddf, dem);
                        default:
                        case FeildType.TextBox: return new TextBoxExtendedDataField(ddf, dem);
                    }
                });
            }

            return null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
