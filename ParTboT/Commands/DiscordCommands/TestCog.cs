using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System.Threading.Tasks;

namespace ParTboT.Commands
{
    public class TestCog : BaseCommandModule
    {

        [Command("cog")]
        //[Aliases("n")]
        [Description("A new command")]
        public async Task New(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync().ConfigureAwait(false);

            await ctx.Channel.SendMessageAsync("Hello!").ConfigureAwait(false);
        }
    }
}
