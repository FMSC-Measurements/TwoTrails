using CSUtil.ComponentModel;
using FMSC.Core.ComponentModel.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using TwoTrails.Core;

namespace TwoTrails.ViewModels.DataDictionary
{
    public class DataDictionaryEditorModel : NotifyPropertyChangedEx
    {
        public ICommand DeleteField { get; }

        public ObservableCollection<BaseFieldModel> Fields { get; private set; }
        

        public DataDictionaryEditorModel(TtProject project)
        {
            DeleteField = new RelayCommand(x =>
            {
                if (x is BaseFieldModel bfm)
                {
                    if (string.IsNullOrEmpty(bfm.Name) || MessageBox.Show($"Delete Field {bfm.Name}?", "Delete Field", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        Fields.RemoveAt(bfm.Order);

                        for (int i = bfm.Order; i < Fields.Count; i++)
                        {
                            Fields[i].Order = i;
                        }
                    }
                }
            });
            
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
                DefaultValue = "1.23",
                Order = 0
            });
            Fields.Add(new TextBoxFieldModel(Guid.NewGuid().ToString(), "TextBox")
            {
                ValueRequired = true,
                DataType = DataType.TEXT,
                DefaultValue = "something",
                Order = 1
            });
            Fields.Add(new CheckBoxFieldModel(Guid.NewGuid().ToString(), "CheckBox")
            {
                ValueRequired = false,
                DefaultValue = null,
                Order = 2
            });
            Fields.Add(new TextBoxFieldModel(new DataDictionaryField() { DataType = DataType.TEXT, DefaultValue = "test", Name = "Test Name", Order = 3, ValueRequired = true }));
        }


        private void ValidateFields()
        {

        }
    }
}
