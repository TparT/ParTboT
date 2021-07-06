using DSharpPlus.CommandsNext;
using DSharpPlus.Exceptions;
using DSharpPlus.CommandsNext.Attributes;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using System;
using System.Linq;
using System.Collections.Generic;

namespace ParTboT.Commands
{
    [Group("clear")]
    [RequirePermissions(DSharpPlus.Permissions.ManageMessages)]
    public class ClearCommands : BaseCommandModule
    {
        [Command("all")]
        [Description
            (
                "Clears messages in the channel the command was executed\n\n" +
                "**__Usage:__**\n" +
                "```clear all - clears the past 100 messages that are less than 14 days old```\n"
            )
        ]
        public async Task ClearAll(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync().ConfigureAwait(false);
            DiscordMessage Respond;

            var MessageList = (await ctx.Channel.GetMessagesAsync().ConfigureAwait(false)).Where(x => (DateTime.UtcNow - x.Timestamp.UtcDateTime).TotalDays < 14).ToList();
            Console.WriteLine(MessageList.Count);
            if (MessageList.Count is not 0)
            {
                try
                {
                    int MessagesDeleted = MessageList.Count - 1;
                    string Reason = $"{MessagesDeleted} were deleted by {ctx.Member.Username}#{ctx.Member.Discriminator}";
                    if (string.IsNullOrWhiteSpace("") == false)
                    {
                        Reason += $"\n {ctx.Member.Username} also added to this log:\n{""}";
                    }
                    await ctx.Channel.DeleteMessagesAsync(MessageList, "jjjj").ConfigureAwait(false);

                    Respond = await ctx.RespondAsync($"Successfully cleared {MessagesDeleted} Messages!").ConfigureAwait(false);
                }
                catch (BadRequestException BRE)
                {

                    //Console.Clear();
                    //Console.WriteLine(BRE.JsonMessage);
                    Respond = await ctx.RespondAsync($"Failed to clear messages: {BRE.Message} - {BRE.InnerException}").ConfigureAwait(false);
                }
            }
            else
            {
                Respond = await ctx.RespondAsync($"Failed to clear messages: No messages that are less than 14 days old could be found.").ConfigureAwait(false);
            }


            await Task.Delay(2500);
            await ctx.Channel.DeleteMessageAsync(Respond).ConfigureAwait(false);
        }

        [Command("amount")]
        [Aliases("number", "num", "n")]
        [Description("Deletes a number of messages by a given amount of messages to delete\n**[NOTE: Maximum number of messages you can delete is 99 messages that are __less than 14 days old__]**")]
        public async Task ByAmount(CommandContext ctx, [Description("how many messages")] int HowMany)
        {
            await ctx.TriggerTypingAsync().ConfigureAwait(false);
            DiscordMessage Respond;

            if (HowMany <= 100)
            {
                await ctx.Channel.DeleteMessageAsync(ctx.Message).ConfigureAwait(false);
                var MessageList = (await ctx.Channel.GetMessagesAsync(HowMany).ConfigureAwait(false)).ToList();

                try
                {
                    await ctx.Channel.DeleteMessagesAsync(MessageList).ConfigureAwait(false);

                    Respond = await ctx.RespondAsync($"Successfully cleared {MessageList.Count - 1} Messages!").ConfigureAwait(false);
                }
                catch (Exception BRE)
                {
                    Respond = await ctx.RespondAsync($"Failed to clear messages: {BRE.Message} - {BRE.InnerException}").ConfigureAwait(false);
                }

                await Task.Delay(2500);
                await ctx.Channel.DeleteMessageAsync(Respond).ConfigureAwait(false);
            }
        }

        [Command("after")]
        //[Aliases("")]
        [Description("Deletes all messages that were sent AFTER a message by a given message id")]
        public async Task After(CommandContext ctx, ulong MessageID = 0)
        {
            await ctx.TriggerTypingAsync().ConfigureAwait(false);
            DiscordMessage Respond;
            IReadOnlyList<DiscordMessage> MessagesList = null;

            if (MessageID != 0)
            {
                MessagesList = await ctx.Channel.GetMessagesAfterAsync(MessageID).ConfigureAwait(false);
            }
            else if (ctx.Message.ReferencedMessage.Id != 0)
            {
                var OG_Message = ctx.Message.ReferencedMessage;
                MessagesList = await ctx.Channel.GetMessagesAfterAsync(OG_Message.Id).ConfigureAwait(false);
            }
            else
            {
                //throw new DSharpPlus.CommandsNext.Exceptions.InvalidOverloadException("", null);
            }


            try
            {
                await ctx.Channel.DeleteMessagesAsync(MessagesList.ToList()).ConfigureAwait(false);

                Respond = await ctx.RespondAsync($"Successfully cleared {MessagesList.Count - 1} Messages!").ConfigureAwait(false);
            }
            catch (Exception BRE)
            {
                Respond = await ctx.RespondAsync($"Failed to clear messages: {BRE.Message} - {BRE.InnerException}").ConfigureAwait(false);
            }

            await Task.Delay(2500);
            await ctx.Channel.DeleteMessageAsync(Respond).ConfigureAwait(false);

        }

        [Command("before")]
        //[Aliases("")]
        [Description("Deletes all messages that were sent BEFORE a message by a given message id")]
        public async Task Before(CommandContext ctx, ulong MessageID)
        {
            await ctx.TriggerTypingAsync().ConfigureAwait(false);
            DiscordMessage Respond;
            var MessageList = await ctx.Channel.GetMessagesBeforeAsync(MessageID).ConfigureAwait(false);

            try
            {
                await ctx.Channel.DeleteMessagesAsync(MessageList).ConfigureAwait(false);

                Respond = await ctx.RespondAsync($"Successfully cleared {MessageList.Count - 1} Messages!").ConfigureAwait(false);
            }
            catch (Exception BRE)
            {
                Respond = await ctx.RespondAsync($"Failed to clear messages: {BRE.Message} - {BRE.InnerException}").ConfigureAwait(false);
            }

            await Task.Delay(2500);
            await ctx.Channel.DeleteMessageAsync(Respond).ConfigureAwait(false);
        }
    }
}
