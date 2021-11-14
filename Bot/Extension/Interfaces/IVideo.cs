using System;
using DSharpPlus.Entities;
using DSharpPlus.Lavalink;

namespace Horizon.Interface
{
    public interface IVideo
    {
        LavalinkTrack Track { get; set; }
        string Title { get; set; }
        string VideoUrl { get; set; }
        TimeSpan Duration { get; set; }
        string Uploader { get; set; }
        DiscordUser Requester { get; set; }
        DiscordEmbedBuilder VideoEmbed();
    }
}