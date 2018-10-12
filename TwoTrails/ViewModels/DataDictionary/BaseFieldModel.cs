using CSUtil.ComponentModel;
using FMSC.Core.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
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
            set { Set(value); }
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
            Values = field.Values ?? new List<string>();
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
            Values = new List<string>();
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
                this.Values.SequenceEqual(bfm.Values) &&
                this.DefaultValue == bfm.DefaultValue &&
                this.DataType == bfm.DataType &&
                this.ValueRequired == bfm.ValueRequired;
        }

        public bool Equals(DataDictionaryField other)
        {
            return
                this.CN == other.CN &&
                this.Name == other.Name &&
                this.Order == other.Order &&
                this.FieldType == other.FieldType &&
                this.Flags == other.Flags &&
                (this.Values.Count > 0 && (other.Values != null && other.Values.Count > 0)) &&
                this.Values.SequenceEqual(other.Values) &&
                this.DefaultValue.Equals(other.DefaultValue) &&
                this.DataType == other.DataType &&
                this.ValueRequired == other.ValueRequired;
        }
    }
}
