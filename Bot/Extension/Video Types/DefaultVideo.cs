using System;
using DSharpPlus.Entities;
using DSharpPlus.Lavalink;
using Horizon.Interface;

namespace Horizon
{
    public class DefaultVideo : IVideo
    {
        public LavalinkTrack Track { get; set; }
        public string Title { get; set; }
        public string VideoUrl { get; set; }
        public TimeSpan Duration { get; set; }
        public string Uploader { get; set; }
        public DiscordUser Requester { get; set; }

        public DefaultVideo(DiscordUser user, LavalinkTrack track)
        {
            this.Track = track;
            this.Requester = user;

            this.Title = track.Title;
            this.VideoUrl = track.Uri.AbsoluteUri;
            this.Duration = track.Length;
            this.Uploader = track.Author;
        }

        public DiscordEmbedBuilder VideoEmbed()
        {
            return new DiscordEmbedBuilder()
                .WithTitle($"`{this.Title}`")
                .WithUrl(this.VideoUrl)
                .WithDescription($"Duration: `{(this.Track.IsStream ? "LIVE" : Track?.Length.ToString(@"hh\:mm\:ss"))}` | Uploader: `{this.Uploader}`")
                .WithAuthor($"Requested by: {this.Requester.Username}", null, this.Requester.AvatarUrl)
                .WithColor(new DiscordColor("2f3136"))
                .WithTimestamp(DateTime.Now);
        }
    }
}