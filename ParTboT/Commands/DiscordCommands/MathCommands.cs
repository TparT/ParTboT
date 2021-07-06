using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System;
using System.Threading.Tasks;
using org.mariuszgromada.math.mxparser;

namespace ParTboT.Commands
{
    [Group("math")]
    public class MathCommands : BaseCommandModule
    {
        [GroupCommand]
        [Description("Calculates a long expression input. Meaning you can use the math **operators symbols!** (E.g: 5 **+** 1)")]
        public async Task Math(CommandContext ctx, [RemainingText] [Description("The expression")] string expression)
        {
            Expression mp = new Expression(expression);
            await ctx.Channel.SendMessageAsync($"{mp.getExpressionString()} = {mp.calculate()}").ConfigureAwait(false);
        }


        [Command("add")]
        [Description("Adds 2 numbers")]
        public async Task Add(CommandContext ctx,
            [Description("First number")] int NumberOne,
            [Description("Second number")] int NumberTwo)
        {
            await ctx.Channel.SendMessageAsync((NumberOne + NumberTwo).ToString()).ConfigureAwait(false);
        }

        [Command("avg")]
        [Description("Calculates the average between 3 given numbers")]
        public async Task Avg(CommandContext ctx,
            [Description("First number")] double NumberOne,
            [Description("Second number")] double NumberTwo,
            [Description("Third number")] double NumberThree)

        {
            await ctx.Channel.SendMessageAsync(((NumberOne + NumberTwo + NumberThree) / 3).ToString()).ConfigureAwait(false);
        }
    }
}
