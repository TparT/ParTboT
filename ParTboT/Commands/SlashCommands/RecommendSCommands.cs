using DSharpPlus.SlashCommands;
using System.Threading.Tasks;
using YarinGeorge.Utilities.Extensions.DSharpPlusUtils;

namespace ParTboT.Commands.SlashCommands
{
    [SlashCommandGroup("recommend", "Gives you recommend you about stuff: Movies, Anime, Songs, TV shows")]
    public class RecommendSCommands : ApplicationCommandModule
    {
        public ServicesContainer Services { private get; set; }

        [SlashCommand("Anime", "Recommends you anime to watch that match your likings")]
        public async Task New(InteractionContext ctx)
        {
            await ctx.TriggerThinkingAsync().ConfigureAwait(false);

            //Kitsu.NET.Model
        }
    }
}
