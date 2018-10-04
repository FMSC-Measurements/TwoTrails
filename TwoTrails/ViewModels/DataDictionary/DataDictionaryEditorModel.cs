using FMSC.Core.ComponentModel.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using TwoTrails.Core;

namespace TwoTrails.ViewModels.DataDictionary
{
    public class DataDictionaryEditorModel
    {
        public ICommand CreateField { get; }
        public ICommand DeleteField { get; }
        public ICommand Cancel { get; }
        public ICommand Finish { get; }
        public ICommand ImportTemplate { get; }

        private DataDictionaryTemplate origTemplate;
        public ObservableCollection<BaseFieldModel> Fields { get; private set; }

        private int fieldsDeleted, fieldsModified;

        private Window _Window;


        public DataDictionaryEditorModel(Window window, TtProject project)
        {
            _Window = window;

            CreateField = new RelayCommand(x =>
            {
                if (x is FieldType fieldType)
                {
                    switch (fieldType)
                    {
                        case FieldType.ComboBox: Fields.Add(new ComboBoxFieldModel(Guid.NewGuid().ToString())); break;
                        case FieldType.TextBox: Fields.Add(new TextBoxFieldModel(Guid.NewGuid().ToString())); break;
                        case FieldType.CheckBox: Fields.Add(new CheckBoxFieldModel(Guid.NewGuid().ToString())); break;
                    }
                }
            });

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

            Cancel = new RelayCommand(x => { _Window.DialogResult = false; _Window.Close(); });

            Finish = new RelayCommand(x =>
            {
                if (ValidateFields())
                {
                    if ((fieldsDeleted + fieldsModified < 1) || MessageBox.Show(GenerateModifyPrompt(), "DataDictionary Changes", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                    {
                        DataDictionaryTemplate template = new DataDictionaryTemplate(Fields.Select(f => f.CreateDataDictionaryField()));

                        if (project.DAL.HasDataDictionary)
                        {
                            if (!project.DAL.ModifyDataDictionary(template))
                            {
                                MessageBox.Show("There was a problem Modifying the DataDictionary. Please see the log file for details.");
                                return;
                            }
                        }
                        else
                        {
                            if (!project.DAL.CreateDataDictionary(template))
                            {
                                MessageBox.Show("There was a problem Creating the DataDictionary. Please see the log file for details.");
                                return;
                            }
                        }

                        project.DAL.InsertActivity(new TtUserAction(project.Settings.UserName, project.Settings.DeviceName, DateTime.Now, DataActionType.ModifiedDataDictionary));

                        _Window.DialogResult = true;
                        _Window.Close();
                    }
                }
            });

            ImportTemplate = new RelayCommand(x =>
            {
                //todo ability to replace/merge
            });
            
            Fields = new ObservableCollection<BaseFieldModel>();

            if (project.DAL.HasDataDictionary)
            {
                origTemplate = project.DAL.GetDataDictionaryTemplate();
                foreach (DataDictionaryField field in origTemplate)
                {
                    switch (field.FieldType)
                    {
                        case FieldType.ComboBox: Fields.Add(new ComboBoxFieldModel(field)); break;
                        case FieldType.TextBox: Fields.Add(new TextBoxFieldModel(field)); break;
                        case FieldType.CheckBox: Fields.Add(new CheckBoxFieldModel(field)); break;
                    }
                }
            }
        }

        private string GenerateModifyPrompt()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("You are about to modify the DataDictionary. This actiona can not be undone. ");

            if (fieldsDeleted > 0)
            {
                sb.Append($"{fieldsDeleted} field{(fieldsDeleted > 1 ? "s" : "")} will be removed resulting in some point data being deleted. ");
            }

            if (fieldsModified > 0)
            {
                sb.Append($"{fieldsModified} {(fieldsDeleted > 0 ? "other " : "")} field{(fieldsModified > 1 ? "s" : "")} will also be modified (which may result in some data having Default Values added if Value Requied is selected). ");
            }

            sb.Append("Would you like to commit your changes?");

            return sb.ToString();
        }

        private bool ValidateFields()
        {
            //todo
            return false;
        }
    }
}
