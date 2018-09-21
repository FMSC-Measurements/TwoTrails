using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace TwoTrails.Core
{
    public class DataDictionaryTemplate : IEnumerable<DataDictionaryField>
    {
        private readonly Dictionary<string, DataDictionaryField> _Fields = new Dictionary<string, DataDictionaryField>();
        

        public void AddField(DataDictionaryField field)
        {
            if (_Fields.ContainsKey(field.CN))
                _Fields[field.CN] = field;
            else
                _Fields.Add(field.CN, field);
        }

        public void RemoveField(string cn)
        {
            if (_Fields.ContainsKey(cn))
                _Fields.Remove(cn);
        }


        public DataDictionary CreateDefaultDataDictionary()
        {
            Dictionary<string, object> data = new Dictionary<string, object>();

            foreach (DataDictionaryField ddf in this)
                data.Add(ddf.CN, ddf.GetDefaultValue());

            return new DataDictionary();
        }

        
        public IEnumerator<DataDictionaryField> GetEnumerator()
        {
            foreach (DataDictionaryField ddf in _Fields.Values.OrderBy(f => f.Order))
                yield return ddf;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public DataDictionaryField this[string cn]
        {
            get { return _Fields[cn]; }
        }
    }

    public class DataDictionaryField
    {
        public string CN { get; }

        public String Name { get; set; }

        public int Order { get; set; }

        public FieldType FieldType { get; set; }

        public int Flags { get; set; }

        public IList<String> Values { get; set; }

        public String DefaultValue { get; set; }

        public DataType DataType { get; set; }

        public bool ValueRequired { get; set; }


        public DataDictionaryField(String cn = null)
        {
            CN = cn ?? Guid.NewGuid().ToString();
        }


        public object GetDefaultValue()
        {
            if (DefaultValue != null)
            {
                switch (DataType)
                {
                    case DataType.INTEGER: return Int32.Parse(DefaultValue);
                    case DataType.DECIMAL: return Decimal.Parse(DefaultValue);
                    case DataType.FLOAT: return Double.Parse(DefaultValue);
                    case DataType.STRING: return DefaultValue;
                    //case DataType.BYTE_ARRAY: return Int32.Parse(DefaultValue);
                    case DataType.BOOLEAN: return Boolean.Parse(DefaultValue);
                    default: throw new Exception("Invalid DataType");
                }
            }
            else if (ValueRequired)
            {
                switch (DataType)
                {
                    case DataType.INTEGER: return 0;
                    case DataType.DECIMAL: return 0m;
                    case DataType.FLOAT: return 0d;
                    case DataType.STRING: return String.Empty;
                    //case DataType.BYTE_ARRAY: return new byte[0];
                    case DataType.BOOLEAN: return false;
                    default: throw new Exception("Invalid DataType");
                }
            }

            return null;
        }

        public T GetDefaultValue<T>()
        {
            return (T)GetDefaultValue();
        }
    }

    public enum FieldType
    {
        ComboBox = 1,
        TextBox = 2,
        CheckBox = 3
    }

    public enum DataType
    {
        INTEGER = 0,
        DECIMAL = 1,
        FLOAT = 2,
        STRING = 3,
        BYTE_ARRAY = 4,
        BOOLEAN = 5
    }
}
