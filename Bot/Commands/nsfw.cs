using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Newtonsoft.Json.Linq;

namespace Horizon.Commands
{
    class NSFW : BaseCommandModule
    {

        [Command("rule")]
        public async Task Rule(CommandContext ctx, [RemainingText] string query)
        {
            using HttpClient client = new();
            HttpResponseMessage result = await client.GetAsync($"https://rule34.xxx/index.php?page=dapi&s=post&q=index&tags={query}");
            var list = Regex.Matches(await result.Content.ReadAsStringAsync(), "sample_url=\"(.*?)\"");
            await ctx.RespondAsync(list[new Random().Next(0, list.Count)].Groups[1].ToString());
        }

        [Command("e621")]
        public async Task e621(CommandContext ctx, string query)
        {
            List<string> Links = new();

            using HttpClient client = new();
            var result = await client.SendAsync(new HttpRequestMessage
            {
                RequestUri = new Uri($"https://e621.net/posts.json?tags={query}"),
                Headers = {
                    {"user-agent", "Discord Bot/1.0 (Lutshke)"}
                }
            });

            JObject json = JObject.Parse(await result.Content.ReadAsStringAsync());

            for (int i = 0; i < json["posts"].Count(); i++)
                Links.Add((string)json["posts"][i]["file"]["url"]);

            if (!Links.Any())
            {
                await ctx.RespondAsync("Couldnt find anything sowwy UwU");
                return;
            }

            var embed = new DiscordEmbedBuilder()
                .WithImageUrl(Links[new Random().Next(Links.Count)]);

            await ctx.RespondAsync(embed.Build());
        }

        [Command("astolfo")]
        public async Task Astolfo(CommandContext ctx)
        {
            using HttpClient client = new();
            HttpResponseMessage result = await client.GetAsync($"https://rule34.xxx/index.php?page=dapi&s=post&q=index&tags=astolfo_(fate)");
            var list = Regex.Matches(await result.Content.ReadAsStringAsync(), "sample_url=\"(.*?)\"");
            await ctx.RespondAsync(list[new Random().Next(0, list.Count)].Groups[1].ToString());
        }

        [Command("pornhub")]
        public async Task Hubsearch(CommandContext ctx, [RemainingText] string search)
        {
            string url = $"https://oasis-selfbot.tk/API/pornhub/{search}";

            using HttpClient client = new();
            var response = await client.GetAsync(url);
            JObject json = JObject.Parse(await response.Content.ReadAsStringAsync());

            var result = json["videos"][new Random().Next(0, json.Count)];

            var embed = new DiscordEmbedBuilder()
                .WithTitle((string)result["title"])
                .WithDescription((string)result["url"])
                .WithColor(new DiscordColor("2f3136"));

            await ctx.RespondAsync(embed.Build());

        }

        [Command("anal")]
        public async Task Anal(CommandContext ctx)
        {
            await GetJson("https://nekos.life/api/v2/img/anal", ctx);
        }

        [Command("erofeet")]
        public async Task Erofeet(CommandContext ctx)
        {
            await GetJson("https://nekos.life/api/v2/img/erofeet", ctx);
        }

        [Command("feet")]
        public async Task Feet(CommandContext ctx)
        {

            await GetJson("https://nekos.life/api/v2/img/feet", ctx);
        }

        [Command("hentai")]
        public async Task Hentai(CommandContext ctx)
        {
            await GetJson("https://nekos.life/api/v2/img/hentai", ctx);
        }

        [Command("boobs")]
        public async Task Boobs(CommandContext ctx)
        {
            await GetJson("https://nekos.life/api/v2/img/boobs", ctx);
        }

        [Command("tits")]
        public async Task Tits(CommandContext ctx)
        {
            await GetJson("https://nekos.life/api/v2/img/tits", ctx);
        }

        [Command("blowjob")]
        public async Task BlowJob(CommandContext ctx)
        {
            await GetJson("https://nekos.life/api/v2/img/blowjob", ctx);
        }

        [Command("lwedneko")]
        public async Task lewdneko(CommandContext ctx)
        {
            await GetJson("https://nekos.life/api/v2/img/nsfw_neko_gif", ctx);
        }

        [Command("lesbian")]
        public async Task Lesbian(CommandContext ctx)
        {
            await GetJson("https://nekos.life/api/v2/img/les", ctx);
        }

        [Command("waifu")]
        public async Task Waifu(CommandContext ctx)
        {
            await GetJson("https://nekos.life/api/v2/img/waifu", ctx);
        }

        [Command("pussy")]
        public async Task Pussy(CommandContext ctx)
        {
            await GetJson("https://nekos.life/api/v2/img/pussy_jpg", ctx);
        }

        [Command("trap")]
        public async Task Trap(CommandContext ctx)
        {
            await GetJson("https://nekos.life/api/v2/img/trap", ctx);
        }

        private async Task GetJson(string url, CommandContext ctx)
        {
            using HttpClient client = new();
            var response = await client.GetAsync(url);

            string responseBody = await response.Content.ReadAsStringAsync();
            var json = JObject.Parse(responseBody);

            var embed = new DiscordEmbedBuilder()
                .WithColor(new DiscordColor("2f3136"))
                .WithImageUrl((string)json["url"]);

            await ctx.RespondAsync(embed.Build());
        }
    }
}