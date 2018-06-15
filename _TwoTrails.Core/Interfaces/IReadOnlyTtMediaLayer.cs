using System;
using System.Collections.Generic;
using System.Windows.Media.Imaging;
using TwoTrails.Core.Media;

namespace TwoTrails.DAL
{
    public interface IReadOnlyTtMediaLayer
    {
        #region Media
        IEnumerable<TtImage> GetImages(String pointCN = null);
        #endregion

        BitmapImage GetImageData(TtImage image);

        byte[] GetRawImageData(TtImage image);
    }
}
