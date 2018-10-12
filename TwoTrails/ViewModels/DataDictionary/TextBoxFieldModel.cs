using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwoTrails.Core;

namespace TwoTrails.ViewModels.DataDictionary
{
    public class TextBoxFieldModel : BaseFieldModel
    {
        public override DataType DataType
        {
            get => base.DataType;
            set
            {
                if (base.DataType != value)
                {
                    if (value == DataType.INTEGER)
                    {
                        if (base.DataType == DataType.BOOLEAN)
                            DefaultValue = 0;
                        else if (base.DataType == DataType.DECIMAL)
                            DefaultValue = DefaultValue != null ? (int)(decimal)DefaultValue : 0;
                        else if (base.DataType == DataType.FLOAT)
                            DefaultValue = DefaultValue != null ? Math.Round((double)DefaultValue) : 0;
                        else if (base.DataType == DataType.TEXT)
                        {
                            if (DefaultValue != null && !int.TryParse(DefaultValue as string, out int val))
                            {
                                if (decimal.TryParse(DefaultValue as string, out decimal dval) && dval == Math.Abs(dval))
                                    DefaultValue = ((int)dval);
                                else
                                    DefaultValue = 0;
                            }
                        }

                        OnPropertyChanged(nameof(DefaultValue));
                    }
                    else if (value == DataType.DECIMAL)
                    {
                        if (base.DataType == DataType.BOOLEAN)
                            DefaultValue = (decimal)0.0;
                        else if (base.DataType == DataType.INTEGER)
                            DefaultValue = (decimal)(int)DefaultValue;
                        else if (base.DataType == DataType.FLOAT)
                            DefaultValue = (decimal)(double)DefaultValue;
                        else if (base.DataType == DataType.TEXT)
                        {
                            if (DefaultValue != null && !decimal.TryParse(DefaultValue as string, out decimal val))
                            {
                                DefaultValue = val;
                            }
                            else
                                DefaultValue = (decimal)0.0;
                        }

                        OnPropertyChanged(nameof(DefaultValue));
                    }
                    else if (value == DataType.FLOAT)
                    {
                        if (base.DataType == DataType.BOOLEAN)
                            DefaultValue = 0d;
                        else if (base.DataType == DataType.INTEGER)
                            DefaultValue = (double)(int)DefaultValue;
                        else if (base.DataType == DataType.DECIMAL)
                            DefaultValue = (double)(decimal)DefaultValue;
                        else if (base.DataType == DataType.TEXT)
                        {
                            if (DefaultValue != null && !double.TryParse(DefaultValue as string, out double val))
                            {
                                DefaultValue = val;
                            }
                            else
                                DefaultValue = 0d;
                        }

                        OnPropertyChanged(nameof(DefaultValue));
                    }
                }

                base.DataType = value;
            }
        }


        public TextBoxFieldModel(DataDictionaryField field) : base(field) { }

        public TextBoxFieldModel(string cn, string name = null, DataType dataType = DataType.TEXT, bool requiresValue = false)
            : base(cn, FieldType.TextBox, dataType, name, requiresValue)
        {

        }
    }
}
