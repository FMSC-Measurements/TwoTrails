using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwoTrails.Core.Points;

namespace TwoTrails.Core.ComponentModel.History
{
    public abstract class ITtPointCommand : ITtCommand
    {
        protected TtPoint Point;

        public ITtPointCommand(TtPoint point)
        {
            this.Point = point;
        }

        public abstract void Redo();

        public abstract void Undo();
    }
}
