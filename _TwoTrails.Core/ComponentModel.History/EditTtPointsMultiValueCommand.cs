using System;
using System.Collections.Generic;
using System.Reflection;
using TwoTrails.Core.Points;

namespace TwoTrails.Core.ComponentModel.History
{
    public class EditTtPointsMultiValueCommand<T> : ITtPointsCommand
    {
        protected readonly List<T> NewValues;
        protected readonly List<T> OldValues = new List<T>();
        protected readonly PropertyInfo Property;

        public EditTtPointsMultiValueCommand(IEnumerable<TtPoint> points, PropertyInfo property, IEnumerable<T> newValues) : base(points)
        {
            RequireRefresh = property == PointProperties.INDEX;

            this.Property = property;
            this.NewValues = new List<T>(newValues);

            foreach (TtPoint point in points)
            {
                OldValues.Add((T)property.GetValue(point));
            }
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

        protected override DataActionType GetActionType() => DataActionType.ModifiedPoints;
        protected override String GetCommandInfoDescription() => $"Edit {Property.Name} properties of {Points.Count} points";
    }
}
