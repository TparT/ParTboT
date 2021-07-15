using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ParTboT.Events.GuildEvents.Socials.Twitch.API_Services.LiveMonitor
{
    public class MemberStreaming
    {
        public static DiscordRole StreamersRole { get; set; }

        public static async Task MemberStartedStreaming(DiscordClient client, GuildMemberUpdateEventArgs guildMemberUpdate)
        {
            if (guildMemberUpdate.Member.Presence.Activity.ActivityType == ActivityType.Streaming)
            {
                var members = guildMemberUpdate.Guild.Members;
                var guild = client.GetGuildAsync(guildMemberUpdate.Guild.Id).Result;
                var guildMembers = guild.GetAllMembersAsync().Result;
                var guildRoles = guild.Roles.Values;
                int Number = 0;
                try
                {

                    foreach (var member in guildMembers)
                    {
                        if (member.Presence.Activity.ActivityType == ActivityType.Streaming)
                        {
                            await client.SendMessageAsync(guildMemberUpdate.Guild.GetDefaultChannel(), $"{member.DisplayName} - {member.Presence.Activity.StreamUrl}").ConfigureAwait(false);
                            Number++;

                            foreach (DiscordRole Role in guildRoles)
                            {
                                if (Role.Name == "Currently Streaming!")
                                {
                                    StreamersRole = guild.GetRole(Role.Id);
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
                        await client.SendMessageAsync(guildMemberUpdate.Guild.GetDefaultChannel(), $"There are no live streams").ConfigureAwait(false);
                    }
                }
            }
        }
    }
}
