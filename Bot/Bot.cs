using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.Lavalink;
using DSharpPlus.Net;
using DSharpPlus.SlashCommands;
using SpotifyAPI.Web;

namespace Horizon
{
    public class Bot
    {
        public static DiscordClient Client { get; private set; }
        public static CommandsNextExtension CommandsNext { get; private set; }
        public static LavalinkExtension Lavalink { get; private set; }
        public static InteractivityExtension Interactivity { get; set; }
        public static SlashCommandsExtension Slash { get; private set; }
        public static SpotifyClient Spotify { get; private set; }

        public Bot()
        {
            //> Run Lavalink Server
            RunLavalinkServer();
            Thread.Sleep(3000); // Waiting for lavalink to start

            //> Get Config From Config.toml
            var Config = new Config();

            //> Discord Client Setup
            Client = new DiscordClient(
                new DiscordConfiguration
                {
                    Token = Config.Token,
                    TokenType = TokenType.Bot,
                    Intents = DiscordIntents.All, // Presence, Member
                    AutoReconnect = true,
                    LogTimestampFormat = "HH:mm:ss",
                    MinimumLogLevel = Microsoft.Extensions.Logging.LogLevel.Debug,
                }
            );

            //> Spotify
            var config = SpotifyClientConfig.CreateDefault();
            Spotify = new SpotifyClient(config.WithToken(GetSpotifyToken(config)));

            //> Interactivity
            Interactivity = Client.UseInteractivity(
                new InteractivityConfiguration
                {
                    Timeout = TimeSpan.FromSeconds(20)
                }
            );

            //> CommandsNext
            CommandsNext = Client.UseCommandsNext(
                new CommandsNextConfiguration
                {
                    StringPrefixes = new string[] { Config.Prefix },
                    CaseSensitive = false,
                    EnableDms = false,
                    EnableMentionPrefix = false,
                    EnableDefaultHelp = true,
                    DmHelp = false,
                    IgnoreExtraArguments = true,
                }
            );

            //> Enable Lavalink and SlashCommands For DiscordClient
            Slash = Client.UseSlashCommands();
            Lavalink = Client.UseLavalink();

            //> Register Commands
            //> SlashCommands
            // Slash.RegisterCommands<Commands.Interactive>();

            //> Normal Commands
            CommandsNext.RegisterCommands<Commands.NSFW>();
            CommandsNext.RegisterCommands<Commands.Music>();
            CommandsNext.RegisterConverter(new BooleanArgsConverter());

            //> Finish Bot Setup
            CommandsNext.SetHelpFormatter<CustomHelpFormatter>();

            //> Events
            Client.Ready += async (client, e) =>
            {
                await Client.UpdateStatusAsync(new DiscordActivity("with femboy cock", ActivityType.Playing));
            };

            Client.VoiceStateUpdated += (Client, e) =>
            {
                if (e.User.Id == Client.CurrentUser.Id && e.After.Channel == null)
                    StateLoader.Remove(e.Guild.Id);
                return Task.CompletedTask;
            };

            // Skip Pause Buttons
            Client.ComponentInteractionCreated += async (Client, e) =>
            {
                await e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                if (e.Id == "skip")
                    await ExecuteCommand("skip", e.Message);
                else if (e.Id == "loop")
                    await ExecuteCommand("loop", e.Message);
                else if (e.Id == "loopqueue")
                    await ExecuteCommand("loopqueue", e.Message);
                else if (e.Id == "shuffle")
                    await ExecuteCommand("shuffle", e.Message);
                else if (e.Id == "pause")
                {
                    if (StateLoader.GetState(e.Guild).Paused)
                        await ExecuteCommand("resume", e.Message);
                    else
                        await ExecuteCommand("pause", e.Message);
                }

            };

            CommandsNext.CommandErrored += async (cnext, e) =>
                await e.Context.RespondAsync($"`{e.Exception.Message}`");
        }

        private static string GetSpotifyToken(SpotifyClientConfig config)
        {
            var request = new ClientCredentialsRequest("bea88ac2dab04f7b97965ddb7ccbe6a5", "e4631e151b6b4f228209e869c9146fe5");
            var response = new OAuthClient(config).RequestToken(request).Result;
            return response.AccessToken;
        }

        private static void RunLavalinkServer()
        {
            Thread LavalinkThread = new(() =>
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "java",
                        Arguments = "-jar Lavalink.jar",
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                process.WaitForExit();
            });
            LavalinkThread.Priority = ThreadPriority.Highest;
            LavalinkThread.IsBackground = true;
            LavalinkThread.Start();
        }

        public async Task RunAsync()
        {
            var endpoint = new ConnectionEndpoint("127.0.0.1", 2333);
            var lavalinkConfig = new LavalinkConfiguration
            {
                Password = "youshallnotpass", // From your server configuration.
                RestEndpoint = endpoint,
                SocketEndpoint = endpoint
            };

            await Client.ConnectAsync().ConfigureAwait(false);
            await Lavalink.ConnectAsync(lavalinkConfig).ConfigureAwait(false);
            await Task.Delay(-1).ConfigureAwait(false);
        }

        private static async Task ExecuteCommand(string name, DiscordMessage msg)
        {
            await Task.Run(async () =>
            {
                var cmd = CommandsNext.RegisteredCommands[name];
                var ctx = CommandsNext.CreateContext(msg, "$", cmd);
                await cmd.ExecuteAsync(ctx);
            });
        }
    }
}