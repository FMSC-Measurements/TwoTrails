using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using TwoTrails.Core.Points;

namespace TwoTrails.Core.ComponentModel.History
{
    public class EditTtPointCommand<T> : ITtPointCommand
    {
        private T NewValue;
        private T OldValue;
        private PropertyInfo Property;

        public EditTtPointCommand(TtPoint point, PropertyInfo property, T newValue, bool autoCommit = true) : base(point)
        {
            RequireRefresh = property == PointProperties.INDEX;

            this.Property = property;
            this.NewValue = newValue;
            this.OldValue = (T)property.GetValue(point);

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
