using System.Collections.Generic;

namespace Horizon.Extensions.Database
{
    public class PlaylistDatabase
    {
        public int Count { get; set; }
        public Dictionary<string, DBPlaylist> Playlists { get; set; }
    }
}