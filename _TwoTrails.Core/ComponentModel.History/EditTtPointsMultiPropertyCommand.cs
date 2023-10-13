using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TwoTrails.Core.Points;

namespace TwoTrails.Core.ComponentModel.History
{
    /// <summary>
    /// Command To edit one property in a point for multiple points
    /// </summary>
    public class EditTtPointsMultiPropertyCommand : ITtPointsCommand
    {
        private readonly List<object> NewValues;
        private readonly List<object> OldValues = new List<object>();
        private readonly List<PropertyInfo> Properties;

        public EditTtPointsMultiPropertyCommand(TtManager manager, IEnumerable<TtPoint> points, IEnumerable<PropertyInfo> properties, IEnumerable<object> newValues) : base(manager, points)
        {
            RequireRefresh = properties.Any(p => p == PointProperties.INDEX);

            this.Properties = new List<PropertyInfo>(properties);
            this.NewValues = new List<object>(newValues);

            for (int i = 0; i < Points.Count; i++)
            {
                OldValues.Add(Properties[i].GetValue(Points[i]));
            }
        }

        public override void Redo()
        {
            for (int i = 0; i < Points.Count; i++)
            {
                Properties[i].SetValue(Points[i], NewValues[i]);
            }
        }

        public override void Undo()
        {
            for (int i = 0; i < Points.Count; i++)
            {
                Properties[i].SetValue(Points[i], OldValues[i]);
            }
        }

        protected override DataActionType GetActionType() => DataActionType.ModifiedPoints;
        protected override String GetCommandInfoDescription() => $"Edit {Properties.Count} properties of {Points.Count} points";
    }

    /// <summary>
    /// Command To edit one one property in a point for multiple points
    /// </summary>
    public class EditTtPointsMultiPropertyCommand<T> : ITtPointsCommand
    {
        private readonly List<T> NewValues;
        private readonly List<T> OldValues = new List<T>();
        private readonly List<PropertyInfo> Properties;

        public EditTtPointsMultiPropertyCommand(TtManager manager, IEnumerable<TtPoint> points, IEnumerable<PropertyInfo> properties, IEnumerable<T> newValues) : base(manager, points)
        {
            RequireRefresh = properties.Any(p => p == PointProperties.INDEX);

            this.Properties = new List<PropertyInfo>(properties);
            this.NewValues = new List<T>(newValues);

            for (int i = 0; i < Points.Count; i++)
            {
                OldValues.Add((T)Properties[i].GetValue(Points[i]));
            }
        }

        public override void Redo()
        {
            for (int i = 0; i < Points.Count; i++)
            {
                Properties[i].SetValue(Points[i], NewValues[i]);
            }
        }

        public override void Undo()
        {
            for (int i = 0; i < Points.Count; i++)
            {
                Properties[i].SetValue(Points[i], OldValues[i]);
            }
        }

        protected override DataActionType GetActionType() => DataActionType.ModifiedPoints;
        protected override String GetCommandInfoDescription() => $"Edit {Properties.Count} properties of {Points.Count} points";
    }
}
