using System;

namespace TwoTrails.Core
{
    public class TtVersionOld
    {
        public int Major;
        public int Minor;
        public int Update;

        public TtVersionOld(int maj, int min, int up)
        {
            Major = maj;
            Minor = min;
            Update = up;
        }

        public TtVersionOld(String versionString)
        {
            Major = Minor = Update = 0;

            if (versionString == null)
                return;

            String[] vals = versionString.Split('.');

            if (vals.Length > 0)
            {
                Int32.TryParse(vals[0], out Major);

                if (vals.Length > 1)
                {
                    Int32.TryParse(vals[1], out Minor);

                    if (vals.Length > 2)
                    {
                        Int32.TryParse(vals[2], out Update);
                    }
                }
            }


        }

        public override string ToString()
        {
            return String.Format("{0}.{1}.{2}", Major, Minor, Update);
        }

        public int ToIntVersion()
        {
            return Major * 10000 + Minor * 100 + Update;
        }


        public override int GetHashCode()
        {
            return Major & Minor & Update;
        }


        public override bool Equals(object obj)
        {
            TtVersionOld other = obj as TtVersionOld;

            if (other == null)
                return false;

            return this.Major == other.Major &&
                this.Minor == other.Minor &&
                this.Update == other.Update;
        }

        public static bool operator ==(TtVersionOld @this, TtVersionOld other)
        {
            return @this.Equals(other);
        }

        public static bool operator !=(TtVersionOld @this, TtVersionOld other)
        {
            return @this.Major != other.Major ||
                @this.Minor != other.Minor ||
                @this.Update != other.Update;
        }

        public static bool operator <(TtVersionOld @this, TtVersionOld other)
        {
            if (@this.Major <= other.Major)
            {
                if (@this.Major == other.Major)
                {
                    if (@this.Minor <= other.Minor)
                    {
                        if (@this.Minor == other.Minor)
                        {
                            if (@this.Update < other.Update)
                            {
                                return true;
                            }
                        }
                        else
                            return true;
                    }
                }
                else
                    return true;
            }

            return false;
        }

        public static bool operator >(TtVersionOld @this, TtVersionOld other)
        {
            if (@this.Major >= other.Major)
            {
                if (@this.Major == other.Major)
                {
                    if (@this.Minor >= other.Minor)
                    {
                        if (@this.Minor == other.Minor)
                        {
                            if (@this.Update > other.Update)
                            {
                                return true;
                            }
                        }
                        else
                            return true;
                    }
                }
                else
                    return true;
            }

            return false;
        }
    }
}
