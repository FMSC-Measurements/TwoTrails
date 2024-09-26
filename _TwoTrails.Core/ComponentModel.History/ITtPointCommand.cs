using System;
using TwoTrails.Core.Points;

namespace TwoTrails.Core.ComponentModel.History
{
    public abstract class ITtPointCommand : ITtBaseCommand
    {
        protected TtPoint Point;


        public ITtPointCommand(TtManager manager, TtPoint point) : base(manager)
        {
            this.Point = point ?? throw new ArgumentNullException(nameof(point));
        }


        protected override int GetAffectedItemCount() => 1;
        protected override String GetCommandInfoDescription() => "Point";
    }
}
