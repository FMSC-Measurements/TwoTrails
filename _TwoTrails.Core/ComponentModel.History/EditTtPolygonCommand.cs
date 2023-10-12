﻿using System.Reflection;

namespace TwoTrails.Core.ComponentModel.History
{
    public class EditTtPolygonCommand<T> : ITtPolygonCommand
    {
        private T NewValue;
        private T OldValue;
        private PropertyInfo Property;

        public EditTtPolygonCommand(TtPolygon polygon, PropertyInfo property, T newValue) : base(polygon)
        {
            this.Property = property;
            this.NewValue = newValue;
            this.OldValue = (T)property.GetValue(polygon);
        }

        public override void Redo()
        {
            Property.SetValue(Polygon, NewValue);
        }

        public override void Undo()
        {
            Property.SetValue(Polygon, OldValue);
        }

        protected override DataActionType GetActionType() => DataActionType.ModifiedPolygons;
        protected override string GetCommandInfoDescription() => $"Edit {Property.Name} of polygon {Polygon.Name}";
    }
}
