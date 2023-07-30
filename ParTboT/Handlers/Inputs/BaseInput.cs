using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.SlashCommands;
using System.Threading.Tasks;

namespace ParTboT.Handlers.Inputs
{
    public abstract class BaseInput<T> where T : DiscordEventArgs
    {
        public string MsgContent { get; set; }

        public BaseInput(string msgContent)
        {
            MsgContent = msgContent;
        }

        public abstract Task<DiscordMessage> SendAsync(CommandContext commandContext, DiscordMessage msg = null);
        public abstract Task<DiscordMessage> SendAsync(InteractionContext interactionContext, bool edit = false, ulong? followupId = null);
        //public abstract Task<TResult> WaitForInputAsync<TResult>();
        public abstract Task<(TResult selectedValue, DiscordInteraction interaction)> SendAndWaitForInputAsync<TResult>(CommandContext commandContext, DiscordMessage msg = null);
        public abstract Task<(TResult selectedValue, DiscordInteraction interaction)> SendAndWaitForInputAsync<TResult>(InteractionContext interactionContext, bool edit = false, ulong? followupId = null);
    }
}
