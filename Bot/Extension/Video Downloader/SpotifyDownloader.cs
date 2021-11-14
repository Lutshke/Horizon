using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.Lavalink;
using Horizon.Interface;
using SpotifyAPI.Web;
using static Horizon.Extensions.Checker;

namespace Horizon.Downloader
{
    public class SpotifyDownloader : IVideoDownloader
    {
        public string Name { get; set; } = "spotify";
        public LavalinkNodeConnection Node { get; }

        public SpotifyDownloader(LavalinkNodeConnection node)
        {
            Node = node;
        }

        private static string GetSpotifyId(string url)
        {
            return Regex.Match(url, @"(?:(?:track)|(?:playlist))\/(\w+)\??", RegexOptions.Compiled).Groups[1].Value;
        }

        public async Task<List<IVideo>> GetVideos(string query, DiscordUser user)
        {
            string SpotifyId = GetSpotifyId(query);
            var track = query.Contains("track")
                ? await Bot.Spotify.Tracks.Get(SpotifyId).ConfigureAwait(false)
                : (await Bot.Spotify.Playlists.Get(SpotifyId).ConfigureAwait(false)).Tracks.Items.First().Track as FullTrack;
            var tracks = await Node.Rest.GetTracksAsync($"{track.Name} {track.Artists.First().Name}").ConfigureAwait(false);
            CheckForSong(tracks);
            return tracks.Tracks.Take(1).Select(track => new DefaultVideo(user, track) as IVideo).ToList();
        }
    }
}