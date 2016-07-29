using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using TwoTrails.Core.Points;

namespace TwoTrails.Core.ComponentModel.History
{
    public class EditTtPointCommand : ITtPointCommand
    {
        private object NewValue;
        private object OldValue;
        private PropertyInfo Property;

        public EditTtPointCommand(TtPoint point, PropertyInfo property, object newValue, bool autoCommit = true) : base(point)
        {
            this.Property = property;
            this.NewValue = newValue;
            this.OldValue = property.GetValue(point);

            if (autoCommit)
                Redo();
        }

        public override void Redo()
        {
            Property.SetValue(Point, NewValue);
        }

        public override void Undo()
        {
            Property.SetValue(Point, OldValue);
        }
    }
}
