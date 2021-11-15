using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Lavalink;
using DSharpPlus.SlashCommands;
using Horizon.Extensions;
using Horizon.Interface;

namespace Horizon.Commands
{
    class Interactive : ApplicationCommandModule
    {

        [ContextMenu(ApplicationCommandType.MessageContextMenu, "Play this song")]
        public static async Task MessageMenu(ContextMenuContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
            Checker.CheckForMemberConnection(ctx.Member);

            string search = FilterForUrl(ctx.TargetMessage.Content) ?? FilterContent(ctx.TargetMessage.Content);
            if (string.IsNullOrEmpty(search))
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Nothing in the message matched the requirements :("));
                return;
            }

            var state = StateLoader.GetState(ctx.Guild);
            var loadResult = await Music.GetMediaData(search, ctx.User).ConfigureAwait(false);
            bool IsPlaylist = loadResult.Count > 1;
            var track = loadResult[0];

            if (IsPlaylist)
            {
                foreach (IVideo trk in loadResult)
                    state.Queue.Add(trk);
                int count = state.Playing ? loadResult.Count : loadResult.Count - 1;
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Added `{count}` tracks to the Queue!")).ConfigureAwait(false);

                if (!state.Playing)
                {
                    await MusicPlayer.PlayTrack(state, state.Queue.Pop(0));
                    await ctx.Channel.SendMessageAsync($"Now playing {track.Title}!").ConfigureAwait(false);
                }
            }
            else if (state.Playing)
            {
                state.Queue.Add(track);
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Added `{track.Title}` to the Queue!")).ConfigureAwait(false);
            }
            else
            {
                state.Channel = ctx.Channel;
                await MusicPlayer.PlayTrack(state, track);
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Now playing {track.Title}!")).ConfigureAwait(false);
            }
        }

        private static string FilterContent(string content)
        {
            if (content.StartsWith("$"))
            {
                var splittedContent = content.Split(" ");
                if (splittedContent.Length > 1)
                    return string.Join(" ", splittedContent.Skip(1));
                return null;
            }
            return content;
        }

        private static string FilterForUrl(string content)
        {
            string search = null;
            var words = content.Split(' ');
            foreach (string word in words)
            {
                using MyClient client = new();
                client.HeadOnly = true;
                try
                {
                    _ = client.DownloadString(word.Trim());
                    search = word.Trim();
                }
                catch { }
            }

            return search;
        }
    }

    class MyClient : WebClient
    {
        public bool HeadOnly { get; set; }
        protected override WebRequest GetWebRequest(Uri address)
        {
            WebRequest req = base.GetWebRequest(address);
            if (HeadOnly && req.Method == "GET")
            {
                req.Method = "HEAD";
            }
            return req;
        }
    }
}