using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using Genius.Models.Response;
using Genius.Models.Song;
using IronPython.Runtime.Operations;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using YarinGeorge.Games.TicTacToe;
using YarinGeorge.Utilities.ApiExtentions.GeniusApiExtention;
using YarinGeorge.Utilities.Converters;

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
