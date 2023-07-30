using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System.Threading.Tasks;

namespace ParTboT.Commands.TextCommands
{
    public class HelpCommands : BaseCommandModule
    {
        [Command("how")]
        [Description("Shows you how to use a command")]
        public async Task How(CommandContext ctx, [Description("A number")] string commandName)
        {
            await ctx.RespondAsync($"Hello there, {ctx.Member.Nickname}! The {commandName} is used like so: ..... ").ConfigureAwait(false);
        }
    }
}
