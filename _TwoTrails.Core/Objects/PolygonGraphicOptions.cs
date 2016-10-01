using System;
using System.Collections.Generic;
using System.Text;

namespace TwoTrails.Core
{
    public delegate void OnColorChangeEvent(PolygonGraphicOptions pgo, GraphicCode code, int value);

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

        private int _UnAdjBndColor;
        public int UnAdjBndColor
        {
            get { return _UnAdjBndColor; }
            set
            {
                SetField(ref _UnAdjBndColor, value, () => { OnColorChange(GraphicCode.UNADJBND_COLOR, value); });
            }
        }

        private int _AdjNavColor;
        public int AdjNavColor
        {
            get { return _AdjNavColor; }
            set
            {
                SetField(ref _AdjNavColor, value, () => { OnColorChange(GraphicCode.ADJNAV_COLOR, value); });
            }
        }

        private int _UnAdjNavColor;
        public int UnAdjNavColor
        {
            get { return _UnAdjNavColor; }
            set
            {
                SetField(ref _UnAdjNavColor, value, () => { OnColorChange(GraphicCode.UNADJNAV_COLOR, value); });
            }
        }

        private int _AdjPtsColor;
        public int AdjPtsColor
        {
            get { return _AdjPtsColor; }
            set
            {
                SetField(ref _AdjPtsColor, value, () => { OnColorChange(GraphicCode.ADJPTS_COLOR, value); });
            }
        }

        private int _UnAdjPtsColor;
        public int UnAdjPtsColor
        {
            get { return _UnAdjPtsColor; }
            set
            {
                SetField(ref _UnAdjPtsColor, value, () => { OnColorChange(GraphicCode.UNADJPTS_COLOR, value); });
            }
        }

        private int _WayPtsColor;
        public int WayPtsColor
        {
            get { return _WayPtsColor; }
            set
            {
                SetField(ref _WayPtsColor, value, () => { OnColorChange(GraphicCode.WAYPTS_COLOR, value); });
            }
        }

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

        public PolygonGraphicOptions(String cn, int AdjBndColor, int UnAdjBndColor, int AdjNavColor, int UnAdjNavColor,
                                     int AdjPtsColor, int UnAdjPtsColor, int WayPtsColor,
                                     float AdjWidth, float UnAdjWidth) : base(cn)
        {
            this._AdjBndColor = AdjBndColor;
            this._UnAdjBndColor = UnAdjBndColor;
            this._AdjNavColor = AdjNavColor;
            this._UnAdjNavColor = UnAdjNavColor;
            this._AdjPtsColor = AdjPtsColor;
            this._UnAdjPtsColor = UnAdjPtsColor;
            this._WayPtsColor = WayPtsColor;
            this._AdjWidth = AdjWidth;
            this._UnAdjWidth = UnAdjWidth;
        }


        private void OnColorChange(GraphicCode code, int value)
        {
            ColorChanged?.Invoke(this, code, value);
        }


        public static string ToStringRGBA(int color)
        {
            return String.Format("{0:X2}{1:X2}{2:X2}{3:X2}",
                GetRed(color),
                GetGreen(color),
                GetBlue(color),
                GetAlpha(color));
        }

        public static int GetRed(int color)
        {
            return color % 256;
        }

        public static int GetGreen(int color)
        {
            return (color / 256) % 256;
        }

        public static int GetBlue(int color)
        {
            return (color / 65536) % 256;
        }

        public static int GetAlpha(int color)
        {
            return (color / 16777216) % 256;
        }


        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            PolygonGraphicOptions opts = obj as PolygonGraphicOptions;

            return object.ReferenceEquals(opts, null) &&
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
            if (object.ReferenceEquals(a, null) ^ object.ReferenceEquals(b, null))
                return false;

            if (object.ReferenceEquals(a, null))
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
            if (object.ReferenceEquals(a, null) ^ object.ReferenceEquals(b, null) || object.ReferenceEquals(a, null))
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
