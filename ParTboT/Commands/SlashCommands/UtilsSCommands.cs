using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using ParTboT.Converters;
using ParTboT.DbModels.ParTboTModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ParTboT.Commands.SlashCommands
{
    public class UtilsSCommands : SlashCommandModule
    {
        public ServicesContainer Services { private get; set; }

        [SlashCommandGroup("Utilities", "Useful everyday-use commands.")]
        public class Utils : SlashCommandModule
        {
            [SlashCommandGroup("Qr", "Perform QR code generation stuff i guess :)")]
            public class QrCommands : SlashCommandModule
            {
                [SlashCommand("Invite", "Creates an invite barcode for this server.")]
                public async Task InviteQR(InteractionContext ctx)
                {

                }

                [SlashCommand("Custom", "Creates a QR barcode with the given text.")]
                public async Task CustomQR(InteractionContext ctx, [Option("Text", "Custom text of the barcode (E.g: Link, Song lyrics, A bad dad joke..")] string Text)
                {
                    DiscordWebhookBuilder wb = new();
                    //wb.
                }
            }

            [SlashCommandGroup("Reminders", "You know when you need to use one")]
            public class Reminders : UtilsSCommands
            {
                [SlashCommand("remind_me", "Sets a new reminder")]
                public async Task Remind(InteractionContext ctx, [Option("minutes", "In how many minutes to remind")] long Minutes, [Option("description", "The description of the reminder.")] string Description, [Option("remind_in_dms", "Wether to remind you here with a @mention or in DMs.")] bool RemindInDMs = true)
                {
                    await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource).ConfigureAwait(false);
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
                    await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource).ConfigureAwait(false);

                    List<Reminder> reminders = await Services.MongoDB.LoadAllRecordsAsync<Reminder>("Reminders").ConfigureAwait(false);

                    var rs = reminders.Where(x => x.MemberToRemindTo.Id == ctx.Member.Id).ToArray();

                    InteractivityExtension interactivity = ctx.Client.GetInteractivity();
                    List<Page> pages = new();

                    if (rs.Any())
                    {
                        await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($":+1:\n\n")).ConfigureAwait(false);
                        foreach (var Reminder in rs)
                            pages.Add(new Page(Reminder.Description));
                        await interactivity.SendPaginatedMessageAsync(ctx.Channel, ctx.User, pages).ConfigureAwait(false);
                    }
                    else
                    {
                        await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"You have no reminders")).ConfigureAwait(false);
                    }
                }
            }
        }
    }
}

