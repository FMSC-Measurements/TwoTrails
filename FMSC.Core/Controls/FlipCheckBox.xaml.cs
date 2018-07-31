using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace FMSC.Core.Controls
{
    /// <summary>
    /// Interaction logic for FlipCheckBox.xaml
    /// </summary>
    public partial class FlipCheckBox : UserControl, INotifyPropertyChanged
    {
        public static readonly DependencyProperty FrontProperty =
            DependencyProperty.Register(nameof(Front), typeof(UIElement), typeof(FlipCheckBox), new UIPropertyMetadata(null));
        public UIElement Front
        {
            get { return (UIElement)GetValue(FrontProperty); }
            set { SetValue(FrontProperty, value); }
        }

        public static readonly DependencyProperty BackProperty =
            DependencyProperty.Register(nameof(Back), typeof(UIElement), typeof(FlipCheckBox), new UIPropertyMetadata(null));
        public UIElement Back
        {
            get { return (UIElement)GetValue(BackProperty); }
            set { SetValue(BackProperty, value); }
        }

        public static readonly DependencyProperty FlipDurationProperty =
            DependencyProperty.Register(nameof(FlipDuration), typeof(Duration), typeof(FlipCheckBox), new UIPropertyMetadata((Duration)TimeSpan.FromSeconds(0.5)));
        public Duration FlipDuration
        {
            get { return (Duration)GetValue(FlipDurationProperty); }
            set { SetValue(FlipDurationProperty, value); }
        }



        public static readonly DependencyProperty IsCheckedProperty =
            DependencyProperty.Register(nameof(IsChecked), typeof(bool), typeof(FlipCheckBox), new PropertyMetadata(false));
        public bool IsChecked
        {
            get { return (bool)GetValue(IsCheckedProperty); }
            set
            {
                if (IsChecked != value)
                {
                    SetValue(IsCheckedProperty, value);
                    Flip(value);
                }
            }
        }
        
        private void Flip(bool front)
        {
            var animation = new DoubleAnimation()
            {
                Duration = FlipDuration,
                EasingFunction = EasingFunction,
            };
            animation.To = front ? -1 : 1;
            transform.BeginAnimation(ScaleTransform.ScaleXProperty, animation);

            OnPropertyChanged(new PropertyChangedEventArgs(nameof(IsChecked)));
            OnCheckedChange(new EventArgs());
        }

        private IEasingFunction EasingFunction = new SineEase() { EasingMode = EasingMode.EaseInOut };

        public FlipCheckBox()
        {
            InitializeComponent();

            this.Loaded += (o, s) =>
            {
                if (IsChecked)
                    Flip(true);
            };
        }
        

        public event EventHandler CheckedChange;

        protected virtual void OnCheckedChange(EventArgs e)
        {
            CheckedChange?.Invoke(this, e);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);
        }



        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);

            IsChecked = !IsChecked;
        }
    }

    public class LessThanXToTrueConverter : IValueConverter
    {
        public double X { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (double)value < X;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
