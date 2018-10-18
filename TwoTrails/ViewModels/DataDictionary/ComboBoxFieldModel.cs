using FMSC.Core;
using FMSC.Core.ComponentModel.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using TwoTrails.Core;
using TwoTrails.Dialogs;

namespace TwoTrails.ViewModels.DataDictionary
{
    public class ComboBoxFieldModel : BaseFieldModel
    {
        public ICommand EditValues { get; }

        public override DataType DataType
        {
            get => base.DataType;
            set
            {
                if (base.DataType != value)
                {
                    if (value == DataType.INTEGER)
                    {
                        bool valuesChanged = false;
                        for (int i = 0; i < Values.Count; i++)
                        {
                            if (!int.TryParse(Values[i], out int val))
                            {
                                if (decimal.TryParse(Values[i], out decimal dval) && dval == Math.Abs(dval))
                                {
                                    Values[i] = ((int)dval).ToString();
                                    continue;
                                }

                                if (DefaultValue == Values[i])
                                {
                                    DefaultValue = null;
                                    OnPropertyChanged(nameof(DefaultValue));
                                }

                                Values.RemoveAt(i);
                                i--;
                                valuesChanged = true;
                            }
                        }

                        if (valuesChanged)
                            OnPropertyChanged(nameof(Values));
                    }
                    else if (value == DataType.DECIMAL || value == DataType.FLOAT)
                    {
                        bool valuesChanged = false;
                        for (int i = 0; i < Values.Count; i++)
                        {
                            if (!double.TryParse(Values[i], out double val))
                            {
                                if (DefaultValue == Values[i])
                                {
                                    DefaultValue = null;
                                    OnPropertyChanged(nameof(DefaultValue));
                                }

                                Values.RemoveAt(i);
                                i--;
                                valuesChanged = true;
                            }
                        }

                        if (valuesChanged)
                            OnPropertyChanged(nameof(Values));
                    }
                }

                base.DataType = value;
            }
        }

        public bool IsEditable
        {
            get => Flags.GetBit(0);
            set
            {
                if (value)
                    Flags |= 1 << 0;
                else
                    Flags &= ~(1 << 0);
            }
        }

        public override bool ValueRequired
        {
            get => base.ValueRequired;
            set
            {
                base.ValueRequired = value;

                if (base.ValueRequired)
                    UseDefaultValue = true;
            }
        }

        public bool UseDefaultValue
        {
            get => Get<bool>();
            set
            {
                if (Set(value))
                {
                    if (!value)
                        DefaultValue = null;
                }
            }
        }

        public ComboBoxFieldModel(DataDictionaryField field) : base(field)
        {
            EditValues = new RelayCommand(x =>
            {
                EditValuesDialog dialog = new EditValuesDialog(Values, DataType);
                if (dialog.ShowDialog() == true)
                {
                    Values = dialog.Values;
                }
            });

            if (DefaultValue != null)
                UseDefaultValue = true;
        }

        public ComboBoxFieldModel(string cn, string name = null, bool requiresValue = false)
            : base(cn, FieldType.ComboBox, DataType.TEXT, name, requiresValue)
        {
            EditValues = new RelayCommand(x =>
            {
                EditValuesDialog dialog = new EditValuesDialog(Values, DataType);
                if (dialog.ShowDialog() == true)
                {
                    Values = dialog.Values;
                }
            });
            
            if (DefaultValue != null)
                UseDefaultValue = true;
        }
    }
}
