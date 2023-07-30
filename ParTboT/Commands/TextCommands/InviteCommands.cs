using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using YarinGeorge.Utilities.Extensions.DSharpPlusUtils;

namespace ParTboT.Commands.TextCommands
{
    [Group("invite")]
    [Description("Invite people to this server using these commands!")]
    public class InviteCommands : BaseCommandModule
    {
        public ServicesContainer Services { private get; set; }

        [Command("inviteqr")]
        [Aliases("qr")]
        [Description("Generates a QR code containing this server's invite link.")]
        public async Task InviteQR
            (CommandContext ctx,
            [Description("Time until this invite will expire (Defaults to 1 Day).")] int TimeInSecs = 86400,
            [Description("Maximum number of uses (Defaults to no limit).")] int Uses = 0,
            [Description("Automatically kicks members who joined and dissconnected without assigning a role.")] bool Temp = false)
        {
            await ctx.TriggerTypingAsync().ConfigureAwait(false);

            DiscordInvite invite = await ctx.Guild.GetOrCreateInviteAsync(TimeInSecs, Uses, Temp).ConfigureAwait(false);

            MemoryStream ms = Services.BarcodeService.GenerateBarcodeImage(invite.ToString());
            ms.Position = 0;

            await ctx.RespondAsync
                (new DiscordMessageBuilder()
                    .AddFile($"Invite_{invite.Code}.png", ms)
                    .AddEmbed(new DiscordEmbedBuilder().WithThumbnail(Formatter.AttachedImageUrl($"Invite_{invite.Code}.png"))))
                .ConfigureAwait(false);

            await ms.DisposeAsync();
        }
    }

    class subcommand : InviteCommands
    {
        [Command("subc")]
        [Description("Says the name of the bot's developer")]
        public async Task SubC(CommandContext ctx)
        {
            await ctx.RespondAsync("Im a sub sub command").ConfigureAwait(false);
        }
    }
}
