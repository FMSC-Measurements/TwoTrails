using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TwoTrails.Core.Points;

namespace TwoTrails.Mapping
{
    /// <summary>
    /// Interaction logic for PointInfoBox.xaml
    /// </summary>
    public partial class PointInfoBox : UserControl
    {
        public TtMapPoint Point { get; }

        public String Header { get { return GetHeader(Point.Point); } }


        public Visibility HasComment { get { return String.IsNullOrWhiteSpace(Point.Point.Comment) ? Visibility.Collapsed : Visibility.Visible; } }


        public Double Latitude { get { return Adjusted ? Point.AdjLocation.Latitude : Point.UnAdjLocation.Latitude; } }
        public Double Longitude { get { return Adjusted ? Point.AdjLocation.Latitude : Point.UnAdjLocation.Longitude; } }

        
        public Double UtmX { get { return Adjusted ? Point.Point.AdjX : Point.Point.UnAdjX; } }
        public Double UtmY { get { return Adjusted ? Point.Point.AdjY : Point.Point.UnAdjY; } }
        public Double Elevation { get { return Adjusted ? Point.Point.AdjZ : Point.Point.UnAdjZ; } }


        public Visibility IsTravType { get { return Point.Point.IsTravType() ? Visibility.Visible : Visibility.Collapsed; } }

        public Double? FwdAz { get { return Point.Point.IsTravType() ? ((TravPoint)Point.Point).FwdAzimuth : null; } }
        public Double? BackAz { get { return Point.Point.IsTravType() ? ((TravPoint)Point.Point).FwdAzimuth : null; ; } }
        public Double SlpDist { get { return Point.Point.IsTravType() ? ((TravPoint)Point.Point).SlopeDistance : 0; ; } }
        public Double SlpAng { get { return Point.Point.IsTravType() ? ((TravPoint)Point.Point).SlopeAngle : 0; ; } }


        public Visibility IsGpsType { get { return Point.Point.IsGpsType() ? Visibility.Visible : Visibility.Collapsed; } }

        public Double? RMSEr { get { return Point.Point.IsGpsType() ? ((GpsPoint)Point.Point).RMSEr : null; } }



        public bool Adjusted { get; }
        public string IsAdjusted { get { return Adjusted ? "(Adj)" : "(UnAdj)"; } }


        public PointInfoBox(TtMapPoint point, bool adjusted)
        {
            this.Point = point;
            this.Adjusted = adjusted;

            this.DataContext = this;
            InitializeComponent();
        }


        public string GetHeader(TtPoint point)
        {
            if (point.OpType == Core.OpType.Quondam)
            {
                TtPoint parent = ((QuondamPoint)point).ParentPoint;

                return String.Format("{0} ({1}) \u2794 {2} ({3})    ",
                        point.PID,
                        point.OpType,
                        parent.PID,
                        parent.OpType);
            }
            else
            {
                return String.Format("{0} ({1})",
                        point.PID,
                        point.OpType);
            }
        }
    }
}
