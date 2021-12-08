using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.Lavalink;
using DSharpPlus.Lavalink.EventArgs;
using Horizon.Extensions;
using Horizon.Interface;
using static Horizon.Extensions.Checker;

namespace Horizon
{
    public class MusicPlayer
    {
        //> Music Properties
        private readonly Random Random = new();
        private LavalinkExtension Lavalink { get; set; }
        private LavalinkNodeConnection Node => Lavalink.ConnectedNodes.Values.FirstOrDefault();

        public MusicPlayer(LavalinkExtension lavalink)
        {
            Lavalink = lavalink;
        }

        public async Task HandlePlaybackStart(CommandContext ctx, GuildState state, List<IVideo> result)
        {
            CheckForMemberConnection(ctx.Member);
            if (state.Connection is null)
                state.Connection = await Node.ConnectAsync(ctx.Member.VoiceState.Channel).ConfigureAwait(false);

            var IsPlaylist = result.Count > 1;
            var track = result[0];

            if (IsPlaylist)
            {
                foreach (IVideo trk in result)
                    state.Queue.Add(trk);
                int count = state.Playing ? result.Count : result.Count - 1;
                await ctx.RespondAsync($"Added `{count}` tracks to the Queue!").ConfigureAwait(false);

                if (!state.Playing)
                {
                    await PlayTrack(state, state.Queue.Pop(0)).ConfigureAwait(false);
                    await ctx.Channel.SendMessageAsync($"Now playing {track.Title}!").ConfigureAwait(false);
                }
            }
            else if (state.Playing)
            {
                state.Queue.Add(track);
                await ctx.RespondAsync($"Added `{track.Title}` to the Queue!").ConfigureAwait(false);
            }
            else
            {
                state.Channel = ctx.Channel;
                await PlayTrack(state, track).ConfigureAwait(false);
                await ctx.RespondAsync($"Now playing {track.Title}!").ConfigureAwait(false);
            }
        }

        //> Music Init Handler
        public async Task PlayTrack(GuildState state, IVideo track)
        {
            state.NowPlaying = track;

            state.Connection.PlaybackFinished += PlaybackFinishedHandler;
            state.Connection.PlaybackStarted += PlaybackStartedHandler;
            state.Connection.TrackException += TrackExceptionHandler;
            state.Connection.TrackStuck += TrackStuckHandler;

            await state.Connection.PlayAsync(track.Track);
        }

        //> Music Error Handlers
        private async Task TrackExceptionHandler(LavalinkGuildConnection conn, TrackExceptionEventArgs args)
        {
            await StateLoader.GetState(conn.Guild).Channel
                .SendMessageAsync($"`Something bwoke while twying to pway the song UwU`\n`Exception: {args.Error}`");
            await conn.StopAsync();
        }
        private async Task TrackStuckHandler(LavalinkGuildConnection conn, TrackStuckEventArgs args)
        {
            await StateLoader.GetState(conn.Guild).Channel
                .SendMessageAsync("`Something got stwuck while twying to pway the song UwU`");
            await conn.StopAsync();
        }

        //> Music Event Handlers
        private async Task PlaybackStartedHandler(LavalinkGuildConnection conn, TrackStartEventArgs args)
        {
            var state = StateLoader.GetState(conn.Guild);
            state.Playing = true;
            if (state.LastMessage is not null)
                try { await state.LastMessage.DeleteAsync().ConfigureAwait(false); } catch { }
            var msgBuilder = new DiscordMessageBuilder()
                .WithEmbed(state.NowPlaying.VideoEmbed())
                .AddComponents(GetInteractionComponents());
            state.LastMessage = await state.Channel.SendMessageAsync(msgBuilder).ConfigureAwait(false);
        }

        private async Task PlaybackFinishedHandler(LavalinkGuildConnection conn, TrackFinishEventArgs args)
        {
            var state = StateLoader.GetState(conn.Guild);

            if (state.Loop && !state.Skipped)
            {
                await state.Connection.PlayAsync(state.NowPlaying.Track);
                return;
            }

            state.Skipped = false;
            if (state.Queue.Any())
            {
                var track = state.Queue.Pop(state.Shuffle ? Random.Next(state.Queue.Count - 1) : 0);

                if (state.LoopQueue)
                    state.Queue.Add(state.NowPlaying);

                state.NowPlaying = track;

                await state.Connection.PlayAsync(track.Track);
            }
            else
            {
                state.Playing = false;
                state.NowPlaying = null;
                state.Connection.PlaybackFinished -= PlaybackFinishedHandler;
                state.Connection.PlaybackStarted -= PlaybackStartedHandler;
                state.Connection.TrackException -= TrackExceptionHandler;
                state.Connection.TrackStuck -= TrackStuckHandler;
            }
        }

        private DiscordComponent[] GetInteractionComponents()
        {
            return new DiscordComponent[] {
                new DiscordButtonComponent(DSharpPlus.ButtonStyle.Danger, "pause", "Pause | Resume", false),
                new DiscordButtonComponent(DSharpPlus.ButtonStyle.Danger, "skip", "Skip", false),
                new DiscordButtonComponent(DSharpPlus.ButtonStyle.Danger, "loop", "Loop", false),
                new DiscordButtonComponent(DSharpPlus.ButtonStyle.Danger, "loopqueue", "Loop Queue", false),
                new DiscordButtonComponent(DSharpPlus.ButtonStyle.Danger, "shuffle", "Shuffle", false)
            };
        }
    }
}