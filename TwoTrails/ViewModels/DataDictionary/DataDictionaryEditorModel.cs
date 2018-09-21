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

            Fields.Add(new ComboBoxFieldModel(Guid.NewGuid().ToString(), "ComboBox"));
            Fields.Add(new TextBoxFieldModel(Guid.NewGuid().ToString(), "TextBox"));
            Fields.Add(new CheckBoxFieldModel(Guid.NewGuid().ToString(), "CheckBox"));
        }

    }
}
