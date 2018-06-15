using System.Collections.Generic;

namespace FMSC.Core.Xml.KML
{
    public class ExtendedData
    {
        public List<Data> DataItems { get; set; } = new List<Data>();


        public ExtendedData() { }


        public void AddData(Data data)
        {
            if (data.IsValid)
                DataItems.Add(data);
        }

        /// <summary>
        /// Removes first Data that has a matching ID
        /// </summary>
        /// <param name="id">ID o f data</param>
        public void RemoveDataById(string id)
        {
            for (int i = DataItems.Count - 1; i > -1; i--)
            {
                if (DataItems[i].ID == id)
                {
                    DataItems.RemoveAt(i);
                }
            }
        }

        public class Data
        {
            public string ID { get; set; }
            public string Name { get; set; }
            public string Value { get; set; }

            public bool IsValid { get { return ID != null && Name != null && Value != null; } }


            public Data() : this(null, null, null) { }

            public Data(string name, string value) : this(name, value, null) { }

            public Data(string name, string value, string id)
            {
                Name = name;
                Value = value;
                ID = id;
            }

            public Data(string name, object value) : this(name, value, null) { }

            public Data(string name, object value, string id)
            {
                Name = name;
                Value = value.ToString();
                ID = id;
            }
        }
    }
}
