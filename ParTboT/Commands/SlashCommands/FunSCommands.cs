using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using System.Threading.Tasks;

namespace ParTboT.Commands.SlashCommands
{
    public class FunSCommands : SlashCommandModule
    {
        [SlashCommand("hello", "Responses back with 'Hello [UserWhoExecutedTheCommand] !'.")]
        public async Task HelloCommand(InteractionContext ctx)
        {
            //Console.WriteLine($"The slash command test was executed by {ctx.Member.Username}!");
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"Hello {ctx.Member.DisplayName}!")).ConfigureAwait(false);
        }
    }
}
