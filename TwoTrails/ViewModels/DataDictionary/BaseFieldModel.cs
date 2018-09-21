using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwoTrails.Core;

namespace TwoTrails.ViewModels.DataDictionary
{
    public class BaseFieldModel
    {
        protected DataDictionaryField _DataDictionaryField;

        public String Name { get { return _DataDictionaryField.Name; } set { _DataDictionaryField.Name = value; } }

        public bool RequiresValue { get { return _DataDictionaryField.ValueRequired; } set { _DataDictionaryField.ValueRequired = value; } }


        public BaseFieldModel(string cn, FieldType fieldType, string name = null, bool requiresValue = false)
        {
            _DataDictionaryField = new DataDictionaryField(cn) {
                FieldType = fieldType,
                Name = name,
                ValueRequired = requiresValue
            };
        }



    }
}
