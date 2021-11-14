using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.Entities;

namespace Horizon.Interface
{
    public interface IVideoDownloader
    {
        string Name { get; set; }
        Task<List<IVideo>> GetVideos(string query, DiscordUser user);
    }
}