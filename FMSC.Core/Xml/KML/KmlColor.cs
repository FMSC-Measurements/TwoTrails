using System;
using System.Collections.Generic;

namespace FMSC.Core.Xml.KML
{
    public class KmlColor
    {
        private int _r = 0;
        public int R
        {
            get { return _r; }
            set
            {
                if (value > 255)
                    _r = 255;
                else if (value < 0)
                    _r = 0;
                else
                    _r = value;
            }
        }

        private int _g = 0;
        public int G
        {
            get { return _g; }
            set
            {
                if (value > 255)
                    _g = 255;
                else if (value < 0)
                    _g = 0;
                else
                    _g = value;
            }
        }

        private int _b = 0;
        public int B
        {
            get { return _b; }
            set
            {
                if (value > 255)
                    _b = 255;
                else if (value < 0)
                    _b = 0;
                else
                    _b = value;
            }
        }

        private int _a = 255;
        public int A
        {
            get { return _a; }
            set
            {
                if (value > 255)
                    _a = 255;
                else if (value < 0)
                    _a = 0;
                else
                    _a = value;
            }
        }


        public KmlColor() { }

        public KmlColor (int color, bool abgrFormat = false)
        {
            if (abgrFormat)
            {
                A = color % 256;
                B = (color / 256) % 256;
                G = (color / 65536) % 256;
                R = (color / 16777216) % 256;
            }
            else
            {
                R = color % 256;
                G = (color / 256) % 256;
                B = (color / 65536) % 256;
                A = (color / 16777216) % 256;
            }

            if (A == 0)
                A = 255;
        }

        public KmlColor(string color, bool abgrFormat = false)
        {
            SetColorFromStringRGBA(color, abgrFormat);
        }

        public KmlColor(KmlColor color)
        {
            A = color.A;
            R = color.R;
            G = color.G;
            B = color.B;
        }

        public KmlColor(int r, int g, int b, int a)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }

        /// <summary>
        /// Sets Color from a string
        /// </summary>
        /// <param name="color">Color in format of ######## (3, 2 alpha-numeric values per hue and 2 for opacity)</param>
        public void SetColorFromStringRGBA(string color, bool abgrFormat = false)
        {
            if (color == null || color.Length < 2)
                throw new Exception("String must be greater than 2 and even");

            if (color.Length % 2 != 0)
                throw new Exception("String must be even");

            int i = 0;
            List<string> colors = new List<string>();


            for (; i < color.Length; i += 2)
            {
                colors.Add(color.Substring(i, 2));
            }

            int len = colors.Count;

            if (abgrFormat)
            {
                if (len > 3)
                    R = System.Convert.ToInt32(colors[3], 16);
                if (len > 2)
                    G = System.Convert.ToInt32(colors[2], 16);
                if (len > 1)
                    B = System.Convert.ToInt32(colors[1], 16);
                if (len > 0)
                    A = System.Convert.ToInt32(colors[0], 16);
            }
            else
            {
                if (len > 0)
                    R = System.Convert.ToInt32(colors[0], 16);
                if (len > 1)
                    G = System.Convert.ToInt32(colors[1], 16);
                if (len > 2)
                    B = System.Convert.ToInt32(colors[2], 16);
                if (len > 3)
                    A = System.Convert.ToInt32(colors[3], 16);
            }
        }

        public string ToStringRGBA()
        {
            return $"{R:X2}{G:X2}{B:X2}{A:X2}";
        }

        public string ToStringABGR()
        {
            return $"{A:X2}{B:X2}{G:X2}{R:X2}";
        }

        /// <returns>ABGR format</returns>
        public override string ToString()
        {
            return ToStringABGR();
        }
    }
}
