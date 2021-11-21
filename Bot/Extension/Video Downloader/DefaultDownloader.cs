using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.Lavalink;
using Horizon.Interface;
using static Horizon.Extensions.Checker;

namespace Horizon.Downloader
{
    public class DefaultDownloader : IVideoDownloader
    {
        public string Name { get; set; } = "default";
        public LavalinkNodeConnection Node { get; }

        public DefaultDownloader(LavalinkNodeConnection node)
        {
            Node = node;
        }

        public async Task<List<IVideo>> GetVideos(string query, DiscordUser user)
        {
            var tracks = query.StartsWith("http")
                ? (await Node.Rest.GetTracksAsync(new Uri(query)).ConfigureAwait(false))
                : (await Node.Rest.GetTracksAsync(query).ConfigureAwait(false));

            CheckForSong(tracks);
            return GetTracksAsIVideo(user, tracks);
        }

        private static List<IVideo> GetTracksAsIVideo(DiscordUser user, LavalinkLoadResult tracks)
        {
            return (tracks.LoadResultType == LavalinkLoadResultType.PlaylistLoaded
                            ? tracks.Tracks.Select(track => new LavalinkVideo(user, track))
                            : tracks.Tracks.Take(1).Select(track => new LavalinkVideo(user, track)))
            .Cast<IVideo>().ToList();
        }
    }
}