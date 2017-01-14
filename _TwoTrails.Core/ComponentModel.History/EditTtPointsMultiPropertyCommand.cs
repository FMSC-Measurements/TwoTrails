using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using TwoTrails.Core.Points;

namespace TwoTrails.Core.ComponentModel.History
{
    public class EditTtPointsMultiPropertyCommand : ITtPointsCommand
    {
        private List<object> NewValues;
        private List<object> OldValues = new List<object>();
        private List<PropertyInfo> Properties;

        public EditTtPointsMultiPropertyCommand(IEnumerable<TtPoint> points, IEnumerable<PropertyInfo> properties, IEnumerable<object> newValues, bool autoCommit = true) : base(points)
        {
            RequireRefresh = properties.Any(p => p == PointProperties.INDEX);

            this.Properties = new List<PropertyInfo>(properties);
            this.NewValues = new List<object>(newValues);
            
            for (int i = 0; i < Points.Count; i++)
            {
                OldValues.Add(Properties[i].GetValue(Points[i]));
            }

            if (autoCommit)
                Redo();
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
    }
}
