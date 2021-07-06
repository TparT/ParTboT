using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System.Threading.Tasks;

namespace ParTboT.Commands
{
    class HelpCommands : BaseCommandModule
    {
        [Command("how")]
        [Description("Shows the user how to use a commands")]

        public async Task How(CommandContext ctx,
            [Description("gfgf")] double NumberOne)
        {
            string MemberName = (ctx.Member.Mention).ToString();
            await ctx.Channel.SendMessageAsync("Hello there " + MemberName + "!").ConfigureAwait(false);
        }
    }
}
