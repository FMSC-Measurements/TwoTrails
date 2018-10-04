using CSUtil;
using CSUtil.ComponentModel;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System;

namespace TwoTrails.Core
{
    public class DataDictionary : NotifyPropertyChangedEx, IEquatable<DataDictionary>, IEqualityComparer<DataDictionary>, IEnumerable<KeyValuePair<string, object>>
    {
        private readonly Dictionary<string, object> _Data;
        
        public string PointCN { get; private set; }
        

        public DataDictionary(string pointCN = null, Dictionary<string, object> data = null)
        {
            PointCN = pointCN;
            _Data = data ?? new Dictionary<string, object>();
        }

        public DataDictionary(DataDictionary dataDictionary)
        {
            PointCN = dataDictionary.PointCN;
            _Data = dataDictionary.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }
        
        public DataDictionary(string pointCN, DataDictionaryTemplate dataDictionaryTemplate) : this(pointCN)
        {
            foreach (DataDictionaryField field in dataDictionaryTemplate)
                _Data.Add(field.CN, field.GetDefaultValue());
        }


        public void ClearValues()
        {
            foreach (string id in _Data.Keys.ToArray())
                _Data[id] = null;
        }

        public void UpdateValueWOTrigger(string cn, object value)
        {
            if (_Data.ContainsKey(cn))
            {
                object oval = _Data[cn];

                if (oval == null || !oval.Equals(value))
                {
                    _Data[cn] = value;
                }
            }
            else
            {
                _Data.Add(cn, value);
            }
        }

        public object this[string cn]
        {
            get { return _Data[cn]; }
            set
            {
                if (_Data.ContainsKey(cn))
                {
                    object oval = _Data[cn];

                    if (oval == null || !oval.Equals(value))
                    {
                        _Data[cn] = value;
                        OnPropertyChanged(cn);
                    }
                }
                else
                {
                    _Data.Add(cn, value);
                    OnPropertyChanged(cn);
                }
            }
        }


        public override bool Equals(object obj)
        {
            return obj is DataDictionary dd && Equals(this, dd);
        }
        
        public bool Equals(DataDictionary dd)
        {
            return Equals(this, dd);
        }
        
        public bool Equals(DataDictionary x, DataDictionary y)
        {
            return _Data.DictionaryEqual(y._Data);
        }


        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return _Data.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }


        public int GetHashCode(DataDictionary obj)
        {
            return obj.GetHashCode();
        }

        public override int GetHashCode()
        {
            return _Data.GetHashCode();
        }
    }
}
