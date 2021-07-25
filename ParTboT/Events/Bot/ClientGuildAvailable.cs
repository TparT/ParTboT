using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;
using ParTboT.DbModels.DSharpPlus;
using ParTboT.DbModels.ParTboTModels;
using ParTboT;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace ParTboT.Events.BotEvents
{
    public class ClientGuildAvailable
    {
        private static readonly EventId BotEventId;
        public DiscordMember Discordmember { get; }
        private ServicesContainer Services { get; }

        public ClientGuildAvailable(ServicesContainer services)
        {
            Services = services;
        }

        public async Task Client_GuildAvailable(DiscordClient sender, GuildCreateEventArgs e)
        {
            #region New server... yes? Then add to database!
            ParTboTGuildModel NewGuild = new()
            {
                Id = e.Guild.Id,
                Name = e.Guild.Name,
                MemberCount = e.Guild.MemberCount,
                Prefixes = Bot.DefaultPrefixes,
                SocialsFollows = new() { Facebook = new(), FloatPlane = new(), Instagram = new(), LinkedIn = new(), TwitchStreamers = new(), Twitter = new(), YouTubers = new() }
            };
            if (sender.Guilds.ContainsKey(e.Guild.Id))
            {
                //await Services.MongoDB.InsertOneRecordAsync("Guilds", NewGuild).ConfigureAwait(false);
            }

            #endregion

            #region Bot Guilds table thingy

            //var TotalMembersInAllGuilds = 0;
            //var Table = new ConsoleTable("Guild name", "Member count (With bots)", "Pure member count (W/O Bots)", "Bots count", "Guild owner", "Join date (UTC)");
            //foreach (var Guild in sender.Guilds.Values)
            //{
            //    var BotsCount = Guild.Members.Values.Where(x => x.IsBot == true).LongCount();
            //    var PureMembersCount = Guild.MemberCount - BotsCount;
            //    TotalMembersInAllGuilds += Guild.MemberCount;
            //    Table.AddRow(Guild.Name, Guild.MemberCount, PureMembersCount, BotsCount, Guild.Owner, Guild.JoinedAt.UtcDateTime);
            //}

            //sender.Logger.LogInformation($"\n{Table}");

            #endregion

            #region Channels name changing thingy OOOO

            /*var GogyBot = client.GetGuildAsync(745008583178977370).Result.GetMemberAsync(745761885164404846).Result;

            var BotGuildsCounterChannel = await client.GetChannelAsync(798162103784833034).ConfigureAwait(false);
            var TotalOverallMembersCounterChannel = await client.GetChannelAsync(798162565560926248).ConfigureAwait(false);

            var ChannelAuditLogs = await BotGuildsCounterChannel.Guild.GetAuditLogsAsync(null, GogyBot, AuditLogActionType.ChannelUpdate);

            var AuditsTable = new ConsoleTable("Guild", "By", "Catagory", "Type", "Reason", "Time", "ID");

            foreach (var ActionLog in ChannelAuditLogs)
            {
                AuditsTable.AddRow(GogyBot.Guild.Name, ActionLog.UserResponsible, ActionLog.ActionCategory, ActionLog.ActionType, ActionLog.Reason, ActionLog.CreationTimestamp, ActionLog.Id);
            }

            string AuditTableResults = AuditsTable.ToString();

            sender.Logger.LogInformation($"\n{AuditTableResults}\n");

            await TotalOverallMembersCounterChannel.ModifyAsync(async x =>
            {
                x.Name = $"👂 Listening to {TotalMembersInAllGuilds} users";
            }).ConfigureAwait(false);

            await BotGuildsCounterChannel.ModifyAsync(x =>
            {
                x.Name = $"🌐 In {NumberOfGuilds} servers";
                x.Topic = $"{BotName} is currently in {NumberOfGuilds} guilds | Listening to {TotalMembersInAllGuilds} members";
            }).ConfigureAwait(false);*/

            #endregion
        }
    }
}