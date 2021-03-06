using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Horizon.Downloader;
using Horizon.Extensions;
using static Horizon.Extensions.Checker; // For CheckFor.. Methods

namespace Horizon.Commands
{
    class Music : BaseCommandModule
    {
        public DownloaderManager Downloader { get; set; }
        public PlaylistManager Playlist { get; set; }
        public MusicPlayer Player { get; set; }

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
                $"Current Position I `{state.Connection.CurrentState.PlaybackPosition:hh\\:mm\\:ss} : {state.NowPlaying.Track.Length:hh\\:mm\\:ss}`",
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
                var loadResult = await Downloader.GetVideosAsync(search, ctx.User).ConfigureAwait(false);

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

            var loadResult = await Downloader.GetVideosAsync(search, ctx.User).ConfigureAwait(false);
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
            var loadResult = await Downloader.GetVideosAsync(search, ctx.User).ConfigureAwait(false);
            await Player.HandlePlaybackStart(ctx, state, loadResult).ConfigureAwait(false);
        }

        [Command, Aliases("pl")]
        public async Task playlist(CommandContext ctx, params string[] options)
        {
            switch (options[0].Trim().ToLower())
            {
                case "new":
                    await Playlist.CreateNewPlaylist(ctx);
                    break;
                case "show":
                    if (options.Length < 2)
                        throw new Exception("No id given :(");
                    await Playlist.ShowPlaylist(ctx, options[1]);
                    break;
                case "load":
                    if (options.Length < 2)
                        throw new Exception("No id given :(");
                    await Playlist.LoadPlaylist(ctx, options[1]);
                    break;
                case "delete":
                    if (options.Length < 2)
                        throw new Exception("No id given :(");
                    Playlist.DeletePlaylist(options[1]);
                    break;
                case "update":
                    await Playlist.UpdatePlaylist(ctx, options);
                    break;
                case "list":
                    await Playlist.GetUserPlaylists(ctx);
                    break;
                default:
                    break;
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
        public async Task CreateQuickResponse(CommandContext ctx, string msg)
        {
            var message = await ctx.RespondAsync(msg);
            await Task.Delay((int)TimeSpan.FromSeconds(5).TotalMilliseconds);
            await message.DeleteAsync();
        }

        private string GetTimeLine(TimeSpan FullTime, TimeSpan CurrentTime, int multiplier = 1)
        {
            char[] TimeLine = Enumerable.Repeat('-', 10 * multiplier).ToArray();
            TimeLine[GetCurrentTimelinePosition(FullTime, CurrentTime, multiplier)] = '※';
            return new string(TimeLine);
        }

        private static int GetCurrentTimelinePosition(TimeSpan FullTime, TimeSpan CurrentTime, int multiplier) =>
            Math.Abs(((int)(Math.Round(CurrentTime.TotalMilliseconds / FullTime.TotalMilliseconds, 1) * 10) - 1) * multiplier);
    }
}