using System;
using System.Linq;
using DSharpPlus.Entities;
using DSharpPlus.Lavalink;

namespace Horizon.Extensions
{
    public static class Checker
    {
        public static void CheckForMemberConnection(DiscordMember member)
        {
            if (member.VoiceState == null || member.VoiceState.Channel == null)
                throw new Exception("You are not in a voice channel.");
        }

        public static void CheckForBotConnection(GuildState state)
        {
            if (state.Connection is null)
                throw new Exception("Im not even connected :(");
        }

        public static void CheckForPlaying(GuildState state)
        {
            if (state.NowPlaying is null)
                throw new Exception("There is nothing playing.");
        }

        public static void CheckForNotPause(GuildState state)
        {
            if (!state.Paused)
                throw new Exception("Its not paused.");
        }

        public static void CheckForQueueContent(GuildState state)
        {
            if (!state.Queue.Any())
                throw new Exception("Queue is empty :)");
        }

        public static void CheckForSong(LavalinkLoadResult result)
        {
            if (result.LoadResultType.OneOf(LavalinkLoadResultType.LoadFailed, LavalinkLoadResultType.NoMatches))
                throw new Exception($"Track search failed.");
        }
    }
}