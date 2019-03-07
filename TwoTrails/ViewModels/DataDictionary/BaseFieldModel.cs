using CSUtil.ComponentModel;
using FMSC.Core.Windows.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using TwoTrails.Core;

namespace TwoTrails.ViewModels.DataDictionary
{
    public class BaseFieldModel : NotifyPropertyChangedEx, IEquatable<BaseFieldModel>, IEquatable<DataDictionaryField>
    {
        public string CN { get; }

        public String Name
        {
            get { return Get<string>(); }
            set { Set(value); }
        }

        public int Order
        {
            get { return Get<int>(); }
            set {
                Set(value);
                OnPropertyChanged(nameof(Order)); //to notify commands that fields around it have changed
            }
        }

        public FieldType FieldType { get; }
        
        public int Flags
        {
            get { return Get<int>(); }
            set { Set(value); }
        }

        public List<String> Values
        {
            get { return Get<List<string>>(); }
            set { Set(value); }
        }

        public object DefaultValue
        {
            get { return Get(); }
            set { Set(value); }
        }

        public virtual DataType DataType
        {
            get { return Get<DataType>(); }
            set { Set(value); }
        }

        public virtual bool ValueRequired
        {
            get { return Get<bool>(); }
            set { Set(value); }
        }

        public bool DataTypeEditable { get; private set; }

        
        public BaseFieldModel(DataDictionaryField field)
        {
            CN = field.CN;
            Name = field.Name;
            Order = field.Order;
            FieldType = field.FieldType;
            Flags = field.Flags;
            Values = field.Values ?? (field.FieldType == FieldType.ComboBox ? new List<string>() : null);
            DefaultValue = field.GetDefaultValue();
            DataType = field.DataType;
            ValueRequired = field.ValueRequired;
        }

        public BaseFieldModel(string cn, FieldType fieldType, DataType dataType = DataType.TEXT, string name = null, bool valueRequired = false)
        {
            CN = cn;
            FieldType = fieldType;
            Name = name;
            DataType = dataType;
            ValueRequired = valueRequired;
            Values = (fieldType == FieldType.ComboBox ? new List<string>() : null);
            DefaultValue = null;

            DataTypeEditable = true;
        }


        public DataDictionaryField CreateDataDictionaryField()
        {
            return new DataDictionaryField(CN)
            {
                Name = Name,
                Order = Order,
                FieldType = FieldType,
                Flags = Flags,
                Values = Values,
                DefaultValue = DefaultValue,
                DataType = DataType,
                ValueRequired = ValueRequired
            };
        }


        public void ValidateText(object sender, TextCompositionEventArgs e)
        {
            switch (DataType)
            {
                case DataType.INTEGER: e.Handled = ControlUtils.TextIsInteger(sender, e); break;
                case DataType.DECIMAL:
                case DataType.FLOAT: e.Handled = ControlUtils.TextIsDouble(sender, e); break;
            }
        }

        public void ValidateName(object sender, TextCompositionEventArgs e)
        {
            if (!(e.Text.All(x => char.IsLetterOrDigit(x) || " #^*-_+(){}[]:.".Contains(x))))
                e.Handled = true;
        }


        public override bool Equals(object obj)
        {
            return obj is BaseFieldModel bfm && this == bfm;
        }

        public bool Equals(BaseFieldModel bfm)
        {
            return
                this.CN == bfm.CN &&
                this.Name == bfm.Name &&
                this.Order == bfm.Order &&
                this.FieldType == bfm.FieldType &&
                this.Flags == bfm.Flags &&
                this.DataType == bfm.DataType &&
                this.ValueRequired == bfm.ValueRequired &&
                ((this.Values == null) == (bfm.Values == null) &&
                    (this.Values == null || (this.Values.Count == bfm.Values.Count && this.Values.SequenceEqual(bfm.Values)))) &&
                ((this.DefaultValue == null) == (bfm.DefaultValue == null) &&
                    (this.DefaultValue == null || this.DefaultValue.Equals(bfm.DefaultValue)));
            //(!(this.Values != null ^ bfm.Values != null) &&
            //    (this.Values == null || (this.Values.Count == bfm.Values.Count && this.Values.SequenceEqual(bfm.Values)))) &&
            //(!(this.DefaultValue != null ^ bfm.DefaultValue != null) &&
            //    (this.DefaultValue == null || this.DefaultValue.Equals(bfm.DefaultValue)));
        }

        public bool Equals(DataDictionaryField other)
        {
            return
                this.CN == other.CN &&
                this.Name == other.Name &&
                this.Order == other.Order &&
                this.FieldType == other.FieldType &&
                this.Flags == other.Flags &&
                this.DataType == other.DataType &&
                this.ValueRequired == other.ValueRequired &&
                (!(this.Values != null ^ other.Values != null) &&
                    (this.Values == null || (this.Values.Count == other.Values.Count && this.Values.SequenceEqual(other.Values)))) &&
                (!(this.DefaultValue != null ^ other.DefaultValue != null) &&
                    (this.DefaultValue == null || this.DefaultValue.Equals(other.DefaultValue)));
        }
    }
}
