using CSUtil.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwoTrails.Core;

namespace TwoTrails.ViewModels.DataDictionary
{
    public class DataDictionaryEditorModel : NotifyPropertyChangedEx
    {
        public ObservableCollection<BaseFieldModel> Fields { get; private set; }
        

        public DataDictionaryEditorModel(TtProject project)
        {
            Fields = new ObservableCollection<BaseFieldModel>();

            Fields.Add(new ComboBoxFieldModel(Guid.NewGuid().ToString(), "ComboBox")
            {
                ValueRequired = true,
                DataType = DataType.FLOAT,
                Values = new List<string>()
                {
                    "32.1",
                    "233.2",
                    "1.23",
                    "43.2",
                    "331.2"
                },
                DefaultValue = "1.23"
            });
            Fields.Add(new TextBoxFieldModel(Guid.NewGuid().ToString(), "TextBox")
            {
                ValueRequired = true,
                DataType = DataType.TEXT,
                DefaultValue = "something"
            });
            Fields.Add(new CheckBoxFieldModel(Guid.NewGuid().ToString(), "CheckBox")
            {
                ValueRequired = false,
                DefaultValue = null
            });
        }


        private void ValidateFields()
        {

        }
    }
}
