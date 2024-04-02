using System.Collections.Generic;
using System.Reflection;

namespace TwoTrails.Core.ComponentModel.History
{
    public class EditTtPolygonsCommand : ITtPolygonsCommand
    {
        private readonly object NewValue;
        private readonly List<object> OldValues = new List<object>();
        private readonly PropertyInfo Property;

        public EditTtPolygonsCommand(TtManager manager, IEnumerable<TtPolygon> polygons, PropertyInfo property, object newValue) : base(manager, polygons)
        {
            this.Property = property;
            this.NewValue = newValue;

            foreach (TtPolygon polygon in polygons)
            {
                OldValues.Add(property.GetValue(polygon));
            }
        }

        public override void Redo()
        {
            foreach (TtPolygon polygon in Polygons)
            {
                Property.SetValue(polygon, NewValue);
            }
        }

        public override void Undo()
        {
            for (int i = 0; i < Polygons.Count; i++)
            {
                Property.SetValue(Polygons[i], OldValues[i]);
            }
        }

        protected override DataActionType GetActionType() => DataActionType.ModifiedPolygons;
        protected override string GetCommandInfoDescription() => $"Edit {Property.Name} of {Polygons.Count} polygons";
    }
}
