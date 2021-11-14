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
                ? (await Node.Rest.GetTracksAsync(new Uri(query.Split("&start_radio")[0])).ConfigureAwait(false))
                : (await Node.Rest.GetTracksAsync(query).ConfigureAwait(false));

            CheckForSong(tracks);
            if (query.Contains("&list="))
                return tracks.Tracks.Select(track => new DefaultVideo(user, track) as IVideo).ToList();
            return tracks.Tracks.Take(1).Select(track => new DefaultVideo(user, track) as IVideo).ToList();
        }
    }
}