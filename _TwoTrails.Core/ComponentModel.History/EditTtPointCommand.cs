using System.Reflection;
using TwoTrails.Core.Points;

namespace TwoTrails.Core.ComponentModel.History
{
    public class EditTtPointCommand<T> : ITtPointCommand
    {
        private readonly T NewValue;
        private readonly T OldValue;
        private readonly PropertyInfo Property;

        public EditTtPointCommand(TtManager manager, TtPoint point, PropertyInfo property, T newValue) : base(manager, point)
        {
            RequireRefresh = property == PointProperties.INDEX;

            this.Property = property;
            this.NewValue = newValue;
            this.OldValue = (T)property.GetValue(point);
        }

        public override void Redo()
        {
            Property.SetValue(Point, NewValue);
        }

        public override void Undo()
        {
            Property.SetValue(Point, OldValue);
        }


        protected override DataActionType GetActionType() => DataActionType.ModifiedPoints;
        protected override string GetCommandInfoDescription() => $"Edit {Property.Name} of point {Point}";
    }
}
