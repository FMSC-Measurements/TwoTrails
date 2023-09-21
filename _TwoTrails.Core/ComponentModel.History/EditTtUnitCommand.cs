using System.Reflection;
using TwoTrails.Core.Units;

namespace TwoTrails.Core.ComponentModel.History
{
    public class EditTtUnitCommand<T> : ITtUnitCommand
    {
        private T NewValue;
        private T OldValue;
        private PropertyInfo Property;

        public EditTtUnitCommand(TtUnit unit, PropertyInfo property, T newValue) : base(unit)
        {
            this.Property = property;
            this.NewValue = newValue;
            this.OldValue = (T)property.GetValue(unit);
        }

        public override void Redo()
        {
            Property.SetValue(Unit, NewValue);
        }

        public override void Undo()
        {
            Property.SetValue(Unit, OldValue);
        }
    }
}
