using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using ParTboT.Services;
using System.Threading.Tasks;

namespace ParTboT.Commands
{
    public class FixCommand : BaseCommandModule
    {
        [Command("fix")]
        [Description("Says the correct form of a word")]
        public async Task Fix(CommandContext ctx, [Description("Word")] string Word)
        {
            SpellingCorrectingService spelling = new SpellingCorrectingService();

            // A CUSTOM sentence
            string Customsentence = Word; // sees speed instead of spelled (see notes on norvig.com)
            string Customsentence_correction = "";
            foreach (string item in Customsentence.Split(' '))
            {
                Customsentence_correction += "" + spelling.Correct(item);
            }

            await ctx.RespondAsync(($"Did you mean: `{ Customsentence_correction }` ?").ToString()).ConfigureAwait(false);
        }




    }
}
