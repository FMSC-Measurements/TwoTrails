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
                        if (DefaultValue != null && !int.TryParse(DefaultValue, out int val))
                        {
                            if (decimal.TryParse(DefaultValue, out decimal dval) && dval == Math.Abs(dval))
                                DefaultValue = ((int)dval).ToString();
                            else
                                DefaultValue = null;
                        
                            OnPropertyChanged(nameof(DefaultValue));
                        }
                    }
                    else if (value == DataType.DECIMAL || value == DataType.FLOAT)
                    {
                        if (DefaultValue != null && !double.TryParse(DefaultValue, out double val))
                        {
                            DefaultValue = null;
                            OnPropertyChanged(nameof(DefaultValue));
                        }
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
