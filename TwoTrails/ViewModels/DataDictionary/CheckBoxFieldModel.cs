using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwoTrails.ViewModels.DataDictionary
{
    public class CheckBoxFieldModel : BaseFieldModel
    {
        public CheckBoxFieldModel(string cn, string name = null, bool requiresValue = false) : base(cn, Core.FieldType.CheckBox, name, requiresValue)
        {

        }

    }
}
