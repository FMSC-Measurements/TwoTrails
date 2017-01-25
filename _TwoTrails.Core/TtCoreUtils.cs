﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using TwoTrails.Core.Points;

namespace TwoTrails.Core
{
    public static class TtCoreUtils
    {
        public static TtPoint GetPointByType(OpType op)
        {
            switch (op)
            {
                case OpType.GPS: return new GpsPoint();
                case OpType.Take5: return new Take5Point();
                case OpType.Traverse: return new TravPoint();
                case OpType.SideShot: return new SideShotPoint();
                case OpType.Quondam: return new QuondamPoint();
                case OpType.Walk: return new WalkPoint();
                case OpType.WayPoint: return new WayPoint();
            }

            throw new Exception("Unknown OpType");
        }

        public static DateTime ParseTime(String value)
        {
            DateTime time;

            if (!DateTime.TryParse(value, out time))
            {
                if (!DateTime.TryParseExact(value, Consts.DATE_FORMAT, CultureInfo.InvariantCulture, DateTimeStyles.None, out time))
                {
                    time = DateTime.Now;
                }
            }

            return time;
        }
    }
}