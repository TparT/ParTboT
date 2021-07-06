using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using Microsoft.Extensions.Logging;
using ParTboT.Services;
using System;
using System.Linq;
using System.Threading.Tasks;
using YarinGeorge.Utilities.DsharpPlusUtils;
using YarinGeorge.Utilities.Extra;

namespace ParTboT.Events.Bot
{
    public class CommandErrored
    {
        private ServicesContainer _services { get; set; }
        private static EventId BotEventId { get; set; }

        public CommandErrored(ServicesContainer services)
        {
            _services = services;
        }

        public async Task Command_Errored(CommandsNextExtension sender, CommandErrorEventArgs e)
        {
            // let's log the error details
            //e.Context.Client.Logger.LogError(BotEventId, $"{e.Context.User.Username} tried executing '{e.Command?.QualifiedName ?? $"{e.Context.Message.Content} (unknown command)"}' but it errored: {e.Exception.GetType()}: {e.Exception.Message ?? "<no message>"}", DateTime.Now);
            bool SendEmbed = false;
            await DSharpPlusUtils.LogBotError(e);

            // yes, the user lacks required permissions,
            // let them know
            var emoji = DiscordEmoji.FromName(e.Context.Client, ":no_entry:");

            //Title = $"{emoji} Access denied",
            //Description = $"You do not have the permissions required to execute this command.",
            //Color = new DiscordColor(0xFF0000) // red

            // let's wrap the response into an embed
            DiscordEmbedBuilder embed = new();
            embed.Color = DiscordColor.Red;

            switch (e.Exception)
            {
                case InvalidOverloadException IOE:
                    IOE.OutputBigExceptionError();
                    break;
                case ChecksFailedException ex:
                    {
                        SendEmbed = true;
                        foreach (var FailedCheck in ex.FailedChecks)
                        {
                            switch (FailedCheck)
                            {
                                case RequireBotPermissionsAttribute RequiredBotPermissions:
                                    embed.Title = $"{emoji} {ex.Context.Client.CurrentUser.Username} doesn't have permissions";
                                    embed.Description = $"{ex.Context.Client.CurrentUser.Username} must have the '{RequiredBotPermissions.Permissions.ToPermissionString().Replace("guild", "Server")}' in order to run this command.";
                                    break;
                                case RequireUserPermissionsAttribute RequiredUserPermissions:
                                    embed.Title = $"{emoji} Access denied";
                                    embed.Description = $"You do not have the {RequiredUserPermissions.Permissions.ToPermissionString().Replace("guild", "Server")}, which is required to use this command.";
                                    break;
                                case RequirePermissionsAttribute RequirePermissions:
                                    embed.Title = $"{emoji} Access denied";
                                    embed.Description = $"{ex.Context.Client.CurrentUser.Username} AND YOU do not have the {RequirePermissions.Permissions.ToPermissionString().Replace("guild", "Server")}, which is required to use this command.";
                                    break;
                                case RequireGuildAttribute RequiredGuild:
                                    embed.Title = $"{emoji} Access denied";
                                    embed.Description = $"This command can only be executed in a server!";
                                    break;
                            }
                        }

                        break;
                    }

                case CommandNotFoundException CNFE:
                    {
                        //await e.Context.RespondAsync($"```css\n[ {e.Context.User.Username} tried executing '{e.Command?.QualifiedName ?? $"{e.Context.Message.Content} (unknown command)"}' but it errored: {e.Exception.GetType()}: {e.Exception.Message ?? "<no message>"} {DateTime.Now} ]\n```").ConfigureAwait(false);

                        string CustomSentence_Correction = string.Empty;
                        foreach (string Word in e.Context.Message.Content.Split(' '))
                            CustomSentence_Correction += _services.SpellingCorrecting.Correct(Word) + " ";

                        await e.Context.RespondAsync($"Command {CNFE.CommandName} not found, did you mean: `{ CustomSentence_Correction }` ?".ToString()).ConfigureAwait(false);
                        break;
                    }

                case RateLimitException RLE:
                    Console.WriteLine(RLE.JsonMessage);
                    break;
                case BadRequestException BRE:
                    Console.WriteLine($"Errors: {BRE.Errors}\n" +
                                      $"Web response: {BRE.WebResponse.Response}");
                    break;
                default:
                    {
                        e.Exception.OutputBigExceptionError();
                        DiscordActivity ErrorStatus = new DiscordActivity($"{(await e.Context.Client.GetCurrentApplicationAsync().ConfigureAwait(false)).Owners.FirstOrDefault().Username} getting mad about that an error has occured with the {e.Command.Name} command", ActivityType.Watching);
                        await e.Context.Client.UpdateStatusAsync(ErrorStatus).ConfigureAwait(false);
                        break;
                    }
            }

            if (SendEmbed == true)
            {
                await e.Context.RespondAsync(embed).ConfigureAwait(false);
            }
        }
    }
}
