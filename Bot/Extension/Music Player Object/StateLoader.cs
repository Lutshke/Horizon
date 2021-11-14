using System.Collections.Generic;
using DSharpPlus.Entities;

namespace Horizon
{
    public static class StateLoader
    {
        private static readonly Dictionary<ulong, GuildState> States = new();

        public static GuildState GetState(DiscordGuild guild)
        {
            if (!States.ContainsKey(guild.Id))
                States.Add(guild.Id, new GuildState());

            return States[guild.Id];
        }

        public static void Remove(ulong id) => States.Remove(id);
    }
}