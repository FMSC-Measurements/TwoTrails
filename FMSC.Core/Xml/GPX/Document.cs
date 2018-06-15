using System.Collections.Generic;
using System.Linq;

namespace FMSC.Core.Xml.GPX
{
    public class GpxDocument
    {
        public string Author { get; set; }
        public Metadata Metadata { get; set; }
        public string Extensions { get; set; }

        public List<Point> Waypoints { get; } = new List<Point>();
        public List<Route> Routes { get; } = new List<Route>();
        public List<Track> Tracks { get; } = new List<Track>();
        

        public GpxDocument(string author = null)
        {
            Author = author ?? "Auto Generated";
        }


        public Point GetPointById(string id) => Waypoints.FirstOrDefault(p => p.ID == id);
        
        public void DeletePointById(string id)
        {
            for (int i = Waypoints.Count -1; i > -1; i--)
            {
                if (Waypoints[i].ID == id)
                {
                    Waypoints.RemoveAt(i);
                }
            }
        }

        public void DeletePointByName(string name)
        {
            for (int i = Waypoints.Count - 1; i > -1; i--)
            {
                if (Waypoints[i].Name == name)
                {
                    Waypoints.RemoveAt(i);
                }
            }
        }


        public Route GetRouteByName(string name) => Routes.FirstOrDefault(r => r.Name == name);

        public void DeleteRouteByName(string name)
        {
            for (int i = Routes.Count - 1; i > -1; i--)
            {
                if (Routes[i].Name == name)
                {
                    Routes.RemoveAt(i);
                }
            }
        }


        public Track GetTrackByName(string name) => Tracks.FirstOrDefault(t => t.Name == name);

        public void DeleteTrackByName(string name)
        {
            for (int i = Tracks.Count - 1; i > -1; i--)
            {
                if (Tracks[i].Name == name)
                {
                    Tracks.RemoveAt(i);
                }
            }
        }
    }
}
