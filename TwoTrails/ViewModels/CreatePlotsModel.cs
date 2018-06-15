using CSUtil;
using CSUtil.ComponentModel;
using FMSC.Core;
using FMSC.Core.ComponentModel.Commands;
using FMSC.GeoSpatial.UTM;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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

        public TtSettings Settings { get; }

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

        public bool IsGenerating { get { return Get<bool>(); } set { Set(value); } }


        public CreatePlotsModel(TtProject project, Window window)
        {
            _Manager = project.Manager;
            Settings = project.Settings;

            Polygons = new ObservableCollection<TtPolygon>(
                _Manager.GetPolygons().Where(p => _Manager.GetPoints(p.CN).HasAtLeast(2, pt => pt.IsBndPoint())));

            UomDistance = Distance.FeetTenths;

            if (Polygons.Count > 0)
                SelectedPolygon = Polygons[0];

            GridX = GridY = 100;

            SamplePoints = false;
            SampleTypeItem = SampleType.Percent;

            BoundaryBuffer = false;
            BufferAmount = 10;

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

            if (Tilt < -45 || Tilt > 45)
            {
                MessageBox.Show("Y Axis Tilt must be between -45° and +45°");
                return;
            }

            if (SamplePoints == true &&
                (SampleTypeItem == SampleType.Percent && SampleAmount < 1 || SampleAmount > 100) ||
                (SampleTypeItem == SampleType.Points && SampleAmount < 1))
            {
                MessageBox.Show($"The Sample Amount must be {(SampleTypeItem == SampleType.Percent ? "between 1% and 100%." : "greater than 1 point.")}");
                return;
            }

            if (BoundaryBuffer == true && BufferAmount < 1)
            {
                MessageBox.Show("The Boundary Buffer must be greater than 0.");
                return;
            }

            List<TtPolygon> polygons = _Manager.GetPolygons();
            string gPolyName = $"{SelectedPolygon.Name}_Plts";

            TtPolygon poly = null;

            try
            {
                poly = polygons.First(p => p.Name == gPolyName);
            }
            catch
            {
                //
            }


            if (poly != null)
            {
                if (MessageBox.Show($"Plots '{gPolyName}' already exist. Would you like to overwrite the plots?", "Plots Already Exist", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
                    return;

                _Manager.DeletePointsInPolygon(poly.CN);
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

            IsGenerating = true;
            GeneratePoints(poly);
        }

        private void GeneratePoints(TtPolygon poly)
        {
            double gridX = FMSC.Core.Convert.Distance(GridX, Distance.Meters, UomDistance);
            double gridY = FMSC.Core.Convert.Distance(GridY, Distance.Meters, UomDistance);

            double angle = Tilt * -1;

            poly.Description = $"Angle: {Tilt}°, Grid({UomDistance.ToStringAbv()}) X:{GridX} Y:{GridY}, Created from Polygon: {SelectedPolygon.Name}";

            IEnumerable<TtPoint> points = _Manager.GetPoints(SelectedPolygon.CN).Where(p => p.IsBndPoint());

            TtMetadata defMeta = points.First().Metadata;

            IEnumerable<UTMCoords> coords = points.Select(p => TtUtils.GetCoords(p, defMeta.Zone));
            List<Point> utmPoints = points
                .Select(p =>
                {
                    UTMCoords c = TtUtils.GetCoords(p, defMeta.Zone);
                    return new Point(c.X, c.Y);
                }).ToList();

            if (utmPoints[0] != utmPoints[utmPoints.Count - 1])
                utmPoints.Add(utmPoints[0]);

            UtmExtent.Builder builder = new UtmExtent.Builder(defMeta.Zone);
            builder.Include(utmPoints);
            UtmExtent extent = builder.Build();

            Random rand = new Random();
            UTMCoords startCoords = SelectedPoint != null ?
                TtUtils.GetCoords(SelectedPoint, defMeta.Zone) :
                new UTMCoords(
                    (rand.NextDouble() * (extent.East - extent.West) + extent.West),
                    (rand.NextDouble() * (extent.North - extent.South) + extent.South),
                    defMeta.Zone
                );
            
            PolygonCalculator calc = new PolygonCalculator(utmPoints);
            PolygonCalculator.Boundaries bnds = calc.PointBoundaries;

            Point farCorner = TtUtils.GetFarthestCorner(
                startCoords.X, startCoords.Y,
                bnds.TopLeft.Y, bnds.BottomRight.Y,
                bnds.TopLeft.X, bnds.BottomRight.X);

            double dist = MathEx.Distance(startCoords.X, startCoords.Y, farCorner.X, farCorner.Y);

            int ptAmtY = (int)(Math.Floor(dist / gridY) + 1);
            int ptAmtX = (int)(Math.Floor(dist / gridX) + 1);

            double farLeft, farRight, farTop, farBottom;

            PolygonCalculator inBnds = new PolygonCalculator(new Point[]
            {
                bnds.TopLeft,
                new Point(bnds.BottomRight.X, bnds.TopLeft.Y),
                bnds.BottomRight,
                new Point(bnds.TopLeft.X, bnds.BottomRight.Y)
            });

            farLeft = startCoords.X - (ptAmtX * gridX);
            farRight = startCoords.X + (ptAmtX * gridX);
            farTop = startCoords.Y + (ptAmtY * gridY);
            farBottom = startCoords.Y - (ptAmtY * gridY);
            
            int i = 0;
            double j = farLeft;
            double k = farTop;

            List<Point> addPoints = new List<Point>();
            Point tmp;

            while (j <= farRight)
            {
                while (k >= farBottom)
                {
                    tmp = angle != 0 ? MathEx.RotatePoint(j, k, angle, startCoords.X, startCoords.Y) : new Point(j, k);
                    
                    if (calc.IsPointInPolygon(tmp.X, tmp.Y))
                        addPoints.Add(tmp);

                    k -= gridY;
                }
                j += gridX;
                k = farTop;
            }

            if (BoundaryBuffer == true)
            {
                double ba = FMSC.Core.Convert.Distance(BufferAmount, Distance.Meters, UomDistance);
                
                for (i = addPoints.Count - 1; i > -1; i--)
                {
                    Point p = addPoints[i];
                    for (int m = 0; m < utmPoints.Count - 1; m++)
                    {
                        if (MathEx.LineToPointDistance2D(utmPoints[m], utmPoints[m + 1], p) < ba)
                        {
                            addPoints.RemoveAt(i);
                            break;
                        }
                    }
                    
                }
            }

            if (SamplePoints == true)
            {
                int maxPoints = SampleTypeItem == SampleType.Percent ?
                    (int)((SampleAmount / 100.0) * addPoints.Count) : SampleAmount;

                while (maxPoints < addPoints.Count)
                {
                    addPoints.RemoveAt(rand.Next(addPoints.Count - 1));
                }
            }


            List<TtPoint> wayPoints = new List<TtPoint>();
            i = 0;
            WayPoint curr, prev = null;

            foreach (Point p in addPoints)
            {
                curr = new WayPoint()
                {
                    UnAdjX = p.X,
                    UnAdjY = p.Y,
                    Polygon = poly,
                    Group = _Manager.MainGroup,
                    Metadata = defMeta,
                    Index = i,
                    Comment = "Generated Point",
                    PID = PointNamer.NamePoint(poly, prev)
                };

                wayPoints.Add(curr);
                prev = curr;
                j++;
            }

            _Manager.AddPoints(wayPoints);

            MessageBox.Show($"{addPoints.Count} WayPoints Created");
            IsGenerating = false;
        }
    }
}
