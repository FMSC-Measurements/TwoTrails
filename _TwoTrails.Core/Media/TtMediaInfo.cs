using FMSC.Core.ComponentModel;
using System;
using System.Collections.ObjectModel;
using TwoTrails.Core.Points;

namespace TwoTrails.Core.Media
{
    public class TtMediaInfo : BaseModel
    {
        private TtPoint _Point;
        public String Title { get { return ToString(); } }

        private ObservableCollection<TtImage> _Images;
        public ReadOnlyObservableCollection<TtImage> Images { get; }

        public int TotalMediaCount { get { return _Images.Count; } }
        public int ImageCount { get { return _Images.Count; } }

        

        public TtMediaInfo(TtPoint point)
        {
            _Point = point;
            _Point.PropertyChanged += Point_PropertyChanged;

            _Images = new ObservableCollection<TtImage>();
            Images = new ReadOnlyObservableCollection<TtImage>(_Images);
        }

        private void Point_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(TtPoint.PID) || e.PropertyName == nameof(TtPoint.Polygon))
                OnPropertyChanged(nameof(Title));
        }

        private void UpdateCounts()
        {
            OnPropertyChanged(nameof(ImageCount));
            OnPropertyChanged(nameof(TotalMediaCount));
        }

        public void AddImage(TtImage image)
        {
            _Images.Add(image);
            UpdateCounts();
        }

        public void RemoveImage(TtImage image)
        {
            _Images.Remove(image);
            UpdateCounts();
        }

        public override string ToString()
        {
            return $"{_Point.PID} ({_Point.Polygon})";
        }
    }
}
