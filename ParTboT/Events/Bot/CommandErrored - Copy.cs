using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParTboT.Events.Bot
{
    public class CommandErrored_copy
    {

        private static readonly EventId BotEventId;

        public static async Task Command_Errored(CommandsNextExtension cnt, CommandErrorEventArgs e)
        {
            CommandContext ctx = e.Context;

            // let's log the error details
            e.Context.Client.Logger.LogError(BotEventId, $"{e.Context.User.Username} tried executing '{e.Command?.QualifiedName ?? $"{e.Context.Message.Content} (unknown command)"}' but it errored: {e.Exception.GetType()}: {e.Exception.Message ?? "<no message>"}", DateTime.Now);
            //var channelId = e.Context.Channel.Id.ToString();
            //await e.Context.RespondAsync($"```css\n[ {e.Context.User.Username} tried executing '{e.Command?.QualifiedName ?? $"{e.Context.Message.Content} (unknown command)"}' but it errored: {e.Exception.GetType()}: {e.Exception.Message ?? "<no message>"} {DateTime.Now} ]\n```").ConfigureAwait(false);

            //SpellingCorrectingService spelling = new SpellingCorrectingService();

            // let's check if the error is a result of lack
            // of required permissions

            // yes, the user lacks required permissions,
            // let them know

            var emoji = DiscordEmoji.FromName(e.Context.Client, ":no_entry:");

            // let's wrap the response into an embed
            var embed = new DiscordEmbedBuilder();

            await e.Context.RespondAsync("", embed: embed);

            switch (e.Exception)
            {
                case ChecksFailedException Checksfailedexception:
                    if (Checksfailedexception.FailedChecks.FirstOrDefault(x => x is CooldownAttribute) is CooldownAttribute Cooldownattribute)
                    {
                        TimeSpan tempo = TimeSpan.FromSeconds(Cooldownattribute.GetRemainingCooldown(ctx).TotalSeconds);
                        switch (tempo)
                        {
                            case TimeSpan n when (n.Days >= 1):
                                await ctx.Channel.SendMessageAsync($"Please wait {tempo.Days} days and {tempo.Hours} houres before using this command again! {ctx.Member.Mention}.");
                                break;
                            case TimeSpan n when (n.Hours >= 1):
                                await ctx.Channel.SendMessageAsync($"Please wait {tempo.Hours} houres and {tempo.Minutes} minutes before using this command again! {ctx.Member.Mention}.");
                                break;
                            case TimeSpan n when (n.Minutes >= 1):
                                await ctx.Channel.SendMessageAsync($"Please wait {tempo.Minutes} minutes and {tempo.Seconds} seconds before using this command again! {ctx.Member.Mention}.");
                                break;
                            default:
                                await ctx.Channel.SendMessageAsync($"{ctx.Member.Mention}, Please wait {tempo.Seconds} before using this command again!");
                                break;
                        };
                    }

                    break;
                case CommandNotFoundException Commandnotfoundexception:
                    if (e.Command?.Name == "ajuda")
                    {
                        //await ctx.Channel.SendMessageAsync($":no_entry_sign: | {ctx.User.Mention} The command {e.Context.RawArgumentString} Does.*");

                        string Customsentence_correction = string.Empty;
                        foreach (string item in e.Context.RawArguments)
                        {
                            //Customsentence_correction += spelling.Correct(item) + " ";
                        }

                        await e.Context.RespondAsync(($"{ctx.User.Mention} The command {e.Context.RawArgumentString} was not found, did you mean: `{ Customsentence_correction }` ?").ToString()).ConfigureAwait(false);
                    }

                    break;
                case NotFoundException Notfoundexception:
                    await ctx.Channel.SendMessageAsync($"{ctx.User.Mention}, The specified member was not found.");
                    break;
                case UnauthorizedException Unauthorizedexception:
                    e.Context.Client.Logger.LogDebug(new EventId(601, "insufficient permissions"), $"[{e.Context.User.Username}({e.Context.User.Id})] tried to use '{e.Command?.QualifiedName ?? "<unknown command>"}' but there was an error: {e.Exception}\ninner exception:{e.Exception?.InnerException}.", DateTime.Now);
                    break;
                case ServerErrorException Servererrorexception:
                    e.Context.Client.Logger.LogDebug(new EventId(601, "Invalid Command"), $"[{e.Context.User.Username}({e.Context.User.Id})] tried to use '{e.Command?.QualifiedName ?? "<unknown command>"}' but there was an error: {e.Exception}\ninner exception:{e.Exception?.InnerException}.", DateTime.Now);
                    break;
                case Newtonsoft.Json.JsonReaderException _:
                    e.Context.Client.Logger.LogDebug(new EventId(602, "Discord servers error"), $"[{e.Context.User.Username}({e.Context.User.Id})] tried to use '{e.Command?.QualifiedName ?? "<unknown command>"}' but discord's servers errored .", DateTime.Now);
                    break;
                default:
                    e.Context.Client.Logger.LogDebug(new EventId(601, "Command Error"), $"[{e.Context.User.Username}({e.Context.User.Id})] tried to use '{e.Command?.QualifiedName ?? "<unknown command>"}' but there was an error: {e.Exception}\ninner exception:{e.Exception?.InnerException}.", DateTime.Now);

                    var String = new StringBuilder();
                    var channel = await ctx.Client.GetChannelAsync(784445037244186734);
                    var error = e.Exception.ToString();

                    if (error.Length >= 2000)
                    {
                        String.AppendLine($"User ID: {e.Context.User.Id}");
                        String.AppendLine($"Guild ID: {e.Context.Guild.Id}");
                        String.AppendLine("Because the message is very large, a log above has been attached.");
                        String.AppendLine($"({Formatter.MaskedUrl("Error message link", e.Context.Message.JumpLink)})");

                        embed.WithAuthor($"{e.Context.User.Username}", e.Context.User.AvatarUrl, e.Context.User.AvatarUrl);
                        embed.WithTitle($"Command executed: {e.Command?.QualifiedName ?? "<unknown command>"}");
                        embed.WithTimestamp(DateTime.Now);
                        embed.WithDescription(String.ToString());

                        var stream = GenerateStreamFromString(error);
                        //await channel.SendMessageAsync("ErrorLog.txt", stream, embed: embed.Build());
                    }
                    else
                    {
                        String.AppendLine($"User ID: {e.Context.User.Id}");
                        String.AppendLine($"Guild ID: {e.Context.Guild.Id}");
                        String.AppendLine(e.Exception.ToString());
                        String.AppendLine($"({Formatter.MaskedUrl("Error message link", e.Context.Message.JumpLink)})");

                        embed.WithAuthor($"{e.Context.User.Username}", e.Context.User.AvatarUrl, e.Context.User.AvatarUrl);
                        embed.WithTitle($"Command executed: {e.Command?.QualifiedName ?? "<unknown command>"}");
                        embed.WithTimestamp(DateTime.Now);
                        embed.WithDescription(String.ToString());
                        await channel.SendMessageAsync(embed: embed.Build());
                    }
                    await ctx.Channel.SendMessageAsync("An error has occured! The issue was reported to my developer's server.");
                    break;
            }
        }

        static Stream GenerateStreamFromString(string s)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }
    }
}