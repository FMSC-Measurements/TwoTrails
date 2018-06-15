using CSUtil.ComponentModel;
using FMSC.Core.ComponentModel.Commands;
using Microsoft.Win32;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TwoTrails.Core.Points;
using TwoTrails.Utils;

namespace TwoTrails.Controls
{
    /// <summary>
    /// Interaction logic for PointDetailsControl.xaml
    /// </summary>
    public partial class PointDetailsControl : UserControl
    {
        public ICommand CopyCellValueCommand { get; }
        public ICommand ExportValuesCommand { get; }

        public PointDetailsControl()
        {
            CopyCellValueCommand = new RelayCommand(x => CopyCellValue(dgPoints));
            ExportValuesCommand = new RelayCommand(x => ExportValues(dgPoints));

            InitializeComponent();
        }

        public static readonly DependencyProperty SelectedItemsListProperty =
                DependencyProperty.Register("Points", typeof(IList<TtPoint>), typeof(PointDetailsControl), new PropertyMetadata(null));
        
        public IList<TtPoint> Points
        {
            get { return (IList<TtPoint>)GetValue(SelectedItemsListProperty); }
            set { SetValue(SelectedItemsListProperty, value); }
        }

        public static readonly DependencyProperty FieldControllerProperty =
                DependencyProperty.Register("FieldController", typeof(PointFieldController), typeof(PointDetailsControl), new PropertyMetadata(null));

        public PointFieldController FieldController
        {
            get { return (PointFieldController)GetValue(FieldControllerProperty); }
            set { SetValue(FieldControllerProperty, value); }
        }


        private void ExportValues(DataGrid grid)
        {
            if (grid.SelectedItems.Count > 0)
            {
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.FileName = "SelectedPoints";
                sfd.DefaultExt = ".csv";
                sfd.Filter = "CSV Document (*.csv)|*.csv|All Types (*.*)|*.*";

                if (sfd.ShowDialog() == true)
                {
                    Export.CheckCreateFolder(Path.GetDirectoryName(sfd.FileName));
                    Export.Points(grid.SelectedItems.Cast<TtPoint>().ToList(), sfd.FileName);
                }
            }
        }

        private void CopyCellValue(DataGrid grid)
        {
            if (grid != null)
            {
                var cellInfo = grid.SelectedCells[grid.CurrentCell.Column.DisplayIndex];
                
                if (cellInfo.Column.GetCellContent(cellInfo.Item) is TextBlock tb)
                    Clipboard.SetText(tb.Text);
            }
        }

        public class PointFieldController : NotifyPropertyChangedEx
        {
            public bool Index { get { return Get<bool>(); } set { Set(value); } }
            public bool PID { get { return Get<bool>(); } set { Set(value); } }
            public bool OpType { get { return Get<bool>(); } set { Set(value); } }
            public bool OnBoundary { get { return Get<bool>(); } set { Set(value); } }
            public bool TimeCreated { get { return Get<bool>(); } set { Set(value); } }

            public bool Polygon { get { return Get<bool>(); } set { Set(value); } }
            public bool Metadata { get { return Get<bool>(); } set { Set(value); } }
            public bool Group { get { return Get<bool>(); } set { Set(value); } }

            public bool AdjX { get { return Get<bool>(); } set { Set(value); } }
            public bool AdjY { get { return Get<bool>(); } set { Set(value); } }
            public bool AdjZ { get { return Get<bool>(); } set { Set(value); } }
            public bool UnAdjX { get { return Get<bool>(); } set { Set(value); } }
            public bool UnAdjY { get { return Get<bool>(); } set { Set(value); } }
            public bool UnAdjZ { get { return Get<bool>(); } set { Set(value); } }

            public bool Accuracy { get { return Get<bool>(); } set { Set(value); } }

            public bool Latitude { get { return Get<bool>(); } set { Set(value); } }
            public bool Longitude { get { return Get<bool>(); } set { Set(value); } }
            public bool Elevation { get { return Get<bool>(); } set { Set(value); } }
            public bool RMSEr { get { return Get<bool>(); } set { Set(value); } }

            public bool FwdAzimuth { get { return Get<bool>(); } set { Set(value); } }
            public bool BkAzimuth { get { return Get<bool>(); } set { Set(value); } }
            public bool HorizontalDistance { get { return Get<bool>(); } set { Set(value); } }
            public bool SlopeDistance { get { return Get<bool>(); } set { Set(value); } }
            public bool SlopeAngle { get { return Get<bool>(); } set { Set(value); } }

            public bool ParentPoint { get { return Get<bool>(); } set { Set(value); } }

            public bool HasQuondamLinks { get { return Get<bool>(); } set { Set(value); } }
            public bool Comment { get { return Get<bool>(); } set { Set(value); } }

            public bool CN { get { return Get<bool>(); } set { Set(value); } }
            public bool PolygonCN { get { return Get<bool>(); } set { Set(value); } }
            public bool MetadataCN { get { return Get<bool>(); } set { Set(value); } }
            public bool GroupCN { get { return Get<bool>(); } set { Set(value); } }
            public bool ParentCN { get { return Get<bool>(); } set { Set(value); } }

            public PointFieldController()
            {
                Index = true;
                PID = true;
                OpType = true;
                OnBoundary = true;
                TimeCreated = true;
                Polygon = true;
                Metadata = true;
                Group = true;
                AdjX = true;
                AdjY = true;
                AdjZ = true;
                UnAdjX = true;
                UnAdjY = true;
                UnAdjZ = true;
                Accuracy = true;
                Latitude = true;
                Longitude = true;
                Elevation = true;
                RMSEr = true;
                FwdAzimuth = true;
                BkAzimuth = true;
                HorizontalDistance = true;
                SlopeDistance = true;
                SlopeAngle = true;
                ParentPoint = true;
                HasQuondamLinks = true;
                Comment = true;
                CN = true;
                PolygonCN = true;
                MetadataCN = true;
                GroupCN = true;
                ParentCN = true;
            }
        }
    }
}
