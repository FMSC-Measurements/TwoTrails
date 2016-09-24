using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FMSC.Core.Xml.KML
{
    public class KmlDocument
    {
        #region Properties

        public string Name { get; set; }
        public string Desctription { get; set; }
        public string StyleUrl { get; set; }
        public Properties Properties { get; set; }
        public bool? Open { get; set; }
        public bool? Visibility { get; set; }

        public string CN { get; }
        public List<Style> Styles { get; } = new List<Style>();
        public List<StyleMap> StyleMaps { get; } = new List<StyleMap>();
        public List<Folder> SubFolders { get; } = new List<Folder>();
        public List<Placemark> PlaceMarks { get; } = new List<Placemark>();
        #endregion
        

        public KmlDocument(string name = null, string desc = null)
        {
            Name = name ?? String.Empty;
            Desctription = desc ?? String.Empty;
            CN = Guid.NewGuid().ToString();
        }
       

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
            for (int i = PlaceMarks.Count - 1; i > -1; i--)
            {
                if (PlaceMarks[i].CN == cn)
                {
                    PlaceMarks.RemoveAt(i);
                    return;
                }
            }
        }

        public Placemark GetPlacemark(string cn) => PlaceMarks.FirstOrDefault(p => p.CN == cn);

        public Placemark GetPlacemarkByName(string name) => PlaceMarks.FirstOrDefault(p => p.Name == name);
        

        public void RemoveStyleById(string id)
        {
            for (int i = Styles.Count - 1; i > -1; i--)
            {
                if (Styles[i].ID == id)
                {
                    Styles.RemoveAt(i);
                    return;
                }
            }
        }

        public Style GetStyleById(string id) => Styles.FirstOrDefault(s => s.ID == id);
        
        public void RemoveStyleMapById(string id)
        {
            for (int i = StyleMaps.Count - 1; i > -1; i--)
            {
                if (StyleMaps[i].ID == id)
                {
                    StyleMaps.RemoveAt(i);
                    return;
                }
            }
        }

        public StyleMap GetStyleMapById(string id) => StyleMaps.FirstOrDefault(s => s.ID == id);
    }

}
