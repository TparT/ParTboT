using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using ParTboT.DbModels.ParTboTModels;
using System;
using System.Linq;
using System.Threading.Tasks;
using YarinGeorge.Databases.MongoDB;
using YarinGeorge.Utilities.Extensions;

namespace ParTboT.Events.BotEvents
{
    public static class ClientReady
    {
        public async static Task Client_ReadyEvent(DiscordClient client, ReadyEventArgs e)
        {
            DiscordActivity Ready = new DiscordActivity("with my new code", ActivityType.Playing);
            await client.UpdateStatusAsync(Ready, UserStatus.Online, DateTimeOffset.Now.Subtract(TimeSpan.FromDays(1)));
            try
            {
                (await Bot.Commands.Services.Get<MongoCRUD>().LoadManyByFieldsValuesAndFieldsAsync<ParTboTGuildModel>("Guilds", "_id", client.Guilds.Keys)).ToList().ForEach
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