using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.Entities;

namespace Horizon
{
    public class BooleanArgsConverter : IArgumentConverter<bool>
    {
        public Task<Optional<bool>> ConvertAsync(string value, CommandContext ctx)
        {
            if (bool.TryParse(value, out var boolean))
                return Task.FromResult(Optional.FromValue(boolean));

            return value.ToLower() switch
            {
                "yes" or "y" or "t" => Task.FromResult(Optional.FromValue(true)),
                "no" or "n" or "f" => Task.FromResult(Optional.FromValue(false)),
                _ => Task.FromResult(Optional.FromNoValue<bool>()),
            };
        }
    }
}