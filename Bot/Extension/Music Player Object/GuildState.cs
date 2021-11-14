using System.Collections.Generic;
using DSharpPlus.Entities;
using DSharpPlus.Lavalink;
using Horizon.Interface;

namespace Horizon
{
    public class GuildState
    {
        public bool Loop { get; set; }
        public bool Paused { get; set; }
        public bool Playing { get; set; }
        public bool Shuffle { get; set; }
        public bool Skipped { get; set; }
        public bool LoopQueue { get; set; }
        public IVideo NowPlaying { get; set; }
        public List<IVideo> Queue { get; set; }
        public DiscordChannel Channel { get; set; }
        public DiscordMessage LastMessage { get; set; }
        public LavalinkGuildConnection Connection { get; set; }

        public GuildState()
        {
            this.Queue = new();
            this.Playing = false;
            this.Loop = false;
            this.LoopQueue = false;
            this.Shuffle = false;
            this.Skipped = false;
            this.Connection = null;
            this.LastMessage = null;
        }

        public bool IsRequester(DiscordUser user) => this.NowPlaying.Requester == user;
    }
}