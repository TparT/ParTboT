using DSharpPlus;
using DSharpPlus.EventArgs;
using DSharpPlus.Exceptions;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using DSharpPlus.Net.Models;
using System;

namespace ParTboT.Events.BotEvents
{
    class ClientNewGuildJoin
    {
        private static readonly EventId BotEventId;
        public async static Task Client_NewGuildJoin(DiscordClient sender, GuildCreateEventArgs e)
        {
            sender.Logger.LogInformation
                (
                    BotEventId,
                    $" Joined a new Guild! [ Guild name: {e.Guild.Name} | Members count: {e.Guild.MemberCount} | Guild owner: {e.Guild.Owner} ]\n"
                );

            var client = ParTboT.Bot.Client;

            var BotName = client.CurrentUser.Username;
            var NumberOfGuilds = client.Guilds.Count;
            var Guilds = client.Guilds;

            var TotalMembersInAllGuilds = 0;

            foreach (var Guild in Guilds)
            {
                var GuildMembers = Guild.Value.Members;

                foreach (var Member in GuildMembers)
                {
                    TotalMembersInAllGuilds++;
                }
            }

            try
            {
                /*var BotGuildsCounterChannel = await client.GetChannelAsync(798162103784833034).ConfigureAwait(false);
                var NewGuildsBotJoinsLogChannel = await client.GetChannelAsync(797915594900635698).ConfigureAwait(false);
                var TotalOverallMembersCounterChannel = await client.GetChannelAsync(798162565560926248).ConfigureAwait(false);

                var audittime = 

                var LastMessageInNewJoinMsgByID = NewGuildsBotJoinsLogChannel.LastMessageId;
                var LastMessageInNewJoinChannel = await NewGuildsBotJoinsLogChannel.GetMessageAsync(LastMessageInNewJoinMsgByID);
                var LastMessageInNewJoinChannelTime = LastMessageInNewJoinChannel.Timestamp.UtcDateTime;

                var TimeNow = DateTime.UtcNow;
                var DifferenceInMinutes = TimeNow.Subtract(LastMessageInNewJoinChannelTime).TotalMinutes;

                if (DifferenceInMinutes < )

                await NewGuildsBotJoinsLogChannel.SendMessageAsync
                ($" Joined a new Guild! [ Guild name: {e.Guild.Name} | Members count: {e.Guild.MemberCount} | Guild owner: {e.Guild.Owner} ]\n").ConfigureAwait(false);

                string CurrentTotalMembersChannelName = TotalOverallMembersCounterChannel.Name;
                string CurrentBotGuildsCounterChannelName = BotGuildsCounterChannel.Name;

                string NewTotalMembersChannelName = $"👂 Listening to {TotalMembersInAllGuilds} users";
                string NewBotGuildsCounterChannelName = $"🌐 In {NumberOfGuilds} servers";
                

                if (CurrentTotalMembersChannelName != NewTotalMembersChannelName)
                {
                    await TotalOverallMembersCounterChannel.ModifyAsync(async x =>
                    {
                        x.Name = NewTotalMembersChannelName;
                    }).ConfigureAwait(false);
                }
                else
                {
                    sender.Logger.LogInformation($"");
                }



                if (CurrentBotGuildsCounterChannelName != NewBotGuildsCounterChannelName)
                {
                    await BotGuildsCounterChannel.ModifyAsync(async x =>
                    {
                        x.Name = NewBotGuildsCounterChannelName;
                        x.Topic = $"{BotName} is currently in {NumberOfGuilds} guilds | Listening to {TotalMembersInAllGuilds} members";
                    }).ConfigureAwait(false);
                }
                else
                {
                    sender.Logger.LogInformation($"");
                }*/
            }
            catch (RateLimitException rle)
            {
                sender.Logger.LogError($"{rle.Message} : {rle.InnerException}");
            }
            
        }        
    }
}