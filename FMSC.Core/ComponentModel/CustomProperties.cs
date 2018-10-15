using FMSC.Core.ComponentModel.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;

namespace FMSC.Core.ComponentModel
{
    public static class CustomProperties
    {
    }

    public static class RelayCommandProperties
    {
        public static object GetParameter(DependencyObject obj)
        {
            return (object)obj.GetValue(ParameterProperty);
        }

        public static void SetParameter(DependencyObject obj, object value)
        {
            obj.SetValue(ParameterProperty, value);
        }

        public static readonly DependencyProperty ParameterProperty = DependencyProperty.RegisterAttached("Parameter", typeof(object), typeof(RelayCommandProperties), new UIPropertyMetadata(null, ParameterChanged));

        private static void ParameterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var button = d as ButtonBase;
            if (button == null)
            {
                return;
            }
            
            button.CommandParameter = e.NewValue;

            var cmd = button.Command as ICanExecuteChanged;
            if (cmd != null)
            {
                cmd.RaiseCanExecuteChanged();

                if (button.CommandParameter is INotifyPropertyChanged inpcObj)
                    inpcObj.PropertyChanged += (s, pe) => cmd.RaiseCanExecuteChanged();
            }
        }
    }
}
