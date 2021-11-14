using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.Lavalink;
using DSharpPlus.Lavalink.EventArgs;
using Horizon.Extensions;
using Horizon.Interface;

namespace Horizon
{
    public static class MusicPlayer
    {
        //> Music Properties
        private static readonly Random Random = new();

        //> Music Init Handler
        public static async Task PlayTrack(GuildState state, IVideo track)
        {
            state.NowPlaying = track;

            state.Connection.PlaybackFinished += PlaybackFinishedHandler;
            state.Connection.PlaybackStarted += PlaybackStartedHandler;
            state.Connection.TrackException += TrackExceptionHandler;
            state.Connection.TrackStuck += TrackStuckHandler;

            await state.Connection.PlayAsync(track.Track);
        }

        //> Music Error Handlers
        private async static Task TrackExceptionHandler(LavalinkGuildConnection conn, TrackExceptionEventArgs args)
        {
            await StateLoader.GetState(conn.Guild).Channel
                .SendMessageAsync($"`Something bwoke while twying to pway the song UwU`\n`Exception: {args.Error}`");
            await conn.StopAsync();
        }
        private async static Task TrackStuckHandler(LavalinkGuildConnection conn, TrackStuckEventArgs args)
        {
            await StateLoader.GetState(conn.Guild).Channel
                .SendMessageAsync("`Something got stwuck while twying to pway the song UwU`");
            await conn.StopAsync();
        }

        //> Music Event Handlers
        private static async Task PlaybackStartedHandler(LavalinkGuildConnection conn, TrackStartEventArgs args)
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
        private static async Task PlaybackFinishedHandler(LavalinkGuildConnection conn, TrackFinishEventArgs args)
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

        private static DiscordComponent[] GetInteractionComponents()
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