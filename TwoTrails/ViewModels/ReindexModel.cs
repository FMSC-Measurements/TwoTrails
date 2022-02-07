using FMSC.Core;
using FMSC.Core.ComponentModel;
using FMSC.Core.Windows.ComponentModel.Commands;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using TwoTrails.Core;
using TwoTrails.Core.Points;

namespace TwoTrails.ViewModels
{
    public class ReindexModel : BaseModel
    {
        private TtHistoryManager _Manager;
        public ReadOnlyObservableCollection<TtPolygon> Polygons => _Manager.Polygons;
        public TtPolygon SelectedPolygon { get { return Get<TtPolygon>(); } set { Set(value); } }
        public ReindexMode ReindexMode { get { return Get<ReindexMode>(); } set { Set(value); } }

        public ICommand ReindexCommand { get; }


        public ReindexModel(TtHistoryManager manager)
        {
            _Manager = manager;
            ReindexCommand = new BindedRelayCommand<ReindexModel>(
                x=> Reindex(), x=> SelectedPolygon != null,
                this, m => new { m.SelectedPolygon, m.ReindexMode });
        }


        private void Reindex()
        {
            if (SelectedPolygon != null)
            {
                List<TtPoint> points = _Manager.GetPoints(SelectedPolygon.CN);

                if (points.Count > 0)
                {
                    if (ReindexMode == ReindexMode.PID)
                    {
                        _Manager.EditPointsMultiValues(points.OrderBy(p => p.PID), PointProperties.INDEX, Enumerable.Range(0, points.Count));
                    }
                    else if (ReindexMode == ReindexMode.CreationTime)
                    {
                        _Manager.EditPointsMultiValues(points.OrderBy(p => p.TimeCreated), PointProperties.INDEX, Enumerable.Range(0, points.Count));
                    }
                    else if (ReindexMode == ReindexMode.CurrentOrder)
                    {
                        _Manager.EditPointsMultiValues(points, PointProperties.INDEX, Enumerable.Range(0, points.Count));
                    }

                    MessageBox.Show($"Polygon '{SelectedPolygon.Name}' reindexed by {ReindexMode.GetDescription()}.", "Reindex Polyon", MessageBoxButton.OK);
                }
                else
                {
                    MessageBox.Show("Polygon does not contain any points.");
                }
            }
            else
            {
                MessageBox.Show("No Polygon selected.");
            }
        }
    }

    public enum ReindexMode
    {
        [Description("PID (Point ID)")]
        PID = 1,
        [Description("Creation Time")]
        CreationTime = 0,
        [Description("Current Order")]
        CurrentOrder = 2
    }
}
