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

                    Fields.Last().Order = Fields.Count - 1;
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
            sb.Append("You are about to modify the DataDictionary. This action can not be undone. ");

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
            fieldsDeleted = origTemplate != null ? fieldsDeleted = origTemplate.Count(of => !Fields.Any(f => f.CN == of.CN)) : 0;
            //fieldsModified = 0;
            fieldsModified = origTemplate != null ? Fields.Count(f => origTemplate.HasField(f.CN) && !f.Equals(origTemplate[f.CN])) : 0;

            foreach (BaseFieldModel bfm in Fields)
            {
                if (String.IsNullOrWhiteSpace(bfm.Name))
                {
                    MessageBox.Show("One or more Fields does not have a name.", "No Field Name", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }

                //if (origTemplate.HasField(bfm.CN) && !bfm.Equals(origTemplate[bfm.CN]))
                //{
                //    fieldsModified++;
                //}

                if (bfm.ValueRequired && bfm.DefaultValue == null)
                {
                    MessageBox.Show($"Field {bfm.Name} is marked as Value Requied but does not have a Default Value.", "No Default Value", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }

                if (bfm is ComboBoxFieldModel cbfm && !cbfm.IsEditable)
                {
                    if (!cbfm.Values.Any())
                    {
                        MessageBox.Show($"Field {cbfm.Name} does not have any Values. It needs to have Values or be marked as Editable.", "No Values", MessageBoxButton.OK, MessageBoxImage.Error);
                        return false;
                    }
                    else if (cbfm.Values.Count() < 2)
                    {
                        if (MessageBox.Show($@"Field {cbfm.Name} has only one Value and is not marked as Editable. You will not be able to select 
Multiple Values for this field. Do you want to add more options to this field?", "One Value Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.Yes) == MessageBoxResult.Yes)
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }
    }
}
