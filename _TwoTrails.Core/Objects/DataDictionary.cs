using System;
using System.Collections.Generic;
using System.Text;

namespace TwoTrails.Core
{
    public class DataDictionary
    {
        private readonly Dictionary<string, object> _Data;

        public string PointCN { get; }
        
        
        public DataDictionary(string pointCN, Dictionary<string, object> data = null)
        {
            PointCN = pointCN;
            _Data = data ?? new Dictionary<string, object>();
        }


        public void Update<T>(string cn, T value)
        {
            if (_Data.ContainsKey(cn))
            {
                _Data[cn] = value;
            }
            else
            {
                _Data.Add(cn, value);
            }
        }


        public T Get<T>(string cn)
        {
            return (T)_Data[cn];
        }

    }
}
