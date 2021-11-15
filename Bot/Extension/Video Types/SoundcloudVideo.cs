using System;
using DSharpPlus.Entities;
using DSharpPlus.Lavalink;
using Horizon.Interface;

namespace Horizon
{
    public class SoundcloudVideo : IVideo
    {
        public LavalinkTrack Track { get; set; }
        public string Title { get; set; }
        public string VideoUrl { get; set; }
        public TimeSpan Duration { get; set; }
        public string Uploader { get; set; }
        public DiscordUser Requester { get; set; }

        public SoundcloudVideo(DiscordUser user, LavalinkTrack track, string title, string query)
        {
            this.Track = track;
            this.Requester = user;

            this.Title = title;
            this.VideoUrl = query;
            this.Duration = track.Length;
            this.Uploader = "unknown";
        }

        public DiscordEmbedBuilder VideoEmbed()
        {
            return new DiscordEmbedBuilder()
                .WithTitle($"`{this.Title}`")
                .WithUrl(this.VideoUrl)
                .WithDescription($"Duration: `{(this.Track.IsStream ? "LIVE" : Track?.Length.ToString(@"hh\:mm\:ss"))}`")
                .WithAuthor($"Requested by: {this.Requester.Username}", null, this.Requester.AvatarUrl)
                .WithColor(new DiscordColor("2f3136"))
                .WithTimestamp(DateTime.Now);
        }
    }
}