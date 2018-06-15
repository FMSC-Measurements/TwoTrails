namespace FMSC.Core.Xml.GPX
{
    public abstract class BaseTrack
    {
        public string Name { get; set; }
        public string Comment { get; set; }
        public string Description { get; set; }
        public string Source { get; set; }
        public string Link { get; set; }
        public int? Number { get; set; }
        public string Type { get; set; }
        public string Extensions { get; set; }


        public BaseTrack(string name = null, string desc = null)
        {
            Name = name;
            Description = desc;
        }
    }

}
