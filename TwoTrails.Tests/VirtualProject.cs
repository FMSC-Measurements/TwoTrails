using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwoTrails.Core;
using TwoTrails.DAL;
using TwoTrails.Tests.DAL;
using TwoTrails.Tests.Settings;

namespace TwoTrails.Tests
{
    public class VirtualProject : IDisposable
    {
        public TtManager Manager { get; }
        public TtHistoryManager HistoryManager { get; }

        public ITtDataLayer DAL { get; }
        public ITtMediaLayer MAL { get; }

        public ITtSettings Settings { get; }


        public VirtualProject(ITtSettings? settings)
        {
            DAL = new VirtualDataAccessLayer();
            MAL = new VirtualMediaAccessLayer();

            if (settings == null)
            {
                Settings = new TtSettings(new VirtualDeviceSettings(), new VirtualMetadataSettings(), new TtPolygonGraphicSettings());
            }
            else
            {
                Settings = settings;
            }

            Manager = new TtManager(DAL, MAL, Settings);
            HistoryManager = new TtHistoryManager(Manager);
        }

        public void Dispose()
        {
            //
        }
    }
}
