using System;
using System.Windows.Markup;
using System.Linq;
using System.Collections.Generic;

namespace FMSC.Core.ComponentModel
{
    public class EnumToItemsSource : MarkupExtension
    {
        private readonly Type _type;
        public string Exclusions { get; set; }

        public EnumToItemsSource(Type type)
        {
            _type = type;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (Exclusions == null)
                return Enum.GetValues(_type);

            IEnumerable<string> exclusions = Exclusions.Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries).Select(e => e.ToLower()).ToArray();

            return Enum.GetValues(_type).Cast<object>().Where(e => !exclusions.Contains(e.ToString().ToLower()));
        }
    }
}
