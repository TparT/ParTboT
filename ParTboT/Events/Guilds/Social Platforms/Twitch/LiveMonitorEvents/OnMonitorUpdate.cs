﻿using EasyConsole;
using Figgle;
using MongoDB.Driver;
using ParTboT.DbModels.SocialPlatforms;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using TwitchLib.Api.Services.Events.LiveStreamMonitor;

namespace ParTboT.Events.GuildEvents.SocialPlatforms.Twitch.LiveMonitorEvents
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

                //var API = _services.TwitchAPI.V5;

                var Streamers =
                    (_services.MongoDB.GetCollectionAsync<TwitchStreamer>(_services.Config.LocalMongoDB_Streamers).GetAwaiter().GetResult())
                    .AsQueryable().Select(f => f._id).Distinct().ToList();

                try
                {
                    _services.LiveMonitorService.SetChannelsById(Streamers);
                }
                catch (NullReferenceException NRE)
                {
                    Console.WriteLine("No channels to add!");
                }

                _services.LiveMonitorService.OnStreamUpdate -= Monitor_OnStreamUpdateAsync;
                await Task.Delay(1000);
                _services.LiveMonitorService.OnStreamUpdate += Monitor_OnStreamUpdateAsync;
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