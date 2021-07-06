using DSharpPlus.Entities;
using EasyConsole;
using System;
using ParTboT;
using YarinGeorge.Utilities.Databases.MongoDB;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Microsoft.Extensions.Logging;
using TwitchLib.Api.Services.Events.LiveStreamMonitor;
using ParTboT.DbModels;
using Figgle;
using TwitchLib.Api;
using ParTboT.DbModels.SocialPlatforms;

namespace ParTboT.Events.Guilds.SocialPlatforms.Twitch.LiveMonitorEvents
{
    public class OnMonitorUpdate
    {
        private ServicesContainer _services { get; set; }

        public OnMonitorUpdate(ServicesContainer services)
        {
            _services = services;
        }

        public static Stopwatch UpdateTime { get; set; }

        public async void Monitor_OnStreamUpdateAsync(object sender, OnStreamUpdateArgs e)
        {
            #region Out of guild

            try
            {

                UpdateTime = Stopwatch.StartNew();
                Output.WriteLine(ConsoleColor.Blue,
                    $"\n[{DateTime.Now:T} Live monitor - Updating] Fetching streamers information and setting new channels.");

                var API = ParTboT.Bot.Services.TwitchAPI.V5;

                List<string> Channels = new List<string>();

                var Streamers
                    = _services.MongoDB.GetOnlySpecificFieldValuesAsync<TwitchStreamer>(_services.Config.LocalMongoDB_Streamers, "_id");

                foreach (var Streamer in Streamers.Result)
                {
                    Channels.Add(Streamer);
                }

                try
                {
                    ParTboT.Bot.Services.LiveMonitorService.SetChannelsById(Channels);
                }
                catch (NullReferenceException NRE)
                {
                    Console.WriteLine("No channels to add!");
                }

                ParTboT.Bot.Services.LiveMonitorService.OnStreamUpdate -= Monitor_OnStreamUpdateAsync;
                await Task.Delay(1000);
                ParTboT.Bot.Services.LiveMonitorService.OnStreamUpdate += Monitor_OnStreamUpdateAsync;
            }
            catch (Exception err)
            {
                Output.WriteLine(ConsoleColor.Red, FiggleFonts.Standard.Render($"ERROR in update"));
                Output.WriteLine(ConsoleColor.Red,
                    $"Error in {err.Source} {err.Message} - InnerException: {err.InnerException}");
            }

            #endregion


            #region From Guild members statuses

            /*
            var client = GogyBot_Alpha.Bot.Client;
            var ClientGuilds = client.Guilds.Values;

            foreach (var Guild in ClientGuilds)
            {
                //var guild = client.GetGuildAsync(client.Guild.Id).Result;
                var guildMembers = Guild.GetAllMembersAsync().Result;
                var guildRoles = Guild.Roles.Values;
                int Number = 0;
                try
                {

                    foreach (var member in guildMembers)
                    {
                        if (member.Presence.Activity.ActivityType == ActivityType.Streaming)
                        {
                            await client.SendMessageAsync(Guild.GetDefaultChannel(), $"{member.DisplayName} - {member.Presence.Activity.StreamUrl}").ConfigureAwait(false);
                            Number++;

                            foreach (DiscordRole Role in guildRoles)
                            {
                                if (Role.Name == "Currently Streaming!")
                                {
                                    StreamersRole = Guild.GetRole(Role.Id);
                                    await member.GrantRoleAsync(StreamersRole);
                                }
                            }
                            //str.AppendLine(member.Value.Presence.Activity.RichPresence.Application.Name);
                        }
                        else if (member.Presence.Activity.ActivityType != ActivityType.Streaming && member.Guild.Roles.ContainsKey(StreamersRole.Id))
                        {
                            await member.RevokeRoleAsync(StreamersRole).ConfigureAwait(false);
                        }
                    }
                }
                catch
                {
                    if (Number == 0)
                    {
                        //await ctx.RespondAsync($"There are no live streams").ConfigureAwait(false);
                    }
                }
            }

            await Task.Delay(7000);*/

            #endregion
        }
    }
}