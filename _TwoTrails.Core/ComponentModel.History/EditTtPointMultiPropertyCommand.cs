﻿using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TwoTrails.Core.Points;

namespace TwoTrails.Core.ComponentModel.History
{
    public class EditTtPointMultiPropertyCommand : ITtPointCommand
    {
        private readonly List<object> NewValues;
        private readonly List<object> OldValues = new List<object>();
        private readonly List<PropertyInfo> Properties;

        public EditTtPointMultiPropertyCommand(TtManager manager, TtPoint point, IEnumerable<PropertyInfo> properties, IEnumerable<object> newValues) : base(manager, point)
        {
            RequireRefresh = properties.Any(p => p == PointProperties.INDEX);

            this.Properties = new List<PropertyInfo>(properties);
            this.NewValues = new List<object>(newValues);

            for (int i = 0; i < Properties.Count; i++)
            {
                OldValues.Add(Properties[i].GetValue(Point));
            }
        }

        public override void Redo()
        {
            for (int i = 0; i < Properties.Count; i++)
            {
                Properties[i].SetValue(Point, NewValues[i]);
            }
        }

        public override void Undo()
        {
            for (int i = 0; i < Properties.Count; i++)
            {
                Properties[i].SetValue(Point, OldValues[i]);
            }
        }

        protected override DataActionType GetActionType() => DataActionType.ModifiedPoints;
        protected override string GetCommandInfoDescription() => $"Edit {Properties.Count} properties of {Point}";
    }
}
