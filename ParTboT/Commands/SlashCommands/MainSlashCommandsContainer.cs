using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ParTboT.Commands.SlashCommands
{

    public class MainSlashCommandsContainer : ApplicationCommandModule
    {
        [SlashCommandGroup("server", "Quickly manage and perform server related operations")]
        public class ServerCommands
        {
            [SlashCommandGroup("member", "Manage a member the server and take actions such as: Kick/Ban/Mute/Deafen")]
            public class MemberCommands
            {
                [SlashCommand("role", "Manage a role of a member")]
                public async Task ManageRole
                    (

                    InteractionContext ctx,

                    [Option("memberManage", "The member to manage their role")]
                    DiscordUser User,

                    [Choice("Grant", "Grant")]
                    [Choice("Revoke", "Revoke")]
                    [Option("action", "The action to take on the role")]
                    string Action,
                    [Option("role", "The role to grant to the member")]
                    DiscordRole Role,

                    [Option("reason", "Reason the role was given to the user")]
                    string Reason = null

                    )
                {
                    await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource).ConfigureAwait(false);

                    var Member = await ctx.Guild.GetMemberAsync(User.Id).ConfigureAwait(false);
                    string Response;
                    switch (Action)
                    {
                        case "Grant": { await Member.GrantRoleAsync(Role, Reason).ConfigureAwait(false); Response = $"granted role '{Role.Name}' to {Member.DisplayName}"; } break;
                        case "Revoke": { await Member.RevokeRoleAsync(Role, Reason).ConfigureAwait(false); Response = $"revoked role '{Role.Name}' from {Member.DisplayName}"; } break;
                        default: { Response = "HOWWW"; } break;
                    }

                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Successfully {Response}")).ConfigureAwait(false);
                }
            }

        }

        //[SlashCommand("hello", "Responses back with 'Hello [UserWhoExecutedTheCommand] !'.")]
        //public async Task HelloCommand(InteractionContext ctx)
        //{
        //    //Console.WriteLine($"The slash command test was executed by {ctx.Member.Username}!");
        //    //await ctx.CreateResponseAsync(DiscordInteractionResponseType.DeferredChannelMessageWithSource, new DSharpPlus.SlashCommands.Entities.DiscordInteractionResponseBuilder { Content = $"Hello {ctx.Member.Username}" }).ConfigureAwait(false);

        //    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder { Content = $"?????hjthere {ctx.Member.Mention}!" }).ConfigureAwait(false);
        //}

        [SlashCommand("stopslash", "Stops the process of the slash commands")]
        public async Task StopSlash(InteractionContext ctx)
        {

            if (ctx.Client.CurrentApplication.Owners.Select(x => x.Id).Contains(ctx.Member.Id))
            {
                await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder { Content = $"Bye!" });
                Environment.Exit(69);
            }
            else
            {
                await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder { Content = "You are not allowed to do that!" });
            }
        }

        //await ctx.CreateResponseAsync(DiscordInteractionResponseType.DeferredChannelMessageWithSource, new DSharpPlus.SlashCommands.Entities.DiscordInteractionResponseBuilder { Content = Platform

    }

}
