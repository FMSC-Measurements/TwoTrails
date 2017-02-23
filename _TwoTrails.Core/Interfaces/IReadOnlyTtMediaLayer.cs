using System;
using System.Collections.Generic;
using TwoTrails.Core;
using TwoTrails.Core.Media;
using TwoTrails.Core.Points;

namespace TwoTrails.DAL
{
    public interface IReadOnlyTtMediaLayer
    {
        #region Media
        IEnumerable<TtImage> GetPictures(String pointCN);
        #endregion
    }
}
