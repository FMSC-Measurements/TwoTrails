using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using TwoTrails.Core.Points;

namespace TwoTrails.Core.ComponentModel.History
{
    public class EditTtPointsMultiValueCommand : ITtPointsCommand
    {
        private List<object> NewValues;
        private List<object> OldValues = new List<object>();
        private PropertyInfo Property;

        public EditTtPointsMultiValueCommand(IEnumerable<TtPoint> points, PropertyInfo property, IEnumerable<object> newValues, bool autoCommit = true) : base(points)
        {
            this.Property = property;
            this.NewValues = new List<object>(newValues);

            foreach (TtPoint point in points)
            {
                OldValues.Add(property.GetValue(point));
            }

            if (autoCommit)
                Redo();
        }

        public override void Redo()
        {
            for (int i = 0; i < Points.Count; i++)
            {
                Property.SetValue(Points[i], NewValues[i]);
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
