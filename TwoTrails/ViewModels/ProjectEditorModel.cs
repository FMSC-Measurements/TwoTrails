using CSUtil.ComponentModel;
using FMSC.Core.ComponentModel.Commands;
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

namespace TwoTrails.ViewModels
{
    public class ProjectEditorModel : NotifyPropertyChangedEx
    {
        private TtProject _Project;


        public string PolygonName { get { return Get<string>(); } set { Set(value); OnPropertyChanged(nameof(RequirePolySave)); } }
        public string PolygonDesc { get { return Get<string>(); } set { Set(value); OnPropertyChanged(nameof(RequirePolySave)); } }
        public string PolygonPointStartIndex { get { return Get<string>(); } set { Set(value); OnPropertyChanged(nameof(RequirePolySave)); } }
        public string PolygonIncrement { get { return Get<string>(); } set { Set(value); OnPropertyChanged(nameof(RequirePolySave)); } }
        public string PolygonAccuracy { get { return Get<string>(); } set { Set(value); OnPropertyChanged(nameof(RequirePolySave)); } }

        public TtPolygon CurrentPolygon
        {
            get { return Get<TtPolygon>(); }

            private set
            {
                Set(value);
                ResetPoly();
            }
        }

        public ICommand PolygonChangedCommand { get; }
        public ICommand SavePolygonCommand { get; }
        public ICommand ResetPolygonCommand { get; }
        public ICommand DeletePolygonCommand { get; }

        public ICommand MetadataChangedCommand { get; }
        public ICommand SaveMetadataCommand { get; }
        public ICommand ResetMetadataCommand { get; }
        public ICommand DeleteMetadataCommand { get; }

        public ICommand GroupChangedCommand { get; }
        public ICommand SaveGroupCommand { get; }
        public ICommand ResetGroupCommand { get; }
        public ICommand DeleteGroupCommand { get; }


        public ReadOnlyObservableCollection<TtPolygon> Polygons { get { return _Project.Manager.Polygons; } }
        public ReadOnlyObservableCollection<TtMetadata> Metadata { get { return _Project.Manager.Metadata; } }
        public ReadOnlyObservableCollection<TtGroup> Groups { get { return _Project.Manager.Groups; } }


        public bool RequirePolySave
        {
            get
            {
                return CurrentPolygon != null && (PolygonName != CurrentPolygon.Name ||
                    PolygonDesc != CurrentPolygon.Description ||
                    PolygonPointStartIndex != CurrentPolygon.PointStartIndex.ToString() ||
                    PolygonIncrement != CurrentPolygon.Increment.ToString() ||
                    PolygonAccuracy != CurrentPolygon.Accuracy.ToString());
            }
        }

        public ProjectEditorModel(TtProject project)
        {
            this._Project = project;

            CurrentPolygon = null;

            PolygonChangedCommand = new RelayCommand(x => PolygonChanged(x as TtPolygon));
            DeletePolygonCommand = new RelayCommand(x => DeletePolygon());

            SavePolygonCommand = new BindedRelayCommand<ProjectEditorModel>(
                x => SavePolygon(),
                x => RequirePolySave,
                this,
                x => x.RequirePolySave);
            
            ResetPolygonCommand = new BindedRelayCommand<ProjectEditorModel>(
                x => ResetPoly(),
                x => RequirePolySave,
                this,
                x => x.RequirePolySave);

        }



        private void ResetPoly()
        {
            if (CurrentPolygon != null)
            {
                PolygonName = CurrentPolygon.Name;
                PolygonDesc = CurrentPolygon.Description;
                PolygonPointStartIndex = CurrentPolygon.PointStartIndex.ToString();
                PolygonIncrement = CurrentPolygon.Increment.ToString();
                PolygonAccuracy = CurrentPolygon.Accuracy.ToString();
            }
            else
            {
                PolygonName = PolygonDesc =
                PolygonPointStartIndex = PolygonIncrement =
                PolygonAccuracy = String.Empty;
            }
        }

        private void PolygonChanged(TtPolygon poly)
        {
            CurrentPolygon = poly;
        }

        private void SavePolygon()
        {
            if (CurrentPolygon != null)
            {
                if (!string.IsNullOrWhiteSpace(PolygonName))
                    CurrentPolygon.Name = PolygonName;

                CurrentPolygon.Description = PolygonDesc ?? String.Empty;

                double acc = 6;
                if (double.TryParse(PolygonAccuracy, out acc))
                    CurrentPolygon.Accuracy = acc;

                int tmp = CurrentPolygon.PointStartIndex;
                if (int.TryParse(PolygonPointStartIndex, out tmp))
                    CurrentPolygon.PointStartIndex = tmp;

                tmp = CurrentPolygon.Increment;
                if (int.TryParse(PolygonIncrement, out tmp))
                    CurrentPolygon.Increment = tmp;

                _Project.ProjectUpdated();

                OnPropertyChanged(nameof(RequirePolySave));
            }
        }

        private void DeletePolygon()
        {
            if (CurrentPolygon != null)
            {
                TtPolygon poly = CurrentPolygon;
                ITtManager manager = _Project.Manager;

                if (MessageBox.Show(String.Format("Confirm Delete Polygon '{0}'", poly.Name)) == MessageBoxResult.OK)
                {
                    List<TtPoint> points = manager.GetPoints(poly.CN);

                    if (points.Count > 0)
                    {
                        foreach (TtPoint point in points)
                        {
                            if (point.OpType == OpType.Quondam)
                            {
                                ((QuondamPoint)point).ParentPoint = null;
                            }
                            else if (point.HasQuondamLinks)
                            {
                                foreach (string cn in point.LinkedPoints.ToArray())
                                {
                                    QuondamPoint qp = manager.GetPoint(cn) as QuondamPoint;
                                    
                                    if (qp != null)
                                    {
                                        if (qp.ParentPoint.PolygonCN != poly.CN)
                                        {
                                            GpsPoint gps = new GpsPoint(qp);

                                            if (!string.IsNullOrEmpty(gps.Comment) && string.IsNullOrEmpty(point.Comment))
                                                gps.Comment = point.Comment;

                                            if (qp.ManualAccuracy != null)
                                                gps.ManualAccuracy = qp.ManualAccuracy;

                                            qp.ParentPoint = null;

                                            manager.ReplacePoint(gps);
                                        }
                                        else
                                            qp.ParentPoint = null;
                                    }
                                    //else
                                    //{
                                    //    detached quondam?
                                    //}
                                }
                            }
                        }
                    }

                    manager.DeletePolygon(poly);

                    _Project.ProjectUpdated();
                }
            }
        }
    }
}
