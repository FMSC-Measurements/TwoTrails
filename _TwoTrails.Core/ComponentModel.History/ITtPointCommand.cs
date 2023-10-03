using System;
using TwoTrails.Core.Points;

namespace TwoTrails.Core.ComponentModel.History
{
    public abstract class ITtPointCommand : ITtBaseCommand
    {
        protected TtPoint Point;


        public ITtPointCommand(TtPoint point)
        {
            this.Point = point ?? throw new ArgumentNullException(nameof(point));
        }

        protected override Type GetAffectedType() => PointProperties.DataType;
        protected override int GetAffectedItemCount() => 1;
        protected override String GetCommandInfoDescription() => "Point";
    }
}
