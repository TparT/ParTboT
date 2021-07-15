using DSharpPlus;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;
using ParTboT.DbModels.ParTboTModels;
using System;
using System.Threading.Tasks;
using YarinGeorge.Databases.MongoDB;
using YarinGeorge.Utilities.Extensions;

namespace ParTboT.Events.BotEvents
{
    class ClientGuildLeft
    {
        private static readonly EventId BotEventId;
        public async static Task Client_GuildLeft(DiscordClient sender, GuildDeleteEventArgs e)
        {
            sender.Logger.LogInformation
                (
                    BotEventId,
                    $" - Left a Guild! [ Guild name: {e.Guild.Name} | Members count: {e.Guild.MemberCount} | Guild owner: {e.Guild.Owner} ]\n"
                );

            var DeletedGuild = await Bot.Commands.Services.Get<MongoCRUD>().DeleteOneRecByFieldAndValueAsync<ParTboTGuildModel>("Guilds", "_id", e.Guild.Id);
            Console.WriteLine(DeletedGuild.Name + " Was deleted from the database");

            //var client = ParTboT.Bot.Client;

            //var BotName = client.CurrentUser.Username;
            //var NumberOfGuilds = client.Guilds.Count;
            //var Guilds = client.Guilds;

            //var TotalMembersInAllGuilds = 0;

            //foreach (var Guild in Guilds)
            //{
            //    var GuildMembers = Guild.Value.Members;

            //    foreach (var Member in GuildMembers)
            //    {
            //        TotalMembersInAllGuilds++;
            //    }
            //}

            //var BotLeftChannel = await client.GetChannelAsync(797917988686659644).ConfigureAwait(false);
            //var BotGuildsCounterChannel = await client.GetChannelAsync(798162103784833034).ConfigureAwait(false);
            //var TotalOverallMembersCounterChannel = await client.GetChannelAsync(798162565560926248).ConfigureAwait(false);

            //await BotLeftChannel.SendMessageAsync($"``` - Left a Guild! [ Guild name: {e.Guild.Name} | Members count: {e.Guild.MemberCount} | Guild owner: {e.Guild.Owner} ]\n```").ConfigureAwait(false);

            //await TotalOverallMembersCounterChannel.ModifyAsync(async x =>
            //{
            //    x.Name = $"👂 Listening to {TotalMembersInAllGuilds} users";
            //}).ConfigureAwait(false);

            //await BotGuildsCounterChannel.ModifyAsync(async x =>
            //{
            //    x.Name = $"🌐 In {NumberOfGuilds} servers";
            //    x.Topic = $"{BotName} is currently in {NumberOfGuilds} guilds | Listening to {TotalMembersInAllGuilds} members";
            //}).ConfigureAwait(false);
        }
    }
}
