using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TwoTrails.Core.Points;

namespace TwoTrails.Core.ComponentModel.History
{
    /// <summary>
    /// Command To edit multiple properties in many points
    /// </summary>
    public class EditTtPointsMultiPropertyMultiValueCommand : ITtPointsCommand
    {
        private readonly List<object> NewValues;
        private readonly List<object[]> OldValues = new List<object[]>();
        private readonly List<PropertyInfo> Properties;

        public EditTtPointsMultiPropertyMultiValueCommand(TtManager manager, IEnumerable<TtPoint> points, IEnumerable<PropertyInfo> properties, IEnumerable<object> newValues) : base(manager, points)
        {
            RequireRefresh = properties.Any(p => p == PointProperties.INDEX);

            this.Properties = new List<PropertyInfo>(properties);
            this.NewValues = new List<object>(newValues);

            foreach (TtPoint point in Points)
                OldValues.Add(Properties.Select(p => p.GetValue(point)).ToArray());
        }

        public override void Redo()
        {
            foreach (TtPoint point in Points)
            {
                for (int i = 0; i < Properties.Count; i++)
                    Properties[i].SetValue(point, NewValues[i]);
            }
        }

        public override void Undo()
        {
            for (int i = 0; i < Points.Count; i++)
            {
                TtPoint point = Points[i];
                object[] oldVals = OldValues[i];

                for (int j = 0; j < oldVals.Length; j++)
                {
                    Properties[j].SetValue(point, oldVals[j]);
                }
            }
        }

        protected override DataActionType GetActionType() => DataActionType.ModifiedPoints;
        protected override String GetCommandInfoDescription() => $"Edit {Properties.Count} properties of {Points.Count} points";
    }
}
