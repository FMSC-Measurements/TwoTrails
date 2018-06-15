using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Data;
using TwoTrails.Core;
using TwoTrails.ViewModels;

namespace TwoTrails
{
    public abstract class ExtendedDataField : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public string Name { get; }

        public string PropertyCN { get; }

        public int Flags { get; }

        public IValueConverter Converter { get; }

        public PointEditorModel DataEditor { get; }
        
        public bool IsValueSame { get { return DataEditor.ArePropertyValuesSame(PropertyCN); } }

        public object Value
        {
            get { return DataEditor.ExtendedData[PropertyCN]; }
            set { DataEditor.ExtendedData[PropertyCN] = value; }
        }

        public ExtendedDataField(DataDictionaryField field, PointEditorModel dataEditor, IValueConverter converter = null)
        {
            Name = field.Name;
            PropertyCN = field.CN;
            Flags = field.Flags;
            DataEditor = dataEditor;
            Converter = converter;

            DataEditor.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == PropertyCN && PropertyChanged != null)
                {
                    PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));
                    PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(IsValueSame)));
                }
            };
        }
    }

    public class TextBoxExtendedDataField : ExtendedDataField
    {
        public bool IsNumeric { get; }

        public bool IsNumericDecimal { get; }


        public TextBoxExtendedDataField(DataDictionaryField field, PointEditorModel dataEditor, IValueConverter converter = null) :
            base(field, dataEditor, converter) { }
    }

    public class ComboBoxExtendedDataField : ExtendedDataField
    {
        public IList<String> Values { get; }

        public bool IsEditable { get; }

        public ComboBoxExtendedDataField(DataDictionaryField field, PointEditorModel dataEditor, IValueConverter converter = null) :
            base(field, dataEditor, converter)
        {
            IsEditable = field.Flags == 1;
            Values = field.Values ?? new List<string>();
        }
    }

    public class CheckBoxExtendedDataField : ExtendedDataField
    {
        public CheckBoxExtendedDataField(DataDictionaryField field, PointEditorModel dataEditor, IValueConverter converter = null) :
            base(field, dataEditor, converter) { }
    }
}
