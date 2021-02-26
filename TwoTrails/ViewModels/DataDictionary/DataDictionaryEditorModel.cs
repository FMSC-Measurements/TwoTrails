using FMSC.Core.Windows.ComponentModel.Commands;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Xml;
using TwoTrails.Core;
using TwoTrails.DAL;

namespace TwoTrails.ViewModels.DataDictionary
{
    public class DataDictionaryEditorModel
    {
        public ICommand CreateFieldCommand { get; }
        public ICommand MoveUpCommand { get; }
        public ICommand MoveDownCommand { get; }
        public ICommand DeleteFieldCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand FinishCommand { get; }
        public ICommand ImportTemplateCommand { get; }

        private DataDictionaryTemplate origTemplate;
        public ObservableCollection<BaseFieldModel> Fields { get; private set; }

        private int fieldsDeleted, fieldsModified;

        private Window _Window;


        public DataDictionaryEditorModel(Window window, TtProject project)
        {
            _Window = window;

            CreateFieldCommand = new RelayCommand(x =>
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

            CancelCommand = new RelayCommand(x => { _Window.DialogResult = false; _Window.Close(); });

            FinishCommand = new RelayCommand(x =>
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

                        project.DAL.InsertActivity(new TtUserAction(project.Settings.UserName, project.Settings.DeviceName, $"PC: {project.Settings.AppVersion}", DateTime.Now, DataActionType.ModifiedDataDictionary));

                        _Window.DialogResult = true;
                        _Window.Close();
                    }
                }
            });
            
            MoveUpCommand = new RelayCommand(x =>
            {
                if (x is BaseFieldModel bfm)
                {
                    BaseFieldModel prev = Fields[bfm.Order - 1];
                    prev.Order += 1;
                    bfm.Order -= 1;
                    Fields[bfm.Order] = bfm;
                    Fields[prev.Order] = prev;
                }

            }, x => x is BaseFieldModel bfm && bfm.Order > 0);

            MoveDownCommand = new RelayCommand(x =>
            {
                if (x is BaseFieldModel bfm)
                {
                    BaseFieldModel next = Fields[bfm.Order + 1];
                    next.Order -= 1;
                    bfm.Order += 1;
                    Fields[bfm.Order] = bfm;
                    Fields[next.Order] = next;
                }
            }, x => x is BaseFieldModel bfm && bfm.Order < Fields.Count - 1);


            DeleteFieldCommand = new RelayCommand(x =>
            {
                if (x is BaseFieldModel bfm)
                {
                    if (string.IsNullOrEmpty(bfm.Name) || MessageBox.Show($"Delete Field {bfm.Name}?", "Delete Field", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        Fields.RemoveAt(bfm.Order);

                        for (int i = 0; i < Fields.Count; i++)
                        {
                            Fields[i].Order = i;
                        }
                    }
                }
            });

            ImportTemplateCommand = new RelayCommand(x => ImportTemplate());
            
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
                sb.Append($"{fieldsModified} {(fieldsDeleted > 0 ? "other " : "")}field{(fieldsModified > 1 ? "s" : "")} will also be modified (which may result in some data having Default Values added if Value Requied is selected). ");
            }

            sb.Append("Would you like to commit your changes?");

            return sb.ToString();
        }

        private bool ValidateFields()
        {
            fieldsDeleted = origTemplate != null ? fieldsDeleted = origTemplate.Count(of => !Fields.Any(f => f.CN == of.CN)) : 0;
            fieldsModified = origTemplate != null ? Fields.Count(f => origTemplate.HasField(f.CN) && !f.Equals(origTemplate[f.CN])) : 0;
            
            foreach (BaseFieldModel bfm in Fields)
            {
                if (String.IsNullOrWhiteSpace(bfm.Name))
                {
                    MessageBox.Show("One or more Fields does not have a name.", "No Field Name", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }

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

        private void ImportTemplate()
        {
            OpenFileDialog dialog = new OpenFileDialog();

            dialog.DefaultExt = Consts.FILE_EXTENSION_DATA_DICTIONARY;
            dialog.Filter = $"{Consts.FILE_EXTENSION_DATA_DICTIONARY_FILTER}";
            dialog.CheckFileExists = true;

            if (dialog.ShowDialog() == true)
            {
                using (XmlReader reader = XmlReader.Create(dialog.FileName))
                {
                    DataDictionaryField field = null;
                    List<DataDictionaryField> fields = new List<DataDictionaryField>();

                    try
                    {
                        while (reader.Read())
                        {
                            if (reader.IsStartElement())
                            {
                                switch (reader.Name)
                                {
                                    case "Field":
                                        if (field != null)
                                            fields.Add(field);

                                        string cn = reader.GetAttribute("CN");
                                        if (cn == null)
                                            throw new Exception("Field does not have CN");

                                        string dataType = reader.GetAttribute(TwoTrailsSchema.DataDictionarySchema.DataType);
                                        if (dataType == null || !Enum.TryParse(dataType, out DataType edt))
                                            throw new Exception("Field does not have or has invalid DataType");

                                        field = new DataDictionaryField(cn) { DataType = edt };
                                        break;
                                    case TwoTrailsSchema.DataDictionarySchema.FieldType:
                                        reader.Read();
                                        if (Enum.TryParse(reader.Value, out FieldType ft))
                                            field.FieldType = ft;
                                        else throw new Exception("Invalid Field Type Value");
                                        break;
                                    case TwoTrailsSchema.DataDictionarySchema.Name:
                                        reader.Read();
                                        field.Name = reader.Value;
                                        break;
                                    case TwoTrailsSchema.DataDictionarySchema.FieldOrder:
                                        reader.Read();
                                        if (int.TryParse(reader.Value, out int order))
                                            field.Order = order;
                                        else throw new Exception("Invalid Field Order Value");
                                        break;
                                    case TwoTrailsSchema.DataDictionarySchema.ValueRequired:
                                        reader.Read();
                                        if (bool.TryParse(reader.Value, out bool req))
                                            field.ValueRequired = req;
                                        else throw new Exception("Invalid Value Required Value");
                                        break;
                                    case TwoTrailsSchema.DataDictionarySchema.Flags:
                                        reader.Read();
                                        if (int.TryParse(reader.Value, out int flags))
                                            field.Flags = flags;
                                        else throw new Exception("Invalid Flags Value");
                                        break;
                                    case TwoTrailsSchema.DataDictionarySchema.DefaultValue:
                                        reader.Read();
                                        if (!string.IsNullOrWhiteSpace(reader.Value))
                                        {
                                            switch (field.DataType)
                                            {
                                                case DataType.BOOLEAN:
                                                    field.DefaultValue = bool.Parse(reader.Value);
                                                    break;
                                                case DataType.INTEGER:
                                                    field.DefaultValue = int.Parse(reader.Value);
                                                    break;
                                                case DataType.DECIMAL:
                                                    field.DefaultValue = decimal.Parse(reader.Value);
                                                    break;
                                                case DataType.FLOAT:
                                                    field.DefaultValue = double.Parse(reader.Value);
                                                    break;
                                                case DataType.TEXT:
                                                    field.DefaultValue = reader.Value;
                                                    break;
                                            }
                                        }
                                        break;
                                    case "Value":
                                        reader.Read();
                                        if (field.Values == null)
                                            field.Values = new List<string>();
                                        field.Values.Add(reader.Value);
                                        break;
                                }
                            }
                        }

                        if (field != null)
                            fields.Add(field);

                        Action<DataDictionaryField> AddField = f =>
                        {
                            switch (f.FieldType)
                            {
                                case FieldType.ComboBox: Fields.Add(new ComboBoxFieldModel(f) { Order = Fields.Count }); break;
                                case FieldType.TextBox: Fields.Add(new TextBoxFieldModel(f) { Order = Fields.Count }); break;
                                case FieldType.CheckBox: Fields.Add(new CheckBoxFieldModel(f) { Order = Fields.Count }); break;
                            }
                        };

                        if (Fields.Count > 0)
                        {
                            MessageBoxResult mbr = MessageBox.Show(@"There are already fields in this DataDictionary. Would you like to Add the imported fields to the current ones (Yes) or Replace
the fields with the imported ones (No)", "Add or Replace Fields", MessageBoxButton.YesNoCancel, MessageBoxImage.Question, MessageBoxResult.Cancel);

                            if (mbr == MessageBoxResult.Yes)
                            {
                                foreach (DataDictionaryField f in fields)
                                {
                                    BaseFieldModel bfm = Fields.FirstOrDefault(fc => fc.CN == f.CN);
                                    if (bfm != null)
                                    {
                                        if (MessageBox.Show($"A Field named {bfm.Name} aleady exists. Would you like to overwrite that field?",
                                            "Fields Already Exists", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                                        {
                                            Fields.RemoveAt(bfm.Order);

                                            for (int i = bfm.Order; i < Fields.Count; i++)
                                                Fields[i].Order = i;

                                            AddField(f);
                                        }
                                    }
                                }
                            }
                            else if (mbr == MessageBoxResult.No)
                            {
                                Fields.Clear();
                                foreach (DataDictionaryField f in fields)
                                    AddField(f);
                            }
                        }
                        else
                            foreach (DataDictionaryField f in fields)
                                AddField(f);

                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message, "DataDictionaryEditorModel:ImportTemplate");
                        MessageBox.Show("There was an error importing the DataDictionary Template. See Log for details.", "DataDictionary Import Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
    }
}
