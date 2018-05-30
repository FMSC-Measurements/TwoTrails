using System;
using System.Collections.Generic;
using TwoTrails.Core;
using TwoTrails.Core.Media;
using TwoTrails.Core.Points;

namespace TwoTrails.DAL
{
    public interface ITtMediaLayer : IReadOnlyTtMediaLayer
    {
        String FilePath { get; }
        
        Version GetDataVersion();

        #region Media
        bool InsertMedia(TtMedia media);

        bool UpdateMedia(TtMedia media);

        bool DeleteMedia(TtMedia media);
        #endregion

        #region Util
        void Clean();
        #endregion
    }
}
