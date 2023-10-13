using System.Collections.Generic;
using System.Reflection;
using TwoTrails.Core.Points;

namespace TwoTrails.Core.ComponentModel.History
{
    public class EditTtPointsCommand : ITtPointsCommand
    {
        private readonly object NewValue;
        private readonly List<object> OldValues = new List<object>();
        private readonly PropertyInfo Property;

        public EditTtPointsCommand(TtManager manager, IEnumerable<TtPoint> points, PropertyInfo property, object newValue) : base(manager, points)
        {
            RequireRefresh = property == PointProperties.INDEX;

            this.Property = property;
            this.NewValue = newValue;

            foreach (TtPoint point in points)
            {
                OldValues.Add(property.GetValue(point));
            }
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

        protected override DataActionType GetActionType() => DataActionType.ModifiedPoints;
        protected override string GetCommandInfoDescription() => $"Edit {Property.Name} of {Points.Count} points";
    }
}
