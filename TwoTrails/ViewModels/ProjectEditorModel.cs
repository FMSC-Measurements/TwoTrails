using CSUtil.ComponentModel;
using FMSC.Core;
using FMSC.Core.ComponentModel.Commands;
using FMSC.GeoSpatial.MTDC;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TwoTrails.Controls;
using TwoTrails.Core;
using TwoTrails.Core.Points;
using TwoTrails.Dialogs;

namespace TwoTrails.ViewModels
{
    public class ProjectEditorModel : NotifyPropertyChangedEx
    {
        private TtProject _Project;
        
        public TtProjectInfo ProjectInfo { get { return _Project.ProjectInfo; } }
        public TtManager Manager { get { return _Project.Manager; } }


        public DataEditorControl DataController { get; }


        public double TotalPolygonArea { get { return Get<double>(); } set { Set(value); } }
        public double TotalPolygonPerimeter { get { return Get<double>(); } set { Set(value); } }

        public double PolygonAccuracy { get { return Get<double>(); } set { Set(value); } }

        private TtPolygon _BackupPoly;
        private TtPolygon _CurrentPolygon;
        public TtPolygon CurrentPolygon
        {
            get { return _CurrentPolygon; }
            private set
            {
                TtPolygon old = _CurrentPolygon;

                SetField(ref _CurrentPolygon, value, () => {
                    if (old != null)
                    {
                        old.PropertyChanged -= Polygon_PropertyChanged;
                        old.PolygonChanged -= GeneratePolygonSummary;
                    }

                    if (value != null)
                    {
                        _BackupPoly = new TtPolygon(value);
                        _CurrentPolygon.PropertyChanged += Polygon_PropertyChanged;
                        _CurrentPolygon.PolygonChanged += GeneratePolygonSummary;

                        GeneratePolygonSummary(value);
                    }
                });

            }
        }

        private void GeneratePolygonSummary(TtPolygon polygon)
        {
            PolygonSummary = HaidLogic.GenerateSummary(Manager, polygon, true);
        }

        public PolygonSummary PolygonSummary { get { return Get<PolygonSummary>(); } set { Set(value); } }


        private void Polygon_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!(sender as TtPolygon).Equals(_BackupPoly))
            {
                _Project.ProjectUpdated();
            }
        }

        public ICommand PolygonChangedCommand { get; }
        public ICommand NewPolygonCommand { get; }
        public ICommand DeletePolygonCommand { get; }
        public ICommand PolygonUpdateAccCommand { get; }
        public ICommand PolygonMtdcLookupCommand { get; }
        public ICommand PolygonAccuracyChangedCommand { get; }
        public ICommand SavePolygonSummary { get; }
        

        public int MetadataZone { get { return Get<int>(); } set { Set(value); } }

        private TtMetadata _BackupMeta;
        private TtMetadata _CurrentMetadata;
        public TtMetadata CurrentMetadata
        {
            get { return _CurrentMetadata; }
            private set
            {
                TtMetadata old = _CurrentMetadata;

                SetField(ref _CurrentMetadata, value, () => {
                    if (old != null)
                    {
                        old.PropertyChanged -= Metadata_PropertyChanged;
                    }

                    _BackupMeta = new TtMetadata(value);

                    _CurrentMetadata.PropertyChanged += Metadata_PropertyChanged;
                });

            }
        }

        private void Metadata_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!(sender as TtMetadata).Equals(_BackupMeta))
            {
                _Project.ProjectUpdated();
            }
        }

        public ICommand MetadataChangedCommand { get; }
        public ICommand NewMetadataCommand { get; }
        public ICommand DeleteMetadataCommand { get; } 
        public ICommand MetadataZoneChangedCommand { get; }
        public ICommand MetadataUpdateZoneCommand { get; }
        public ICommand SetDefaultMetadataCommand { get; }


        private TtGroup _BackupGroup;
        private TtGroup _CurrentGroup;
        public TtGroup CurrentGroup
        {
            get { return _CurrentGroup; }
            private set
            {
                TtGroup old = _CurrentGroup;

                SetField(ref _CurrentGroup, value, () => {
                    if (old != null)
                    {
                        old.PropertyChanged -= Group_PropertyChanged;
                    }

                    _BackupGroup = new TtGroup(value);

                    _CurrentGroup.PropertyChanged += Group_PropertyChanged;
                });

            }
        }

        private void Group_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!(sender as TtGroup).Equals(_BackupGroup))
            {
                _Project.ProjectUpdated();
            }
        }

        public ICommand GroupChangedCommand { get; }
        public ICommand NewGroupCommand { get; }
        public ICommand DeleteGroupCommand { get; }


        public ReadOnlyObservableCollection<TtPolygon> Polygons { get { return _Project.Manager.Polygons; } }
        public ReadOnlyObservableCollection<TtMetadata> Metadata { get { return _Project.Manager.Metadata; } }
        public ReadOnlyObservableCollection<TtGroup> Groups { get { return _Project.Manager.Groups; } }


        public ProjectEditorModel(TtProject project)
        {
            _Project = project;

            DataController = new DataEditorControl(project.DataEditor, new DataStyleModel(project));

            PolygonChangedCommand = new RelayCommand(x => PolygonChanged(x as TtPolygon));
            NewPolygonCommand = new RelayCommand(x => NewPolygon(x as ListBox));
            DeletePolygonCommand = new RelayCommand(x => DeletePolygon());

            PolygonAccuracyChangedCommand = new RelayCommand(x => PolygonAccuracyChanged(x as string));

            PolygonUpdateAccCommand = new BindedRelayCommand<ProjectEditorModel>(
                x => UpdatePolygonAcc(),
                x => CurrentPolygon != null && CurrentPolygon.Accuracy != PolygonAccuracy,
                this,
                x => new { x.PolygonAccuracy, CurrentPolygon.Accuracy });

            PolygonMtdcLookupCommand = new BindedRelayCommand<ProjectEditorModel>(
                x => MtdcLookup(),
                x => CurrentPolygon != null,
                this,
                x => x.CurrentPolygon);

            SavePolygonSummary = new BindedRelayCommand<ProjectEditorModel>(
                x => SavePolygonsummary(),
                x => CurrentPolygon != null, this, x => x.CurrentPolygon);

            MetadataChangedCommand = new RelayCommand(x => MetadataChanged(x as TtMetadata));
            NewMetadataCommand = new RelayCommand(x => NewMetadata(x as ListBox));
            SetDefaultMetadataCommand = new RelayCommand(x => SetMetadataDefault());
            DeleteMetadataCommand = new BindedRelayCommand<ProjectEditorModel>(
                x => DeleteMetadata(),
                x => x != null && (x as TtMetadata).CN != Consts.EmptyGuid,
                this,
                x => x.CurrentMetadata);

            MetadataZoneChangedCommand = new RelayCommand(x => MetadataZoneChanged(x as string));

            MetadataUpdateZoneCommand = new BindedRelayCommand<ProjectEditorModel>(
                x => UpdateMetadataZone(),
                x => CurrentMetadata != null && CurrentMetadata.Zone != MetadataZone,
                this,
                x => x.MetadataZone);


            GroupChangedCommand = new RelayCommand(x => GroupChanged(x as TtGroup));
            NewGroupCommand = new RelayCommand(x => NewGroup(x as ListBox));
            DeleteGroupCommand = new BindedRelayCommand<ProjectEditorModel>(
                x => DeleteGroup(),
                x => x != null && (x as TtGroup).CN != Consts.EmptyGuid,
                this,
                x => x.CurrentGroup);

            PolygonShapeChanged(null);

            foreach (TtPolygon poly in Manager.Polygons)
                poly.PolygonChanged += PolygonShapeChanged;

            ((INotifyCollectionChanged)Manager.Polygons).CollectionChanged += PolygonCollectionChanged;
        }

        private void PolygonCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (TtPolygon poly in e.NewItems)
                        poly.PolygonChanged += PolygonShapeChanged;
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (TtPolygon poly in e.OldItems)
                        poly.PolygonChanged -= PolygonShapeChanged;
                    break;
                case NotifyCollectionChangedAction.Replace:
                case NotifyCollectionChangedAction.Reset:
                    foreach (TtPolygon poly in e.NewItems)
                        poly.PolygonChanged += PolygonShapeChanged;
                    foreach (TtPolygon poly in e.OldItems)
                        poly.PolygonChanged -= PolygonShapeChanged;
                    break;
            }
        }

        private void PolygonShapeChanged(TtPolygon polygon)
        {
            double area = 0, perim = 0;
            foreach (TtPolygon poly in Manager.Polygons)
            {
                area += poly.Area;
                perim += poly.Perimeter;
            }

            TotalPolygonArea = area;
            TotalPolygonPerimeter = perim;
        }

        private void PolygonChanged(TtPolygon poly)
        {
            CurrentPolygon = poly;
            PolygonAccuracy = poly != null ? poly.Accuracy : 6d;
        }

        private bool PolygonAccuracyChanged(string accStr)
        {
            if (accStr != null)
            {
                double acc;

                if (double.TryParse(accStr, out acc))
                {
                    PolygonAccuracy = acc;
                    return true;
                }
            }

            return false;
        }

        private void UpdatePolygonAcc()
        {
            CurrentPolygon.Accuracy = PolygonAccuracy;
            OnPropertyChanged(nameof(PolygonAccuracy));
        }

        private void MtdcLookup()
        {
            GpsReportStatus status = SessionData.HasGpsAccReport();
            if (status != GpsReportStatus.CantGetReport)
            {
                MtdcDataDialog dialog = new MtdcDataDialog(SessionData.GpsAccuracyReport, true, true,
                    SessionData.MakeID, SessionData.ModelID);
                dialog.Owner = _Project.MainModel.MainWindow;

                if (dialog.ShowDialog() == true)
                {
                    CurrentPolygon.Accuracy = dialog.Accuracy;
                    PolygonAccuracy = dialog.Accuracy;
                    OnPropertyChanged(nameof(PolygonAccuracy));
                }
            }
            else
            {
                MessageBox.Show("Unable to retrieve MTDC data.", "", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void NewPolygon(ListBox listBox)
        {
            _Project.Manager.AddPolygon(_Project.Manager.CreatePolygon());
            listBox.SelectedIndex = listBox.Items.Count - 1;

            _Project.ProjectUpdated();
        }

        private void DeletePolygon()
        {
            if (CurrentPolygon != null)
            {
                if (MessageBox.Show(String.Format("Confirm Delete Polygon '{0}'", CurrentPolygon.Name), "Delete Polygon",
                    MessageBoxButton.OKCancel, MessageBoxImage.Warning) == MessageBoxResult.OK)
                {
                    _Project.Manager.DeletePolygon(CurrentPolygon);

                    _Project.ProjectUpdated();
                }
            }
        }
        
        private void SavePolygonsummary()
        {
            if (CurrentPolygon != null)
            {
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.DefaultExt = "(Text File *.txt)|*.txt";
                sfd.FileName = String.Format("{0}_Summary", CurrentPolygon.Name);
                sfd.DefaultExt = ".txt";
                sfd.OverwritePrompt = true;

                if (sfd.ShowDialog() == true)
                {
                    using (StreamWriter sw = new StreamWriter(sfd.FileName))
                    {
                        sw.WriteLine(PolygonSummary.SummaryText);
                    }
                }
            }
        }


        private void MetadataChanged(TtMetadata meta)
        {
            MetadataZone = meta != null ? meta.Zone : 13;
            CurrentMetadata = meta;
        }

        private bool MetadataZoneChanged(string zoneStr)
        {
            if (zoneStr != null)
            {
                int zone;

                if (int.TryParse(zoneStr, out zone))
                {
                    MetadataZone = zone;
                    return true;
                }
            }

            return false;
        }

        private void UpdateMetadataZone()
        {
            if (MessageBox.Show("Are you sure you want to change the metadata's zone?", "Confirm zone change.", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                CurrentMetadata.Zone = MetadataZone;
                OnPropertyChanged(nameof(MetadataZone));
            }
        }
        
        private void SetMetadataDefault()
        {
            _Project.Settings.MetadataSettings.SetSettings(CurrentMetadata);
            MessageBox.Show("Metadata defaults set.");
        }

        private void NewMetadata(ListBox listBox)
        {
            _Project.Manager.AddMetadata(_Project.Manager.CreateMetadata());
            listBox.SelectedIndex = listBox.Items.Count - 1;

            _Project.ProjectUpdated();
        }

        private void DeleteMetadata()
        {
            if (CurrentMetadata != null && CurrentMetadata.CN != Consts.EmptyGuid)
            {
                if (MessageBox.Show(String.Format("Confirm Delete Metadata '{0}'", CurrentMetadata.Name), "Delete Metadata",
                MessageBoxButton.OKCancel, MessageBoxImage.Warning) == MessageBoxResult.OK)
                {
                    ITtManager manager = _Project.Manager;

                    IEnumerable<TtPoint> points = manager.GetPoints().Where(p => p.MetadataCN == CurrentMetadata.CN);

                    if (CurrentMetadata.Zone != manager.DefaultMetadata.Zone && points.Any())
                    {
                        if (MessageBox.Show(
                            String.Format("Metdata '{0}' does not have the same zone as the Default Metadata. All the points that use {0} will have their zone converted.", CurrentMetadata),
                            "Convert Zones", MessageBoxButton.OKCancel, MessageBoxImage.Warning) != MessageBoxResult.OK)
                            return;
                    }

                    manager.DeleteMetadata(CurrentMetadata);

                    _Project.ProjectUpdated();
                }
            }
        }


        //TODO add extra metadata info


        private void GroupChanged(TtGroup group)
        {
            CurrentGroup = group;
        }

        private void NewGroup(ListBox listBox)
        {
            _Project.Manager.AddGroup(_Project.Manager.CreateGroup());
            listBox.SelectedIndex = listBox.Items.Count - 1;

            _Project.ProjectUpdated();
        }

        private void DeleteGroup()
        {
            if (CurrentGroup != null && CurrentGroup.CN != Consts.EmptyGuid)
            {
                if (MessageBox.Show(String.Format("Confirm Delete Group '{0}'", CurrentGroup.Name), "Delete Group",
                    MessageBoxButton.OKCancel, MessageBoxImage.Warning) == MessageBoxResult.OK)
                {
                    _Project.Manager.DeleteGroup(CurrentGroup);

                    _Project.ProjectUpdated();
                }
            }
        }
    }
}
