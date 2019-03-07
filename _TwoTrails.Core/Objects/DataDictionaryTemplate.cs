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
        

        public DataDictionaryTemplate(IEnumerable<DataDictionaryField> dataDictionaryFields = null)
        {
            if (dataDictionaryFields != null)
            {
                foreach (DataDictionaryField field in dataDictionaryFields)
                {
                    AddField(field);
                } 
            }
        }


        public bool HasField(string cn)
        {
            return _Fields.ContainsKey(cn);
        }

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


        public DataDictionary CreateDefaultDataDictionary(string pointCN = null)
        {
            return new DataDictionary(pointCN, this);
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

    public class DataDictionaryField : IEquatable<DataDictionaryField>
    {
        public string CN { get; }

        public String Name { get; set; }

        public int Order { get; set; }

        public FieldType FieldType { get; set; }

        public int Flags { get; set; }

        public List<String> Values { get; set; }

        public object DefaultValue { get; set; }

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
                return DefaultValue;
                //switch (DataType)
                //{
                //    case DataType.INTEGER: return Int32.Parse(DefaultValue);
                //    case DataType.DECIMAL: return Decimal.Parse(DefaultValue);
                //    case DataType.FLOAT: return Double.Parse(DefaultValue);
                //    case DataType.TEXT: return DefaultValue;
                //    case DataType.BYTE_ARRAY: return null;// Int32.Parse(DefaultValue);
                //    case DataType.BOOLEAN: return Boolean.Parse(DefaultValue);
                //    default: throw new Exception("Invalid DataType");
                //}
            }
            else if (ValueRequired)
            {
                switch (DataType)
                {
                    case DataType.INTEGER: return 0;
                    case DataType.DECIMAL: return 0m;
                    case DataType.FLOAT: return 0d;
                    case DataType.TEXT: return String.Empty;
                    case DataType.BYTE_ARRAY: return null;// new byte[0];
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


        public override bool Equals(object obj)
        {
            return obj is DataDictionaryField field && field == this;
        }

        public bool Equals(DataDictionaryField other)
        {
            return
                this.CN == other.CN &&
                this.Name == other.Name &&
                this.Order == other.Order &&
                this.FieldType == other.FieldType &&
                this.Flags == other.Flags &&
                this.DataType == other.DataType &&
                this.ValueRequired == other.ValueRequired &&
                ((this.Values == null) == (other.Values == null) &&
                    (this.Values == null || (this.Values.Count == other.Values.Count && this.Values.SequenceEqual(other.Values)))) &&
                ((this.DefaultValue == null) == (other.DefaultValue == null) &&
                    (this.DefaultValue == null || this.DefaultValue.Equals(other.DefaultValue)));
            //(!(this.Values != null ^ other.Values != null) &&
            //    (this.Values == null || (this.Values.Count == other.Values.Count && this.Values.SequenceEqual(other.Values)))) &&
            //(!(this.DefaultValue != null ^ other.DefaultValue != null) &&
            //    (this.DefaultValue == null || this.DefaultValue.Equals(other.DefaultValue)));

        }
        
        public override int GetHashCode()
        {
            var hashCode = 1869274798;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(CN);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
            hashCode = hashCode * -1521134295 + Order.GetHashCode();
            hashCode = hashCode * -1521134295 + FieldType.GetHashCode();
            hashCode = hashCode * -1521134295 + Flags.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<List<string>>.Default.GetHashCode(Values);
            hashCode = hashCode * -1521134295 + EqualityComparer<object>.Default.GetHashCode(DefaultValue);
            hashCode = hashCode * -1521134295 + DataType.GetHashCode();
            hashCode = hashCode * -1521134295 + ValueRequired.GetHashCode();
            return hashCode;
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
        TEXT = 3,
        BYTE_ARRAY = 4,
        BOOLEAN = 5
    }
}
