using System;
using System.Collections.Generic;
using System.Windows.Media.Imaging;
using TwoTrails.Core;
using TwoTrails.Core.Media;
using TwoTrails.Core.Points;

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
