using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.Lavalink;
using Horizon.Interface;
using static Horizon.Extensions.Checker;

namespace Horizon.Downloader
{
    public class SoundCloudDownloader : IVideoDownloader
    {
        public string Name { get; set; } = "soundcloud";
        public LavalinkNodeConnection Node { get; }

        public SoundCloudDownloader(LavalinkNodeConnection node)
        {
            Node = node;
        }

        public async Task<List<IVideo>> GetVideos(string query, DiscordUser user)
        {
            var psi = new ProcessStartInfo
            {
                FileName = "yt-dlp",
                Arguments = $"-q -s --no-playlist -g -e {query}",
                RedirectStandardOutput = true,
                UseShellExecute = false
            };

            var ytdl = Process.Start(psi);
            var response = ytdl.StandardOutput.ReadToEnd();
            ytdl.WaitForExit();

            var results = response.Split('\n').Take(2).ToArray();
            var track = await Node.Rest.GetTracksAsync(new Uri(results[1])).ConfigureAwait(false);

            CheckForSong(track);

            return new List<IVideo>() { new YoutubeDLVideo(user, track.Tracks.First(), results[0], query) };
        }
    }
}