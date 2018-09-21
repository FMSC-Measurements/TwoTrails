using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwoTrails.ViewModels.DataDictionary
{
    public class ComboBoxFieldModel : BaseFieldModel
    {
        public ComboBoxFieldModel(string cn, string name = null, bool requiresValue = false) : base(cn, Core.FieldType.ComboBox, name, requiresValue)
        {

        }
    }
}
