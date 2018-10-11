using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FMSC.Core
{
    public static class Extensions
    {
        public static bool GetBit(this int value, byte bitPosition)
        {
            return (value & (1 << bitPosition)) != 0;
        }

        public static void SetBit(this ref int value, bool bitSet, byte bitPosition)
        {
            if (bitSet)
                value |= 1 << bitPosition;
            else
                value &= ~(1 << bitPosition);
        }
    }
}
