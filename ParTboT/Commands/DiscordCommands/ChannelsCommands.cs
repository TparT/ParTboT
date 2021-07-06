using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System.Threading.Tasks;

namespace ParTboT.Commands
{
    [Group("channel")]
    public class ChannelsCommands : BaseCommandModule
    {
        [Command("edit")]
        //[Aliases("")]
        [Description("A new command")]
        public async Task Edit(CommandContext ctx, DiscordChannel Arg1, string Arg2)
        {
            await ctx.TriggerTypingAsync().ConfigureAwait(false);

            await ctx.RespondAsync("").ConfigureAwait(false);
        }
    }
}
