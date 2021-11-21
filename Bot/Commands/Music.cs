using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Lavalink;
using Horizon.Downloader;
using Horizon.Extensions;
using Horizon.Interface;
using static Horizon.Commands.Playlist;
using static Horizon.Extensions.Checker; // For CheckFor.. Methods

namespace Horizon.Commands
{
    class Music : BaseCommandModule
    {
        public static LavalinkNodeConnection NodeConnection { get; set; } = Bot.Lavalink.ConnectedNodes.Values.First();
        public static DownloaderManager Manager { get; set; } = new();

        [Command, Aliases("fuckoff")]
        public async Task Leave(CommandContext ctx)
        {
            var state = StateLoader.GetState(ctx.Guild);
            CheckForBotConnection(state);
            await state.Connection.DisconnectAsync().ConfigureAwait(false);
            await ctx.RespondAsync($"Left `{ctx.Member.VoiceState.Channel.Name}`").ConfigureAwait(false);
        }

        [Command, Aliases("np")]
        public async Task Playing(CommandContext ctx)
        {
            var state = StateLoader.GetState(ctx.Guild);

            _ = state.NowPlaying ?? throw new Exception("Nothing is Playing rn (。_。)");

            var embed = state.NowPlaying.VideoEmbed()
                .AddField(
                $"Current Position I `{state.Connection.CurrentState.PlaybackPosition.ToString(@"hh\:mm\:ss")} : {state.NowPlaying.Track.Length.ToString(@"hh\:mm\:ss")}`",
                $"`{GetTimeLine(state.NowPlaying.Track.Length, state.Connection.CurrentState.PlaybackPosition, 3)}`");
            await ctx.RespondAsync(embed).ConfigureAwait(false);
        }

        [Command, Aliases("pnow")]
        public async Task PlayNow(CommandContext ctx, [RemainingText] string search)
        {
            var state = StateLoader.GetState(ctx.Guild);

            if (state.Playing)
            {
                CheckForMemberConnection(ctx.Member);
                var loadResult = await GetMediaData(search, ctx.User).ConfigureAwait(false);

                var track = loadResult.First();
                state.Queue.Insert(0, track);
                await Skip(ctx);
            }
            else
                await Play(ctx, search).ConfigureAwait(false);
        }

        [Command, Aliases("pnext")]
        public async Task PlayNext(CommandContext ctx, [RemainingText] string search)
        {
            CheckForMemberConnection(ctx.Member);
            var state = StateLoader.GetState(ctx.Guild);

            if (!state.Playing)
            {
                await Play(ctx, search);
                return;
            }

            var loadResult = await GetMediaData(search, ctx.User).ConfigureAwait(false);
            var track = loadResult.First();

            state.Queue.Insert(0, track);
            // await ctx.Message.CreateReactionAsync(DiscordEmoji.FromUnicode("✅"));
            await CreateQuickResponse(ctx, $"Playing `{track.Title}` as the next Song! φ(゜▽゜*)♪");
        }

        [Command, Aliases("i")]
        public async Task Info(CommandContext ctx)
        {
            static string FormatStr(bool inp) => inp ? "✅" : "❌";

            var state = StateLoader.GetState(ctx.Guild);

            await ctx.RespondAsync(new DiscordEmbedBuilder()
                .WithTitle("⚙ Current Settings ⚙")
                .WithTimestamp(DateTime.Now)
                .WithDescription(new StringBuilder()
                    .AppendLine($"{FormatStr(state.Loop)} | Loop")
                    .AppendLine($"{FormatStr(state.LoopQueue)} | Loop Queue")
                    .AppendLine($"{FormatStr(state.Shuffle)} | Shuffle").ToString())
                .WithColor(new DiscordColor("2f3136")))
                .ConfigureAwait(false);
        }

        [Command, Aliases("p")]
        public async Task Play(CommandContext ctx, [RemainingText] string search)
        {
            var state = StateLoader.GetState(ctx.Guild);
            var loadResult = await GetMediaData(search, ctx.User).ConfigureAwait(false);
            await HandlePlaybackStart(ctx, state, loadResult).ConfigureAwait(false);
        }

        [Command, Aliases("pl")]
        public async Task Playlist(CommandContext ctx, params string[] options)
        {
            switch (options[0].Trim().ToLower())
            {
                case "new":
                    await CreateNewPlaylist(ctx);
                    break;
                case "show":
                    if (options.Length < 2)
                        throw new Exception("No id given :(");
                    await ShowPlaylist(ctx, options[1]);
                    break;
                case "load":
                    if (options.Length < 2)
                        throw new Exception("No id given :(");
                    await LoadPlaylist(ctx, options[1]);
                    break;
                case "delete":
                    if (options.Length < 2)
                        throw new Exception("No id given :(");
                    DeletePlaylist(options[1]);
                    break;
                case "update":
                    await UpdatePlaylist(ctx, options);
                    break;
                case "list":
                    await GetUserPlaylists(ctx);
                    break;
                default:
                    break;
            }
        }

        public static async Task HandlePlaybackStart(CommandContext ctx, GuildState state, List<IVideo> result)
        {
            CheckForMemberConnection(ctx.Member);
            if (state.Connection is null)
                state.Connection = await NodeConnection.ConnectAsync(ctx.Member.VoiceState.Channel).ConfigureAwait(false);

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
                    await MusicPlayer.PlayTrack(state, state.Queue.Pop(0)).ConfigureAwait(false);
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
                await MusicPlayer.PlayTrack(state, track);
                await ctx.RespondAsync($"Now playing {track.Title}!").ConfigureAwait(false);
            }
        }

        [Command, Aliases("st")]
        public async Task SkipTo(CommandContext ctx, TimeSpan time)
        {
            CheckForMemberConnection(ctx.Member);
            var state = StateLoader.GetState(ctx.Guild);
            CheckForPlaying(state);
            await state.Connection.SeekAsync(time).ConfigureAwait(false);
        }

        [Command, Aliases("q")]
        public async Task Queue(CommandContext ctx)
        {
            GuildState state = StateLoader.GetState(ctx.Guild);

            CheckForQueueContent(state);

            DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
                .WithTitle($"Your Queue ({state.Queue.Count})")
                .WithColor(new DiscordColor("2f3136"));

            for (int i = 0; i < 9; i++)
            {
                if (i == state.Queue.Count)
                    break;

                var video = state.Queue[i];

                embed.AddField(
                    $"**{i + 1}.** {video.Title}",
                    $"**Requested by:** `{video.Requester.Username}`"
                );
            }
            await ctx.RespondAsync(embed).ConfigureAwait(false);
        }

        [Command, Aliases("qi")]
        public async Task QueueInfo(CommandContext ctx, int index)
        {
            var state = StateLoader.GetState(ctx.Guild);
            await ctx.RespondAsync(state.Queue.ElementAt(index - 1).VideoEmbed()).ConfigureAwait(false);
        }

        [Command, Aliases("cq")]
        public async Task ClearQueue(CommandContext ctx)
        {
            var state = StateLoader.GetState(ctx.Guild);
            state.Queue.Clear();
            // await ctx.Message.CreateReactionAsync(DiscordEmoji.FromUnicode("✅")).ConfigureAwait(false);
            await CreateQuickResponse(ctx, "Cleared queue! ◑﹏◐");
        }

        [Command, Aliases("l")]
        public async Task Loop(CommandContext ctx)
        {
            var state = StateLoader.GetState(ctx.Guild);

            state.LoopQueue = false;
            state.Loop = !state.Loop;

            // await ctx.Message.CreateReactionAsync(
            //     state.Loop ? DiscordEmoji.FromUnicode("✅") : DiscordEmoji.FromUnicode("❌")
            // ).ConfigureAwait(false);
            await CreateQuickResponse(ctx, state.Loop ? "Loop was turned on! (＠_＠;)" : "Loop was turned off! ಥ_ಥ");
        }

        [Command, Aliases("lq")]
        public async Task LoopQueue(CommandContext ctx)
        {
            var state = StateLoader.GetState(ctx.Guild);

            state.Loop = false;
            state.LoopQueue = !state.LoopQueue;

            // await ctx.Message.CreateReactionAsync(
            //     state.LoopQueue ? DiscordEmoji.FromUnicode("✅") : DiscordEmoji.FromUnicode("❌")
            // );
            await CreateQuickResponse(ctx, state.LoopQueue ? "Loop queue was turned on! (＠_＠;)" : "Loop queue was turned off! ಥ_ಥ");
        }

        [Command, Aliases("sh")]
        public async Task Shuffle(CommandContext ctx)
        {
            var state = StateLoader.GetState(ctx.Guild);

            state.Shuffle = !state.Shuffle;

            // await ctx.Message.CreateReactionAsync(
            //     state.Shuffle ? DiscordEmoji.FromUnicode("✅") : DiscordEmoji.FromUnicode("❌")
            // ).ConfigureAwait(false);
            await CreateQuickResponse(ctx, state.Shuffle ? "Shuffle was turned on! (＠_＠;)" : "Shuffle was turned off! ಥ_ಥ");
        }

        [Command, Aliases("s")]
        public async Task Skip(CommandContext ctx)
        {
            CheckForMemberConnection(ctx.Member);
            var state = StateLoader.GetState(ctx.Guild);
            CheckForBotConnection(state);
            if (ctx.Member.Roles.Any(role => role.Name.Equals("DJ")) || state.IsRequester(ctx.Member) || ctx.Member.Permissions.HasPermission(Permissions.Administrator))
            {
                state.Skipped = true;
                await state.Connection.StopAsync().ConfigureAwait(false);
            }
            // await ctx.Message.CreateReactionAsync(state.Skipped ? DiscordEmoji.FromUnicode("✅") : DiscordEmoji.FromUnicode("❌")).ConfigureAwait(false);
            await CreateQuickResponse(ctx, state.Skipped ? "Skipped Song ╰（‵□′）╯" : "HAHAHA you cant Skip ┗|｀O′|┛");
        }

        [Command, Aliases("pa")]
        public async Task Pause(CommandContext ctx)
        {
            CheckForMemberConnection(ctx.Member);

            var state = StateLoader.GetState(ctx.Guild);

            // Checks
            CheckForBotConnection(state);
            CheckForPlaying(state);

            state.Paused = true;
            await state.Connection.PauseAsync().ConfigureAwait(false);
            // await ctx.Message.CreateReactionAsync(DiscordEmoji.FromUnicode(ctx.Client, "✅"));
            await CreateQuickResponse(ctx, "Paused Song! (；′⌒`)");
        }

        [Command, Aliases("re")]
        public async Task Resume(CommandContext ctx)
        {
            CheckForMemberConnection(ctx.Member);

            var state = StateLoader.GetState(ctx.Guild);

            CheckForBotConnection(state);
            CheckForPlaying(state);
            CheckForNotPause(state);

            state.Paused = false;
            await state.Connection.ResumeAsync().ConfigureAwait(false);
            // await ctx.Message.CreateReactionAsync(DiscordEmoji.FromUnicode(ctx.Client, "✅"));
            await CreateQuickResponse(ctx, "Resumed Song! ＼（〇_ｏ）／");
        }

        [Command, Aliases("vol")]
        public async Task Volume(CommandContext ctx, int Volume)
        {
            CheckForMemberConnection(ctx.Member);

            var state = StateLoader.GetState(ctx.Guild);

            CheckForBotConnection(state);
            CheckForPlaying(state);

            int volume = Math.Clamp(Volume, 1, 420);
            await state.Connection.SetVolumeAsync(volume);
            await ctx.RespondAsync($"Updated Volume to `{volume}%`").ConfigureAwait(false);
        }

        // Other Util Functions
        public static async Task CreateQuickResponse(CommandContext ctx, string msg)
        {
            var message = await ctx.RespondAsync(msg);
            await Task.Delay((int)TimeSpan.FromSeconds(5).TotalMilliseconds);
            await message.DeleteAsync();
        }

        // Music Util Functions
        public static async Task<List<IVideo>> GetMediaData(string search, DiscordUser user)
        {
            var match = Regex.Match(search, @"(?:(?:[a-z]+\.)*)(\w+)\.(?:[a-z]+)");
            var host = match.Success ? match.Groups[1].Value : "default";
            return await Manager.Get(host).GetVideos(search, user).ConfigureAwait(false);
        }

        private static string GetTimeLine(TimeSpan FullTime, TimeSpan CurrentTime, int multiplier = 1)
        {
            char[] TimeLine = Enumerable.Repeat('-', 10 * multiplier).ToArray();
            TimeLine[GetCurrentTimelinePosition(FullTime, CurrentTime, multiplier)] = '※';
            return new string(TimeLine);
        }

        private static int GetCurrentTimelinePosition(TimeSpan FullTime, TimeSpan CurrentTime, int multiplier) =>
            Math.Abs(((int)(Math.Round(CurrentTime.TotalMilliseconds / FullTime.TotalMilliseconds, 1) * 10) - 1) * multiplier);
    }
}