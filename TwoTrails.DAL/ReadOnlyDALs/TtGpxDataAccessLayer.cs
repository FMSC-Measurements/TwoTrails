using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwoTrails.Core;
using TwoTrails.Core.Media;
using TwoTrails.Core.Points;

namespace TwoTrails.DAL
{
    public class TtGpxDataAccessLayer : IReadOnlyTtDataLayer
    {
        public Boolean RequiresUpgrade
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public List<TtGroup> GetGroups()
        {
            throw new NotImplementedException();
        }

        public List<TtMetadata> GetMetadata()
        {
            throw new NotImplementedException();
        }

        public List<TtNmeaBurst> GetNmeaBursts(String pointCN = null)
        {
            throw new NotImplementedException();
        }

        public List<TtImage> GetPictures(String pointCN)
        {
            throw new NotImplementedException();
        }

        public List<TtPoint> GetPoints(String polyCN = null)
        {
            throw new NotImplementedException();
        }

        public List<PolygonGraphicOptions> GetPolygonGraphicOptions()
        {
            throw new NotImplementedException();
        }

        public List<TtPolygon> GetPolygons()
        {
            throw new NotImplementedException();
        }

        public TtProjectInfo GetProjectInfo()
        {
            throw new NotImplementedException();
        }

        public List<TtUserActivity> GetUserActivity()
        {
            throw new NotImplementedException();
        }

        public Boolean HasPolygons()
        {
            throw new NotImplementedException();
        }
    }
}
