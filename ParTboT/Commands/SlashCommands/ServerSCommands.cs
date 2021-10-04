using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using System.Threading.Tasks;

namespace ParTboT.Commands.SlashCommands
{
    [SlashCommandGroup("server", "Quickly manage and perform server related operations")]
    public class ServerSCommands : ApplicationCommandModule
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

                DiscordMember Member = await ctx.Guild.GetMemberAsync(User.Id).ConfigureAwait(false);
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
}

