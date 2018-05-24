using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace FMSC.Core.Controls
{
    public static class ControlUtils
    {

        public static bool TextIsInteger(object sender, TextCompositionEventArgs e)
        {
            return string.IsNullOrEmpty(e.Text) ? false : !e.Text.All(char.IsDigit);
        }


        public static bool TextIsDouble(object sender, TextCompositionEventArgs e)
        {
            return string.IsNullOrEmpty(e.Text) ? false : !(e.Text.All(x => char.IsDigit(x) || x == '.') &&
                !(
                    (sender is TextBox tb) &&
                    (tb.Text.Contains(".") && e.Text.Contains(".")))
                );
        }
    }
}
