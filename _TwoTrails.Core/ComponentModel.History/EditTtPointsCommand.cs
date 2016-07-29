using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using TwoTrails.Core.Points;

namespace TwoTrails.Core.ComponentModel.History
{
    public class EditTtPointsCommand : ITtPointsCommand
    {
        private object NewValue;
        private List<object> OldValues = new List<object>();
        private PropertyInfo Property;

        public EditTtPointsCommand(List<TtPoint> points, PropertyInfo property, object newValue, bool autoCommit = true) : base(points)
        {
            this.Property = property;
            this.NewValue = newValue;

            foreach (TtPoint point in points)
            {
                OldValues.Add(property.GetValue(points));
            }

            if (autoCommit)
                Redo();
        }

        public override void Redo()
        {
            foreach (TtPoint point in Points)
            {
                Property.SetValue(point, NewValue);
            }
        }

        public override void Undo()
        {
            for (int i = 0; i < Points.Count; i++)
            {
                Property.SetValue(Points[i], OldValues[i]);
            }
        }
    }
}
