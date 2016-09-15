using CSUtil.ComponentModel;
using FMSC.Core;
using FMSC.Core.ComponentModel.Commands;
using FMSC.GeoSpatial.UTM;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using TwoTrails.Core;
using TwoTrails.Core.Points;
using TwoTrails.Utils;

namespace TwoTrails.ViewModels
{
    public class CreatePlotsModel : NotifyPropertyChangedEx
    {
        private ITtManager _Manager;

        public ICommand GenerateCommand { get; }
        public ICommand CloseCommand { get; }

        public ObservableCollection<TtPolygon> Polygons { get; }
        public ObservableCollection<TtPoint> Points
        {
            get { return Get<ObservableCollection<TtPoint>>(); }
            set { Set(value); }
        }

        public TtPolygon SelectedPolygon
        {
            get { return Get<TtPolygon>(); }
            set { Set(value, () => Points = new ObservableCollection<TtPoint>(_Manager.GetPoints(value.CN))); }
        }

        public TtPoint SelectedPoint { get { return Get<TtPoint>(); } set { Set(value); } }

        public Distance UomDistance { get { return Get<Distance>(); } set { Set(value); } }


        public int GridX { get { return Get<int>(); } set { Set(value); } }
        public int GridY { get { return Get<int>(); } set { Set(value); } }

        public int Tilt { get { return Get<int>(); } set { Set(value); } }


        public bool? SamplePoints { get { return Get<bool?>(); } set { Set(value); } }

        public SampleType SampleTypeItem { get { return Get<SampleType>(); } set { Set(value); } }
        public int SampleTypeIndex { get { return Get<int>(); } set { Set(value); } }
        public int SampleAmount { get { return Get<int>(); } set { Set(value); } }

        public bool? BoundaryBuffer { get { return Get<bool?>(); } set { Set(value); } }
        public int BufferAmount { get { return Get<int>(); } set { Set(value); } }


        public CreatePlotsModel(ITtManager manager, Window window)
        {
            _Manager = manager;

            Polygons = new ObservableCollection<TtPolygon>(
                manager.GetPolyons().Where(p => manager.GetPoints(p.CN).HasAtLeast(2, pt => pt.IsBndPoint())));

            UomDistance = Distance.FeetTenths;

            if (Polygons.Count > 0)
                SelectedPolygon = Polygons[0];

            SamplePoints = false;
            SampleTypeItem = SampleType.Percent;

            BoundaryBuffer = false;

            SelectedPoint = null;

            GenerateCommand = new RelayCommand(x => ValidateSettings());
            CloseCommand = new RelayCommand(x => window.Close());
        }

        private void ValidateSettings()
        {
            if (GridX < 1 || GridY < 1)
            {
                MessageBox.Show("Grid Interval must be greater than 0");
                return;
            }

            if (Tilt < 45 || Tilt > 45)
            {
                MessageBox.Show("Y Axis Tilt must be between -45° and +45°");
                return;
            }

            if (SamplePoints == true &&
                (SampleTypeItem == SampleType.Percent && SampleAmount < 1 || SampleAmount > 100) ||
                (SampleTypeItem == SampleType.Points && SampleAmount < 1))
            {
                MessageBox.Show(String.Format("The Sample Amount must ",
                    SampleTypeItem == SampleType.Percent ? "be between 1% and 100%." : "be greate than 1 point."));
                return;
            }

            if (BoundaryBuffer == true && BufferAmount < 1)
            {
                MessageBox.Show("The Boundary Buffer must be greater than 0.");
                return;
            }

            List<TtPolygon> polygons = _Manager.GetPolyons();
            string gPolyName = String.Format("{0}_Plts", SelectedPolygon.Name);

            TtPolygon poly = null;

            try
            {
                polygons.First(p => p.Name == gPolyName);
            }
            catch
            {
                //
            }


            if (poly != null)
            {
                if (MessageBox.Show(String.Format("Plots '{0}' already exist. Would you like to overwrite the plots?", gPolyName), "Plots Already Exist", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
                    return;

                _Manager.DeletePoints(_Manager.GetPoints(poly.CN));
            }
            else
            {
                poly = new TtPolygon()
                {
                    Name = gPolyName,
                    PointStartIndex = (polygons.Count + 1) * 1000 + 10,
                    Increment = 1
                };

                _Manager.AddPolygon(poly);
            }

            GeneratePoints(poly);
        }

        private void GeneratePoints(TtPolygon poly)
        {
            double gridX = FMSC.Core.Convert.Distance(GridX, Distance.Meters, UomDistance);
            double gridY = FMSC.Core.Convert.Distance(GridY, Distance.Meters, UomDistance);

            double angle = Tilt * -1;

            poly.Description = String.Format("Angle: {0}°, Grid({3}) X:{1} Y:{2}, Created from Polygon: {4}",
                        Tilt, GridX, GridY, UomDistance.ToStringAbv(), SelectedPolygon.Name);

            IEnumerable<TtPoint> points = _Manager.GetPoints(SelectedPolygon.CN).Where(p => p.IsBndPoint());

            int zone = points.First().Metadata.Zone;

            UtmExtent extent = TtUtils.GetBoundaries(points);

            Random rand = new Random();
            UTMCoords startCoords = SelectedPoint != null ?
                TtUtils.GetCoords(SelectedPoint, zone) :
                new UTMCoords(
                    (rand.NextDouble() * (extent.East - extent.West) + extent.West),
                    (rand.NextDouble() * (extent.North - extent.South) + extent.South),
                    zone
                );


        }
    }
}
