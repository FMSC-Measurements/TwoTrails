using CSUtil;
using CSUtil.ComponentModel;
using FMSC.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwoTrails.Core;
using TwoTrails.Core.Points;

namespace TwoTrails.ViewModels
{
    public class LogDeckCalculatorModel : NotifyPropertyChangedEx
    {
        private TtProject _Project;

        public double LogLength { get => Get<double>(); set => Set(value, UpdateVolume); }
        public double Defect { get => Get<double>(); set => Set(value, UpdateVolume); }
        public double EdgeBuffer { get => Get<double>(); set => Set(value, UpdateVolume); }
        public double Void { get => Get<double>(); set => Set(value, UpdateVolume); }


        public double Perimeter { get => Get<double>(); set => Set(value); }
        public double FaceArea { get => Get<double>(); set => Set(value); }
        public double NetVolume { get => Get<double>(); set => Set(value); }
        public double GrossVolume { get => Get<double>(); set => Set(value); }

        public Distance Distance { get => Get<Distance>(); set => Set(value, () => {
            if (_Project.Settings.MetadataSettings is MetadataSettings ms) { ms.Distance = value; ms.SaveSettings(); }
        }); }
        public Volume Volume { get => Get<Volume>(); set => Set(value, () => {
            if (_Project.Settings.MetadataSettings is MetadataSettings ms) { ms.Volume = value; ms.SaveSettings(); }
        }); }

        public ReadOnlyCollection<TtPolygon> Polygons { get; }

        public TtPolygon Polygon { get => Get<TtPolygon>(); set => Set(value, UpdateVolume); }


        public LogDeckCalculatorModel(TtProject project)
        {
            _Project = project;

            Polygons = new ReadOnlyCollection<TtPolygon>(
                _Project.Manager.GetPolygons().Where(p => _Project.Manager.GetPoints(p.CN).HasAtLeast(2, pt => pt.IsBndPoint())).ToList());

            Distance = project.Settings.MetadataSettings.Distance;
            Volume = project.Settings.MetadataSettings.Volume;

            if (Polygons.Count > 0)
                Polygon = Polygons[0];
        }


        private void UpdateVolume()
        {

        }
    }
}
