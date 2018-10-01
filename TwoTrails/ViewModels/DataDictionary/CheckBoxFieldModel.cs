using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwoTrails.Core;

namespace TwoTrails.ViewModels.DataDictionary
{
    public class CheckBoxFieldModel : BaseFieldModel
    {
        public override bool ValueRequired
        {
            get => base.ValueRequired;
            set
            {
                base.ValueRequired = value;

                if (value == true && DefaultValue == null)
                {
                    DefaultValue = false;
                }
            }
        }

        public new bool? DefaultValue
        {
            get
            {
                string defVal = Get<string>();
                return (defVal != null) ? (bool?)bool.Parse(defVal) : null;
            }
            set
            {
                if (ValueRequired)
                {
                    Set(value == true ? "true" : "false");
                }
                else
                {
                    Set(value == true ? "true" : value == false ? "false" : null);
                }
            }
        }


        public CheckBoxFieldModel(DataDictionaryField field) : base(field) { }

        public CheckBoxFieldModel(string cn, string name = null, DataType dataType = DataType.TEXT, bool requiresValue = false)
            : base(cn, FieldType.CheckBox, dataType, name, requiresValue)
        {

        }
    }
}
