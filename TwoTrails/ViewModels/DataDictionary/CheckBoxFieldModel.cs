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
            get => Get<bool?>(); 
            set => Set(ValueRequired ? value == true : value); 
        }


        public CheckBoxFieldModel(DataDictionaryField field) : base(field) { }

        public CheckBoxFieldModel(string cn, string name = null, bool requiresValue = false)
            : base(cn, FieldType.CheckBox, DataType.BOOLEAN, name, requiresValue)
        {

        }
    }
}
