using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ParTboT.Commands.SlashCommands
{
    public class ChannelSCommands : SlashCommandModule
    {
        [SlashCommandGroup("channel", "Manage channel settings such us: Slow-Mode, Hide/UnHide channel, Lock/Unlock channel...")]
        public class ChannelCommands : SlashCommandModule
        {
            [SlashCommandGroup("clear", "Clears messages")]
            public class ClearCommands : SlashCommandModule
            {
                ClearCommandExecuter Clear = new();

                [SlashCommand("space", "Makes a giant blank space when you need it")]
                public async Task SpaceCommand(InteractionContext ctx)
                    => await ctx.CreateResponseAsync
                    (InteractionResponseType.ChannelMessageWithSource,
                        new DiscordInteractionResponseBuilder()
                        .WithContent(Formatter.Underline("\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n")));

                [SlashCommand("amount", "Clears messages with the given amount")]
                public async Task ClearAmountCommand(InteractionContext ctx, [Option("amount", "The amount of messages to delete")] long Amount, [Option("reason", "Reason for the deletion of the messages")] string Reason = null, [Option("ofMember", "Delete messages of a specific member")] DiscordUser User = null, [Option("exceptForMember", "Skips the deletion of the messages that were sent by the given member")] DiscordUser SkipUser = null)
                {
                    var DeletedMessages = await Clear.Clear(ctx, Amount, Reason, User, SkipUser).ConfigureAwait(false);

                    if (DeletedMessages.WasValid == true)
                    {
                        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"Successfuly cleared {DeletedMessages.messages.Count} messages!")).ConfigureAwait(false);
                        await Task.Delay(2000);
                        await ctx.DeleteResponseAsync().ConfigureAwait(false);
                    }
                    else
                    {
                        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Ummm, don't know what you wanted to do there but.. why did you want to delete and skip the messages of the same member?")).ConfigureAwait(false);
                    }
                }

                [SlashCommand("after", "Clears all messages that were sent after a message with the given message id")]
                public async Task ClearAfterCommand(InteractionContext ctx, [Option("messageID", "The amount of messages to delete")] string MessageID, [Option("reason", "Reason for the deletion of the messages")] string Reason = null)
                {
                    if (ctx.Member.PermissionsIn(ctx.Interaction.Channel).HasPermission(Permissions.ManageMessages))
                    {
                        bool IsMessageIdValid = ulong.TryParse(MessageID, out ulong ParsedMessageID);

                        if (IsMessageIdValid == true)
                        {
                            var DeletedMessages = await Clear.ClearAfter(ctx, ParsedMessageID, Reason);

                            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder() { Content = $"Successfuly cleared {DeletedMessages.Count} messages!" }).ConfigureAwait(false);

                            await Task.Delay(2000);
                            await ctx.DeleteResponseAsync().ConfigureAwait(false);
                        }
                        else
                        {
                            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder() { Content = "```bash\n[ERROR]\n```" }).ConfigureAwait(false);
                        }
                    }
                    else
                    {
                        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder { Content = "You do not have a permission to delete messages in this channel" })
                                                    .ConfigureAwait(false);
                    }
                }
            }

            #region Slowmode enum
            //public enum SlowModeTime : int
            //{
            //    [ChoiceName("Off")]
            //    Off = 0,
            //    [ChoiceName("ten 10 seconds")]
            //    TenSeconds = 10,
            //    [ChoiceName("thirty 30 seconds")]
            //    ThirtySeconds = 30,
            //    [ChoiceName("two 2 minutes")]
            //    TwoMinutes = 120,
            //    [ChoiceName("five 5 minutes")]
            //    FiveMinutes = 300,
            //    [ChoiceName("fifteen 15 minutes")]
            //    FifteenMinutes = 900,
            //    [ChoiceName("thirteen 30 minutes")]
            //    ThirtyMinutes = 1800,
            //    [ChoiceName("one 1 hour")]
            //    OneHour = 3600,
            //    [ChoiceName("two 2 hours")]
            //    TwoHours = 7200,
            //    [ChoiceName("six 6 hours")]
            //    SixHours = 21600
            //}
            #endregion Slowmode enum

            [SlashCommandGroup("edit", "Edit channel settings, such as: Slowmode, name")]
            public class EditChannel : SlashCommandModule
            {
                [SlashCommand("slowmode", "Puts the channel in slow-mode with the given interval")]
                public async Task SlowModeChannel
                    (

                    InteractionContext ctx,

                    [Choice("off", "0")]
                    [Choice("10 Seconds", "10")]
                    [Choice("30 Seconds", "30")]
                    [Choice("2 Minutes", "120")]
                    [Choice("5 Minutes", "300")]
                    [Choice("15 Minutes", "900")]
                    [Choice("30 Minutes", "1800")]
                    [Choice("1 Hour", "3600")]
                    [Choice("2 Hours", "7200")]
                    [Choice("6 Hours", "21600")]
                    [Option("interval", "The interval of which members will send the messages at")]
                    string SlowModeInterval,
                    [Option("reset_After", "Reset the slowmode after the given amount of time")]
                    string ResetAfter = null

                    )
                {
                    await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource).ConfigureAwait(false);
                    await ctx.Interaction.Channel.ModifyAsync(x => x.PerUserRateLimit = int.Parse(SlowModeInterval)).ConfigureAwait(false);
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Done!")).ConfigureAwait(false);
                }
            }

            [SlashCommandGroup("misc", "Misc settings")]
            public class Misc : SlashCommandModule
            {
                [SlashCommand("lock", "Locks the channel the command is being executed at")]
                public async Task LockChannel(InteractionContext ctx, [Option("reason", "Reason for audit logs")] string Reason = null, [Option("hideChannel", "Hide the channel that is being locked? (Defaults to false)")] bool HideChannel = false)
                {
                    foreach (var Role in ctx.Guild.Roles)
                    {
                        await ctx.Interaction.Channel.AddOverwriteAsync(Role.Value, Permissions.None, Permissions.SendMessages, Reason).ConfigureAwait(false);
                        if (HideChannel == true)
                        {
                            await ctx.Interaction.Channel.AddOverwriteAsync(Role.Value, Permissions.None, Permissions.AccessChannels, Reason).ConfigureAwait(false);
                        }
                    }

                    await ctx.CreateResponseAsync
                        (InteractionResponseType.ChannelMessageWithSource,
                        new DiscordInteractionResponseBuilder().WithContent("Done!")
                        ).ConfigureAwait(false);
                }

                [SlashCommand("unlock", "Unlocks the current channel that was previously locked with the 'lock' command")]
                public async Task UnLockChannel(InteractionContext ctx, [Option("reason", "Reason for audit logs")] string Reason = null, [Option("unHideChannel", "Unhide the channel that is being locked and hidden? (Defaults to false)")] bool UnHideChannel = false)
                {
                    foreach (var Role in ctx.Guild.Roles)
                    {
                        await ctx.Interaction.Channel.AddOverwriteAsync(Role.Value, Permissions.SendMessages, Permissions.None, Reason).ConfigureAwait(false);
                        if (UnHideChannel == true)
                        {
                            await ctx.Interaction.Channel.AddOverwriteAsync(Role.Value, Permissions.AccessChannels, Permissions.None, Reason).ConfigureAwait(false);
                        }
                    }

                    await ctx.CreateResponseAsync
                        (InteractionResponseType.ChannelMessageWithSource,
                        new DiscordInteractionResponseBuilder().WithContent("Done!")
                        ).ConfigureAwait(false);
                }
            }
        }
    }
}
