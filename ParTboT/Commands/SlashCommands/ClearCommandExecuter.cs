using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ParTboT.Commands.SlashCommands
{
    public class ClearCommandExecuter
    {
        public async Task<(List<DiscordMessage> messages, bool WasValid)> Clear(InteractionContext ctx, long Amount, string Reason = "", DiscordUser User = null, DiscordUser SkipUser = null)
        {
            await ctx.Channel.TriggerTypingAsync().ConfigureAwait(false);

            var MessagesToDelete = (await ctx.Channel.GetMessagesAsync((int)Amount).ConfigureAwait(false)).Where(x => (DateTime.UtcNow - x.Timestamp.UtcDateTime).TotalDays < 14).ToList();
            await ctx.Interaction.Channel.DeleteMessagesAsync(MessagesToDelete, Reason).ConfigureAwait(false);

            return (MessagesToDelete, true);
        }

        public async Task<List<DiscordMessage>> ClearAfter(InteractionContext ctx, ulong MessageID, string Reason = "")
        {
            await ctx.Channel.TriggerTypingAsync().ConfigureAwait(false);

            var MessagesToDelete = (await ctx.Channel.GetMessagesAfterAsync(MessageID).ConfigureAwait(false)).Where(x => (DateTime.UtcNow - x.Timestamp.UtcDateTime).TotalDays < 14).ToList();
            await ctx.Interaction.Channel.DeleteMessagesAsync(MessagesToDelete, Reason).ConfigureAwait(false);

            return MessagesToDelete;
        }
    }
}
