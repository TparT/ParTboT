using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System.Threading.Tasks;

namespace ParTboT.Commands.TextCommands
{
    public class AdminCommands : BaseCommandModule
    {
        [Command("kick")]
        [Description("Kicks a given member")]
        [RequirePermissions(Permissions.KickMembers)]

        public async Task Kick(CommandContext ctx, [Description("The member you want to kick")] DiscordMember member, [Description("OPTIONAL: Reason for kicking the member.")][RemainingText] string Reason)
        {
            await member.RemoveAsync(Reason).ConfigureAwait(false);
        }


        //[Command("upload")]
        ////[Aliases("n")]
        //[Description("Uploads a new emote")]
        //public async Task New(CommandContext ctx, [Description("The name the emoji will have (E.g: SmileyFace . colons are not needed")] string EmojiName, [RemainingText, Description("The URL of the image of the emoji to upload. Can be empty if a **__file attachment__** is provided.")] string ImageURL = null)
        //{
        //    await ctx.TriggerTypingAsync().ConfigureAwait(false);

        //    try
        //    {
        //        Stream ImageFileStream =
        //            (await WebRequest.Create(ImageURL ?? ctx.Message.Attachments[0].Url).GetResponseAsync().ConfigureAwait(false)).GetResponseStream();

        //        DiscordGuildEmoji UploadedEmoji = await ctx.Guild.CreateEmojiAsync(EmojiName.Replace(":", string.Empty), ImageFileStream).ConfigureAwait(false);

        //        await ctx.RespondAsync($"The emoji was succesfully added, Look how cool it looks -> {UploadedEmoji}").ConfigureAwait(false);
        //    }
        //    catch (Exception)
        //    {
        //        await ctx.RespondAsync("There was a problem during this process. Please try again later.").ConfigureAwait(false);
        //    }
        //}
    }
}
