using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using Horizon.Extensions.Database;
using Horizon.Interface;

namespace Horizon.Commands
{
    class Playlist : BaseCommandModule
    {

        public static DatabaseManager Database { get; set; } = new();

        public static async Task CreateNewPlaylist(CommandContext ctx)
        {
            var playlist = new DBPlaylist();

            await ctx.Channel.SendMessageAsync("Whats the name of the playlist? ");
            var result = await ctx.Message.GetNextMessageAsync();

            if (result.TimedOut) throw new Exception("You didnt respond in time! .·´¯`(>▂<)´¯`·. ");
            playlist.Title = result.Result.Content.Trim();
            playlist.Author = ctx.User.Id;
            playlist.Tracks = new();

            await ctx.Channel.SendMessageAsync("Now paste all urls of songs you want (╹ڡ╹ )");

            bool shouldExit = false;

            while (true)
            {
                result = await result.Result.GetNextMessageAsync();

                if (result.TimedOut) break;

                foreach (var str in result.Result.Content.Split(" ").Select(m => m.Trim()))
                {
                    if (str == "exit") break; shouldExit = true;
                    if (Uri.TryCreate(str, UriKind.Absolute, out var url))
                        playlist.Tracks.Add(url.AbsoluteUri);
                }

                if (shouldExit) break;
            }

            if (playlist.Count == 0) throw new Exception("OwO no Swongs addwed to your pwaylist!");

            var guid = Database.AddPlaylist(playlist);
            if (await Database.SavePlaylist())
            {
                await ctx.RespondAsync(
                new DiscordEmbedBuilder()
                    .WithTitle("PLAYLIST CREATED FAILED")
                    .WithDescription($"Something broke :(")
                    .WithColor(new DiscordColor("2f3136")));
            }
            else
            {
                await ctx.RespondAsync(
                    new DiscordEmbedBuilder()
                        .WithTitle("PLAYLIST CREATED SUCCESSFUL")
                        .WithDescription($"Id: `{guid}`")
                        .WithColor(new DiscordColor("2f3136")));
            }

        }

        public static async Task ShowPlaylist(CommandContext ctx, string id)
        {
            var count = 0;
            var playlist = Database.GetPlaylist(id) ?? throw new Exception("No playlist found ( •̀ ω •́ )✧");
            var embed = new DiscordEmbedBuilder()
                .WithTitle($"**{playlist.Title}**")
                .WithDescription($"**ID:** `{id}`\n**Track Count:** `{playlist.Count}`")
                .WithColor(new DiscordColor("2f3136"));

            foreach (var QueryURL in playlist.Tracks)
            {
                var tracks = await Music.GetMediaData(QueryURL, ctx.User);
                foreach (var track in tracks)
                {
                    if (count > 9) break;
                    embed.AddField(track.Title, $"Uploader: `{(string.IsNullOrWhiteSpace(track.Uploader) ? "idk" : track.Uploader)}`");
                    count++;
                }
            }

            await ctx.RespondAsync(embed);
        }

        public static async Task LoadPlaylist(CommandContext ctx, string id)
        {
            var Tracks = new List<IVideo>();
            var playlist = Database.GetPlaylist(id) ?? throw new Exception("No playlist found ( •̀ ω •́ )✧");

            foreach (var QueryURL in playlist.Tracks)
                Tracks.AddRange(await Music.GetMediaData(QueryURL, ctx.User));

            await ctx.RespondAsync($"Loading Playlist `{playlist.Title}` with `{playlist.Count}` tracks! φ(゜▽゜*)♪");
            await Music.HandlePlaybackStart(ctx, StateLoader.GetState(ctx.Guild), Tracks);
        }

        public static async Task UpdatePlaylist(CommandContext ctx, params string[] options)
        {
            if (options.Length < 3)
                throw new Exception("No id given :(");

            var id = options[0].Trim();
            var playlist = Database.GetPlaylist(options[1]);
            if (playlist.Author != ctx.User.Id) throw new Exception("This is not your pwaylist nonono");
            await ctx.Channel.SendMessageAsync("what action do you want to do? ( add <url> | remove <position> )");

            while (true)
            {
                var result = await ctx.Message.GetNextMessageAsync();

                if (result.TimedOut) break;

                var args = result.Result.Content.Split(' ');
                if (args.Length < 2)
                    switch (args[0])
                    {
                        case "add":
                            if (Uri.TryCreate(options[2], UriKind.RelativeOrAbsolute, out var url))
                                playlist.Tracks.Add(url.AbsolutePath);
                            break;
                        case "remove":
                            if (int.TryParse(options[2], out var position))
                                playlist.Tracks.RemoveAt(position);
                            break;
                        default:
                            throw new Exception("Are you dumb??");
                    }
            }

            if (playlist.Count == 0) Database.RemovePlaylist(id);

            await Database.SavePlaylist();
        }

        public static async Task GetUserPlaylists(CommandContext ctx)
        {
            var count = 0;
            var playlists = Database.GetUserPlaylists(ctx.User.Id.ToString());
            var embed = new DiscordEmbedBuilder()
                .WithTitle($"{ctx.Member.Nickname}'s Playlists")
                .WithDescription($"Created Playlists: {playlists.Length}")
                .WithColor(new DiscordColor("2f3136"));

            if (playlists.Any())
            {
                foreach (var playlist in playlists)
                {
                    if (count == 9) break;
                    if (Database.TryGetPlaylist(playlist, out var ply))
                        embed.AddField($"{ply.Title}", $"Id: `{playlist}`");
                    count++;
                }
                await ctx.RespondAsync(embed).ConfigureAwait(false);
                return;
            }
            await ctx.RespondAsync("You dont have any playlists :(\n\n> Use $playlist new to create a new playlist");

        }

        public static void DeletePlaylist(string id) =>
            Database.RemovePlaylist(id);
    }
}