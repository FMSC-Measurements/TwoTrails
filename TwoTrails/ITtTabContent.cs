using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace TwoTrails
{
    public interface ITtTabContent
    {
        TtTabModel TabModel { get; }
    }
}
