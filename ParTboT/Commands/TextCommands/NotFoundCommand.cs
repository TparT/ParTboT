using DSharpPlus.CommandsNext;
using System.Threading.Tasks;

namespace ParTboT.Commands.TextCommands
{
    public class CommandErrorEventArgs : CommandEventArgs
    {
        private async Task OnCommandError(CommandContext ctx, CommandsNextExtension sender, CommandErrorEventArgs e)
        {
            string messagecontents = "An error has occured!";
            await ctx.RespondAsync($"{messagecontents}").ConfigureAwait(false);
        }
    }
}
