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
            var response = new List<IVideo>();
            string SpotifyId = GetSpotifyId(query);
            var tracks = query.Contains("track")
                ? new List<FullTrack>() { await Bot.Spotify.Tracks.Get(SpotifyId).ConfigureAwait(false) }
                : (await Bot.Spotify.Playlists.Get(SpotifyId).ConfigureAwait(false)).Tracks.Items.Select(m => m.Track).Cast<FullTrack>().ToList();

            foreach (var item in tracks)
            {
                var track = await Node.Rest.GetTracksAsync($"{item.Name} {item.Artists.First().Name}").ConfigureAwait(false);
                CheckForSong(track);
                response.Add(new LavalinkVideo(user, track.Tracks.First()));
            }
            return response;
        }
    }
}