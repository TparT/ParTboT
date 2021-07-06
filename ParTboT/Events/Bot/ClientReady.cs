using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using ParTboT.DbModels.ParTboTModels;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ParTboT.Events.Bot
{
    public static class ClientReady
    {
        public async static Task Client_ReadyEvent(DiscordClient client, ReadyEventArgs e)
        {
            DiscordActivity Ready = new DiscordActivity("with my new code", ActivityType.Playing);
            await client.UpdateStatusAsync(Ready, UserStatus.Online, DateTimeOffset.Now.Subtract(TimeSpan.FromDays(1)));

            try
            {
                (await ParTboT.Bot.Services.MongoDB.LoadManyByFieldsValuesAndFieldsAsync<ParTboTGuildModel>("Guilds", "_id", client.Guilds.Keys)).ToList().ForEach
                    (Guild =>
                    {
                        Console.WriteLine($"Guild {Guild.Name} has {Guild.GuildBackups.Values.Count} backups");
                    });
            }
            catch
            {
                //Console.WriteLine("Caught! Don't worry!");
            }


        }
    }
}