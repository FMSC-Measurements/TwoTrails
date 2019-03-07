using System;
using TwoTrails.Core.Media;

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
