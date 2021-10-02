using DSharpPlus;
using NetMQ;
using ParTboT.DbModels.ParTboTModels;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ParTboT.Services
{
    public class RemindersService
    {
        private ServicesContainer _services { get; set; }

        public RemindersService(ServicesContainer services)
        {
            _services = services;
            Log.Information("[Reminders service] Reminders service registered!");
        }

        public async Task<NetMQTimer> StartRemindersServiceAsync(TimeSpan Interval, DiscordClient client)
        {
            NetMQTimer RemindersTimer = new(Interval);
            RemindersTimer.Elapsed += async (s, e) =>
            {
                Log.Information("[Reminders service] Scanning for reminders ...");

                List<Reminder> reminders = await _services.MongoDB.LoadAllRecordsAsync<Reminder>("Reminders").ConfigureAwait(false);
                if (reminders.Any())
                {
                    List<Reminder> Filtered = reminders.Where(x => (DateTime.UtcNow - x.StartTime).TotalSeconds > 2).ToList();
                    if (Filtered.Any())
                    {
                        Log.Information($"[Reminders service] {Filtered.Count} reminders have been triggered!");
                        Filtered.ForEach(async x =>
                        {
                            await (await client.GetChannelAsync(x.ChannelToSendTo).ConfigureAwait(false))
                            .SendMessageAsync($"Hey there {x.MemberToRemindTo.MentionString}! On the {x.RequestedAt} , you wanted me to remind you about the following:\n\n{x.Description}")
                            .ConfigureAwait(false);
                        });

                        await _services.MongoDB.DeleteManyAsync<Reminder>("Reminders", "_id", Filtered.Select(x => x.Id).ToList());
                    }
                    else
                    {
                        Log.Information($"[Reminders service] No reminders found! {reminders.Count} reminders are still in awaiting.");
                    }
                }
                else
                {
                    Log.Information($"[Reminders service] No reminders were found!");
                }
            };

            return RemindersTimer;

            //await Task.Delay(-1);
        }
    }
}
