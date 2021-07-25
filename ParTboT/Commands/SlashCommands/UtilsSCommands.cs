using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using ParTboT.Converters;
using ParTboT.DbModels.ParTboTModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using YarinGeorge.Utilities.Extensions.DSharpPlusUtils;

namespace ParTboT.Commands.SlashCommands
{
    [SlashCommandGroup("Utilities", "Useful everyday-use commands.")]
    public class UtilsSCommands : SlashCommandModule
    {
        public ServicesContainer Services { private get; set; }

        [SlashCommandGroup("Qr", "Perform QR code generation stuff i guess :)")]
        public class QrCommands : UtilsSCommands
        {
            [SlashCommand("Invite", "Creates an invite link barcode for this server.")]
            public async Task InviteQR
                (
                    InteractionContext ctx,

                    [Choice("30 Minutes", 1800)]
                    [Choice("1 Hour", 3600)]
                    [Choice("6 Hours", 21600)]
                    [Choice("12 Hours", 43200)]
                    [Choice("1 Day", 86400)]
                    [Choice("7 Days", 604800)]
                    [Choice("Never", 0)]
                    [Option("Expire_After", "Time until this invite will expire (Defaults to 1 Day).")]
                    long Time = 86400,

                    [Option("Uses", "Maximum number of uses (Defaults to no limit).")]
                    long Uses = 0,

                    [Option("Temporary_access", "Automatically kicks members who joined and dissconnected without assigning a role.")]
                    bool Temp = false
                )
            {
                await ctx.TriggerThinkingAsync().ConfigureAwait(false);

                DiscordInvite invite = await ctx.Guild.GetOrCreateInviteAsync((int)Time, (int)Uses, Temp).ConfigureAwait(false);

                MemoryStream ms = Services.BarcodeService.GenerateBarcodeImage(invite.ToString());
                ms.Position = 0;

                await ctx.EditResponseAsync
                    (new DiscordWebhookBuilder()
                        .AddFile($"Invite_{invite.Code}.png", ms)
                        .AddEmbed(new DiscordEmbedBuilder().WithThumbnail(Formatter.AttachedImageUrl($"Invite_{invite.Code}.png"))))
                    .ConfigureAwait(false);

                await ms.DisposeAsync();
            }

            [SlashCommand("Custom", "Creates a QR barcode with the given text.")]
            public async Task CustomQR(InteractionContext ctx, [Option("Text", "Custom text of the barcode (E.g: Link, Song lyrics, A bad dad joke..")] string Text)
            {
                await ctx.TriggerThinkingAsync().ConfigureAwait(false);

                MemoryStream ms = Services.BarcodeService.GenerateBarcodeImage(Text);
                ms.Position = 0;

                await ctx.EditResponseAsync
                    (new DiscordWebhookBuilder()
                        .AddFile("QRBarcode.png", ms)
                        .AddEmbed(new DiscordEmbedBuilder().WithThumbnail(Formatter.AttachedImageUrl("QRBarcode.png"))))
                    .ConfigureAwait(false);

                await ms.DisposeAsync();
            }
        }

        [SlashCommandGroup("Reminders", "You know when you need to use one")]
        public class Reminders : UtilsSCommands
        {
            [SlashCommand("remind_me", "Sets a new reminder.")]
            public async Task Remind(InteractionContext ctx, [Option("minutes", "In how many minutes to remind")] long Minutes, [Option("description", "The description of the reminder.")] string Description, [Option("remind_in_dms", "Wether to remind you here with a @mention or in DMs.")] bool RemindInDMs = true)
            {
                await ctx.TriggerThinkingAsync().ConfigureAwait(false);
                ulong ChannelToSendTo = RemindInDMs ? (await ctx.Member.CreateDmChannelAsync().ConfigureAwait(false)).Id : ctx.Channel.Id;

                DateTime now = DateTime.UtcNow;
                DateTime Later = now.AddMinutes(Minutes);

                Reminder reminder = new()
                {
                    Id = new Guid(),
                    MemberToRemindTo = ctx.Member.Optimize(),
                    GuildRequestedFrom = ctx.Guild,
                    Description = Description,
                    RequestedAt = DateTime.UtcNow,
                    StartTime = DateTime.UtcNow.AddMinutes(Minutes),
                    EndTime = DateTime.UtcNow.AddMinutes(Minutes * 2),
                    ChannelToSendTo = ChannelToSendTo
                };

                await Services.MongoDB.InsertOneRecordAsync<Reminder>("Reminders", reminder).ConfigureAwait(false);

                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($":+1:")).ConfigureAwait(false);

            }

            [SlashCommand("active_reminders", "Gets reminders")]
            public async Task Reminder(InteractionContext ctx)
            {
                await ctx.TriggerThinkingAsync().ConfigureAwait(false);

                List<Reminder> reminders = await Services.MongoDB.LoadAllRecordsAsync<Reminder>("Reminders").ConfigureAwait(false);

                var rs = reminders.Where(x => x.MemberToRemindTo.Id == ctx.Member.Id).ToArray();

                InteractivityExtension interactivity = ctx.Client.GetInteractivity();
                List<Page> pages = new();

                if (rs.Any())
                {
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($":+1:\n\n")).ConfigureAwait(false);
                    foreach (var Reminder in rs)
                        pages.Add(new Page(Reminder.Description));
                    await interactivity.SendPaginatedMessageAsync(ctx.Channel, ctx.User, pages, token: CancellationToken.None).ConfigureAwait(false);
                }
                else
                {
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"You have no reminders")).ConfigureAwait(false);
                }
            }
        }
    }
}


