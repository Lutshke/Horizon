using System.Collections.Generic;
using System.Linq;

namespace Horizon.Extensions.Database
{
    public class DBPlaylist
    {
        public string Title { get; set; }
        public ulong Author { get; set; }
        public int Count => Tracks.Count;
        public List<string> Tracks { get; set; }
    }
}