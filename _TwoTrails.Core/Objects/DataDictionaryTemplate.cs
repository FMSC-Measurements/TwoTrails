using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

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

        
        public IEnumerator<DataDictionaryField> GetEnumerator()
        {
            foreach (DataDictionaryField f in _Fields.Values)
                yield return f;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class DataDictionaryField
    {
        public string CN { get; }

        public String Name { get; set; }

        public int Order { get; set; }

        public FeildType FeildType { get; set; }

        public int Flag { get; set; }

        public IList<String> Values { get; set; }

        public String DefaultValue { get; set; }

        public DataType DataType { get; set; }


        public DataDictionaryField(String cn = null)
        {
            CN = cn ?? Guid.NewGuid().ToString();
        }
    }

    public enum FeildType
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
