using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwoTrails.Core;
using TwoTrails.Core.Points;

namespace TwoTrails.DAL
{
    //todo create Kml AccessLayer
    public class TtKmlDataAccessLayer : IReadOnlyTtDataLayer
    {
        public bool RequiresUpgrade => false;

        public IEnumerable<TtGroup> GetGroups()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<TtMetadata> GetMetadata()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<TtNmeaBurst> GetNmeaBursts(string pointCN = null)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<TtPoint> GetPoints(string polyCN = null, bool linked = false)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<PolygonGraphicOptions> GetPolygonGraphicOptions()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<TtPolygon> GetPolygons()
        {
            throw new NotImplementedException();
        }

        public TtProjectInfo GetProjectInfo()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<TtUserActivity> GetUserActivity()
        {
            throw new NotImplementedException();
        }

        public bool HasPolygons()
        {
            throw new NotImplementedException();
        }


        public TtKmlDataAccessLayer(string fileName)
        {

        }
    }
}
