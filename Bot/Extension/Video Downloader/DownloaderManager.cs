using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.Lavalink;
using Horizon.Interface;

namespace Horizon.Downloader
{
    public class DownloaderManager
    {
        public DownloaderManager(LavalinkExtension lavalink)
        {
            this.Lavalink = lavalink;
        }

        public void ReloadDownloaders()
        {
            var type = typeof(IVideoDownloader);
            Downloaders = type.Assembly.ExportedTypes
                .Where(x => type.IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract)
                .Select(x =>
                {
                    var paramlessCtor = x.GetConstructors().SingleOrDefault(c => c.GetParameters().Length == 0);
                    return paramlessCtor is not null
                        ? Activator.CreateInstance(x)
                        : Activator.CreateInstance(x, NodeConnection);
                })
                .Cast<IVideoDownloader>()
                .ToDictionary(x => x.Name, x => x);
        }

        public LavalinkExtension Lavalink { get; set; }
        public LavalinkNodeConnection NodeConnection => Lavalink.ConnectedNodes.Values.FirstOrDefault();
        public Dictionary<string, IVideoDownloader> Downloaders { get; set; }

        public async Task<List<IVideo>> GetVideosAsync(string search, DiscordUser user)
        {
            var match = Regex.Match(search, @"(?:(?:[a-z]+\.)*)(\w+)\.(?:[a-z]+)");
            var host = match.Groups.Values.Last().Value;
            return await GetDownloader(host).GetVideos(search, user).ConfigureAwait(false);
        }

        public IVideoDownloader GetDownloader(string Name)
        {
            if (!Downloaders.ContainsKey(Name))
                return Downloaders["default"];
            return Downloaders[Name];
        }
    }
}