using System;
using System.Collections.Generic;
using System.Linq;

namespace FMSC.Core.Xml.KML
{
    public class KmlFolder : KmlProperties
    {
        #region Properties
        public string CN { get; }
        public List<KmlFolder> SubFolders { get; } = new List<KmlFolder>();
        public List<Placemark> Placemarks { get; } = new List<Placemark>();
        #endregion

        
        public KmlFolder(string name = null, string desc = null) : base(name, desc)
        {
            CN = Guid.NewGuid().ToString();
        }
        
        public KmlFolder(KmlFolder folder) : base(folder)
        {
            CN = folder.CN;
            SubFolders.AddRange(folder.SubFolders);
            Placemarks.AddRange(folder.Placemarks);
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

        public KmlFolder GetFolder(string cn) => SubFolders.FirstOrDefault(f => f.CN == cn);

        public KmlFolder GetFolderByName(string name) => SubFolders.FirstOrDefault(f => f.Name == name);

        
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
