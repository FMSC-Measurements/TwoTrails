using System;

namespace TwoTrails.Core
{
    public delegate void OnColorChangeEvent(PolygonGraphicOptions pgo, GraphicCode code, int color);

    public enum GraphicCode
    {
        ADJBND_COLOR,
        ADJNAV_COLOR,
        ADJPTS_COLOR,
        UNADJBND_COLOR,
        UNADJNAV_COLOR,
        UNADJPTS_COLOR,
        WAYPTS_COLOR
    }

    public class PolygonGraphicOptions : TtObject
    {
        public event OnColorChangeEvent ColorChanged;

        private int _AdjBndColor;
        public int AdjBndColor
        {
            get { return _AdjBndColor; }
            set
            {
                SetField(ref _AdjBndColor, value, () => { OnColorChange(GraphicCode.ADJBND_COLOR, value); });
            }
        }

        //public SolidColorBrush AdjBndBrush
        //{
        //    get { return new SolidColorBrush(_AdjBndColor); }
        //}


        private int _UnAdjBndColor;
        public int UnAdjBndColor
        {
            get { return _UnAdjBndColor; }
            set
            {
                SetField(ref _UnAdjBndColor, value, () => { OnColorChange(GraphicCode.UNADJBND_COLOR, value); });
            }
        }

        //public SolidColorBrush UnAdjBndBrush
        //{
        //    get { return new SolidColorBrush(_UnAdjBndColor); }
        //}


        private int _AdjNavColor;
        public int AdjNavColor
        {
            get { return _AdjNavColor; }
            set
            {
                SetField(ref _AdjNavColor, value, () => { OnColorChange(GraphicCode.ADJNAV_COLOR, value); });
            }
        }

        //public SolidColorBrush AdjNavBrush
        //{
        //    get { return new SolidColorBrush(_AdjNavColor); }
        //}


        private int _UnAdjNavColor;
        public int UnAdjNavColor
        {
            get { return _UnAdjNavColor; }
            set
            {
                SetField(ref _UnAdjNavColor, value, () => { OnColorChange(GraphicCode.UNADJNAV_COLOR, value); });
            }
        }

        //public SolidColorBrush UnAdjNavBrush
        //{
        //    get { return new SolidColorBrush(_UnAdjNavColor); }
        //}


        private int _AdjPtsColor;
        public int AdjPtsColor
        {
            get { return _AdjPtsColor; }
            set
            {
                SetField(ref _AdjPtsColor, value, () => { OnColorChange(GraphicCode.ADJPTS_COLOR, value); });
            }
        }

        //public SolidColorBrush AdjPtsBrush
        //{
        //    get { return new SolidColorBrush(_AdjPtsColor); }
        //}


        private int _UnAdjPtsColor;
        public int UnAdjPtsColor
        {
            get { return _UnAdjPtsColor; }
            set
            {
                SetField(ref _UnAdjPtsColor, value, () => { OnColorChange(GraphicCode.UNADJPTS_COLOR, value); });
            }
        }

        //public SolidColorBrush UnAdjPtsBrush
        //{
        //    get { return new SolidColorBrush(_UnAdjPtsColor); }
        //}


        private int _WayPtsColor;
        public int WayPtsColor
        {
            get { return _WayPtsColor; }
            set
            {
                SetField(ref _WayPtsColor, value, () => { OnColorChange(GraphicCode.WAYPTS_COLOR, value); });
            }
        }

        //public SolidColorBrush WayPtsBrush
        //{
        //    get { return new SolidColorBrush(_WayPtsColor); }
        //}


        private float _AdjWidth;
        public float AdjWidth
        {
            get { return _AdjWidth; }
            set
            {
                SetField(ref _AdjWidth, value);
            }
        }

        private float _UnAdjWidth;
        public float UnAdjWidth
        {
            get { return _UnAdjWidth; }
            set
            {
                SetField(ref _UnAdjWidth, value);
            }
        }


        public PolygonGraphicOptions(String cn, PolygonGraphicOptions options) : base(cn)
        {
            this._AdjBndColor = options.AdjBndColor;
            this._UnAdjBndColor = options.UnAdjBndColor;
            this._AdjNavColor = options.AdjNavColor;
            this._UnAdjNavColor = options.UnAdjNavColor;
            this._AdjPtsColor = options.AdjPtsColor;
            this._UnAdjPtsColor = options.UnAdjPtsColor;
            this._WayPtsColor = options.WayPtsColor;
            this._AdjWidth = options.AdjWidth;
            this._UnAdjWidth = options.UnAdjWidth;
        }

        public PolygonGraphicOptions(String cn, int adjBndColor, int unAdjBndColor, int adjNavColor, int unAdjNavColor,
                                     int adjPtsColor, int unAdjPtsColor, int wayPtsColor,
                                     float adjWidth, float unAdjWidth) : base(cn)
        {
            this._AdjBndColor = adjBndColor;// GetColor(adjBndColor);
            this._UnAdjBndColor = unAdjBndColor;// GetColor(unAdjBndColor);
            this._AdjNavColor = adjNavColor;// GetColor(adjNavColor);
            this._UnAdjNavColor = UnAdjNavColor;// GetColor(unAdjNavColor);
            this._AdjPtsColor = adjPtsColor;// GetColor(adjPtsColor);
            this._UnAdjPtsColor = unAdjPtsColor;// GetColor(unAdjPtsColor);
            this._WayPtsColor = wayPtsColor;// GetColor(wayPtsColor);
            this._AdjWidth = adjWidth;
            this._UnAdjWidth = unAdjWidth;
        }


        private void OnColorChange(GraphicCode code, int color)
        {
            ColorChanged?.Invoke(this, code, color);

            //switch (code)
            //{
            //    case GraphicCode.ADJBND_COLOR:
            //        OnPropertyChanged(nameof(AdjBndBrush));
            //        break;
            //    case GraphicCode.ADJNAV_COLOR:
            //        OnPropertyChanged(nameof(AdjNavBrush));
            //        break;
            //    case GraphicCode.ADJPTS_COLOR:
            //        OnPropertyChanged(nameof(AdjPtsBrush));
            //        break;
            //    case GraphicCode.UNADJBND_COLOR:
            //        OnPropertyChanged(nameof(UnAdjBndBrush));
            //        break;
            //    case GraphicCode.UNADJNAV_COLOR:
            //        OnPropertyChanged(nameof(UnAdjNavBrush));
            //        break;
            //    case GraphicCode.UNADJPTS_COLOR:
            //        OnPropertyChanged(nameof(UnAdjPtsBrush));
            //        break;
            //    case GraphicCode.WAYPTS_COLOR:
            //        OnPropertyChanged(nameof(WayPtsBrush));
            //        break;
            //}
        }


        public static string ToStringARGB(int color)
        {
            return $"{GetAlpha(color):X2}{GetRed(color):X2}{GetGreen(color):X2}{GetBlue(color):X2}";
        }

        //public static int GetColor(int argb)
        //{
        //    return Color.FromArgb(GetAlpha(argb), GetRed(argb), GetGreen(argb), GetBlue(argb));
        //}

        public static byte GetAlpha(int color)
        {
            return (byte)((color >> 24) & 255);
        }

        public static byte GetRed(int color)
        {
            return (byte)((color >> 16) & 255);
        }

        public static byte GetGreen(int color)
        {
            return (byte)((color >> 8) & 255);
        }

        public static byte GetBlue(int color)
        {
            return (byte)(color & 255);
        }


        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return (obj is PolygonGraphicOptions opts) &&
                AdjBndColor == opts.AdjBndColor &&
                UnAdjBndColor == opts.UnAdjBndColor &&
                AdjNavColor == opts.AdjNavColor &&
                UnAdjNavColor == opts.UnAdjNavColor &&
                AdjPtsColor == opts.AdjPtsColor &&
                UnAdjPtsColor == opts.UnAdjPtsColor &&
                WayPtsColor == opts.WayPtsColor &&
                AdjWidth == opts.AdjWidth &&
                UnAdjWidth == opts.UnAdjWidth;
        }

        public static bool operator ==(PolygonGraphicOptions a, PolygonGraphicOptions b)
        {
            if (a is null ^ b is null)
                return false;

            if (a is null)
                return true;

            return a.AdjBndColor == b.AdjBndColor &&
                a.UnAdjBndColor == b.UnAdjBndColor &&
                a.AdjNavColor == b.AdjNavColor &&
                a.UnAdjNavColor == b.UnAdjNavColor &&
                a.AdjPtsColor == b.AdjPtsColor &&
                a.UnAdjPtsColor == b.UnAdjPtsColor &&
                a.WayPtsColor == b.WayPtsColor &&
                a.AdjWidth == b.AdjWidth &&
                a.UnAdjWidth == b.UnAdjWidth;
        }

        public static bool operator !=(PolygonGraphicOptions a, PolygonGraphicOptions b)
        {
            if (a is null ^ b is null || a is null)
                return true;

            return a.AdjBndColor != b.AdjBndColor ||
                a.UnAdjBndColor != b.UnAdjBndColor ||
                a.AdjNavColor != b.AdjNavColor ||
                a.UnAdjNavColor != b.UnAdjNavColor ||
                a.AdjPtsColor != b.AdjPtsColor ||
                a.UnAdjPtsColor != b.UnAdjPtsColor ||
                a.WayPtsColor != b.WayPtsColor ||
                a.AdjWidth != b.AdjWidth ||
                a.UnAdjWidth != b.UnAdjWidth;
        }
    }
}
