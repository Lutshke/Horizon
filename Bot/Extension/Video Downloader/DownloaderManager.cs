using System;
using System.Collections.Generic;
using System.Linq;
using DSharpPlus.Lavalink;
using Horizon.Interface;

namespace Horizon.Downloader
{
    public class DownloaderManager
    {
        public LavalinkNodeConnection NodeConnection { get; set; } = Bot.Lavalink.ConnectedNodes.Values.First();
        public Dictionary<string, IVideoDownloader> Downloaders { get; set; }

        public DownloaderManager()
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

        public IVideoDownloader Get(string Name)
        {
            if (!Downloaders.ContainsKey(Name))
                return Downloaders["default"];
            return Downloaders[Name];
        }
    }
}