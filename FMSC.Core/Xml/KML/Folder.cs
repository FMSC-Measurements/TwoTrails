using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FMSC.Core.Xml.KML
{
    public class Folder
    {
        #region Properties

        public string Name { get; set; }
        public string Desctription { get; set; }
        public string StyleUrl { get; set; }
        public Properties Properties { get; set; }
        public bool? Open { get; set; }
        public bool? Visibility { get; set; }

        public string CN { get; }
        public List<Folder> SubFolders { get; } = new List<Folder>();
        public List<Placemark> Placemarks { get; } = new List<Placemark>();
        #endregion

        
        public Folder(string name, string desc = null)
        {
            Name = name;
            CN = Guid.NewGuid().ToString();

            Desctription = desc;
        }


        #region Methods
        
        public void RemoveFolder(string cn)
        {
            for (int i = SubFolders.Count - 1; i > -1; i--)
            {
                if (SubFolders[i].CN == cn)
                {
                    SubFolders.RemoveAt(i);
                    return;
                }
            }
        }

        public Folder GetFolder(string cn) => SubFolders.FirstOrDefault(f => f.CN == cn);

        public Folder GetFolderByName(string name) => SubFolders.FirstOrDefault(f => f.Name == name);

        
        public void RemovePlacemark(string cn)
        {
            for (int i = Placemarks.Count - 1; i > -1; i--)
            {
                if (Placemarks[i].CN == cn)
                {
                    Placemarks.RemoveAt(i);
                    return;
                }
            }
        }

        public Placemark GetPlacemark(string cn) => Placemarks.FirstOrDefault(p => p.CN == cn);

        public Placemark GetPlacemarkByName(string name) => Placemarks.FirstOrDefault(p => p.Name == name);
        #endregion
    }
}
