using FMSC.Core;
using FMSC.GeoSpatial.UTM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TwoTrails.Core;
using TwoTrails.Core.Points;

namespace TwoTrails.ViewModels
{
    public class DataCorrectionModel
    {

        public DataCorrectionModel(TtProject project)
        {
            
        }


        private DataErrors Analyze(ITtManager manager)
        {
            DataErrors errors = DataErrors.None;


            return errors;
        }


        public static void RezonePoints(TtHistoryManager manager, IEnumerable<GpsPoint> points = null)
        {
            manager.RezonePoints(points != null ? points : manager.Points.Where(p => p.IsGpsType()).Cast<GpsPoint>());
        }


        [Flags]
        public enum DataErrors
        {
            None = 0,
            MiszonedPoints = 1 << 0,
            OrphanedQuondams = 1 << 1,
            NullAdjLocs = 1 << 2,
            UnusedPolygons = 1 << 3,
            UnusedMetadata = 1 << 4,
            UnusedGroups = 1 << 5,
            UnadjustedPoints = 1 << 6
        }
    }
}
