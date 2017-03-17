using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using TwoTrails.Core.Media;

namespace TwoTrails
{
    public class ImageTile
    {
        public TtMedia MediaInfo { get; }

        public BitmapImage Image { get; private set; }

        public ImageTile(TtMedia mediaInfo, BitmapImage image)
        {
            MediaInfo = mediaInfo;
            Image = image;
        }
    }
}
