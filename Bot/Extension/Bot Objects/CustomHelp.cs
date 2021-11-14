using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.CommandsNext.Entities;
using DSharpPlus.Entities;

namespace Horizon
{

    public class CustomHelpFormatter : BaseHelpFormatter
    {
        protected DiscordEmbedBuilder _embed;
        protected StringBuilder _strBuilder;

        public CustomHelpFormatter(CommandContext ctx) : base(ctx)
        {
            _embed = new DiscordEmbedBuilder().WithColor(new DiscordColor("2f3136")).WithTimestamp(DateTime.Now); ;
            _strBuilder = new StringBuilder();
        }

        public override BaseHelpFormatter WithCommand(Command command)
        {
            _embed.WithTitle($"[ **HELP** ] How to use: **{command.Name}**");
            string args = "";

            foreach (CommandArgument arg in command.Overloads[0].Arguments)
            {
                if (arg.IsOptional)
                    args += $" [{arg.Name}]";
                else
                    args += $" <{arg.Name}>";
            }

            _embed.WithDescription(command.Description);
            _embed.AddField($"Usage", $"`${command.Name}{args}`");
            return this;
        }

        public override BaseHelpFormatter WithSubcommands(IEnumerable<Command> cmds)
        {
            Dictionary<string, List<Command>> CommandsInCatagorys = new();

            foreach (var cmd in cmds)
            {
                if (cmd.Name == "help") continue;
                if (!CommandsInCatagorys.ContainsKey(cmd.Module.ModuleType.Name))
                    CommandsInCatagorys[cmd.Module.ModuleType.Name] = new();

                CommandsInCatagorys[cmd.Module.ModuleType.Name].Add(cmd);
            }

            _embed.WithTitle($"These are all the Commands ({cmds.Count()})");

            foreach (var cmd in CommandsInCatagorys)
                _embed.AddField(cmd.Key, $"• {string.Join("\n• ", cmd.Value.Select(m => $"{m.Name}"))}", true);

            return this;
        }

        public override CommandHelpMessage Build()
        {
            return new CommandHelpMessage(embed: _embed);
        }
    }

    public static class StringExtensions
    {
        public static string ToFrontUpper(this string str)
        {
            if (str.Length > 1)
                return char.ToUpper(str[0]) + str[1..];

            return str.ToUpper();
        }
    }
}
