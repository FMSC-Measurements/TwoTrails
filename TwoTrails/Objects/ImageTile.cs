using FMSC.Core.ComponentModel.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using TwoTrails.Core.Media;

namespace TwoTrails
{
    public class ImageTile
    {
        public ICommand OnClickCommand { get; }

        public TtImage ImageInfo { get; }

        public BitmapImage Image { get; private set; }

        public ImageTile(TtImage imageInfo, BitmapImage image, Action<object> action = null)
        {
            ImageInfo = imageInfo;
            Image = image;

            if (action != null)
                OnClickCommand = new RelayCommand(action);
        }
    }
}
