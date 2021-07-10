using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Exceptions;
using DSharpPlus.Interactivity;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace YarinGeorge.Utilities.Extensions.DSharpPlusUtils
{
    /// <summary>
    /// Just some more extension methods made by yours truly ~Yarin George.
    /// <br />
    /// <br />
    /// PLEASE DONT SUE ME EMZI...
    /// <br />
    /// <br />
    /// ⠟⠛⣉⣡⣴⣶⣶⣶⣶⣶⣶⣤⣉⡛⢿⣿⣿⠿⠟⠛⣋⣉⣩⣭⣭⣭⣉⣙⠛⠈
    /// <br />
    /// ⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣦⠡⣴⣶⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿
    /// <br />
    /// ⣿⣿⣿⣿⣿⣿⣿⠁⠆⠄⠈⢻⣿⣿⣿⠄⣿⣿⣿⣿⣿⣿⣿⣿⣿⠋⠰⠄⠙⣿
    /// <br />
    /// ⣿⣿⣿⣿⣿⣿⣿⣔⡗⠠⢀⣼⣿⣿⣿⢀⣿⣿⣿⣿⣿⣿⣿⣿⣿⣀⠘⠠⢀⣼
    /// <br />
    /// ⡉⠛⠿⣿⣿⣿⣿⣿⣿⣿⣿⡿⠟⣋⣡⡈⠛⠛⠛⠿⠿⠿⠿⠿⠿⠿⠿⠿⠿⠿
    /// <br />
    /// ⠿⠷⠶⣦⣭⣉⣉⣉⣉⣭⡥⣴⡿⠿⢟⣠⣿⣿⣿⣷⣶⣶⣶⣶⣶⣶⣶⣶⣶⣶
    /// <br />
    /// ⣿⣷⣶⣶⣤⣬⣭⣽⣿⣿⠖⣠⣶⣾⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⡿⠛⠁
    /// <br />
    /// ⣿⣿⣿⣿⡿⠿⠛⣫⣥⣴⣾⣿⣿⣿⣿⣿⣷⣤⣝⠛⢛⣫⣭⣭⣭⣭⣅⠄⠄⠄
    /// <br />
    /// ⣿⣿⣿⣿⣶⣾⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣷⣼⣿⣿⣿⣿⣿⣿⣷⡀⠄
    /// <br />
    /// ⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⡄
    /// <br />
    /// ⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿
    /// <br />
    /// ⣶⣶⣶⣮⣭⣉⣙⡛⠿⠿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⠿⠟⢛⣉⣭
    /// <br />
    /// ⣛⣛⣛⡛⠻⠿⢿⣿⣿⣶⣶⣶⣶⣦⣤⣬⣭⣭⣭⣭⣭⣭⣭⣭⣴⣾⣿⣿⣿⡿
    /// <br />
    /// ⢿⣿⣿⣿⣿⣷⣶⣦⣭⣭⣭⣭⣍⣉⣉⣉⣛⣛⠛⠛⠛⠛⠛⠛⠛⢛⣋⣭⣄⠄
    /// <br />
    /// ⣶⣦⣬⣍⣙⣛⠛⠛⠛⠿⠿⠿⠿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⡿⠋⠄
    /// </summary>
    public static class DSharpPlusUtils
    {
        /// <summary>
        /// God knows why do I need this ¯\_(ツ)_/¯
        /// </summary>
        private static EventId EventId;

        /// <summary>
        /// Gets the client which received this <see cref="SnowflakeObject"/> [Credit goes to Velvet/Velvet#0069
        /// </summary>
        /// <param name="snowflake"></param>
        /// <returns></returns>
        public static DiscordClient GetClient(this SnowflakeObject snowflake)
        {
            var type = snowflake.GetType() == typeof(SnowflakeObject) ? snowflake.GetType() : snowflake.GetType().BaseType!;

            var client = type
                .GetProperty("Discord", BindingFlags.NonPublic | BindingFlags.Instance)!
                .GetValue(snowflake) as DiscordClient;

            return client!;
        }

        /// <summary>
        /// Attempts to retrieve the DiscordMember from cache, then the API if the cache does not have the member.
        /// </summary>
        /// <param name="discordGuild">The guild to get the DiscordMember from.</param>
        /// <param name="discordUser">The user to search for in the DiscordGuild.</param>
        /// <returns>The DiscordMember from the DiscordGuild</returns>
        public static async Task<(bool IsSuccess, DiscordMember Member)> GetMemberAsync(this DiscordUser discordUser, DiscordGuild discordGuild)
        {
            try
            {
                return (true, discordGuild.Members.Values.FirstOrDefault(member => member.Id == discordUser.Id) ?? await discordGuild.GetMemberAsync(discordUser.Id));
            }
            catch (NotFoundException)
            {
                return (false, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Converts the files in the <see cref="IReadOnlyCollection{DiscordMessageFile}"></see> to a <see cref="Dictionary{TKey, TValue}"/>.
        /// </summary>
        /// <param name="Files">The collection of files this message contains.</param>
        /// <returns>A dictionary of the files this message contains. The keys represent file names and values represent the files stream data.</returns>
        public static Dictionary<string, Stream> ToFilesDictionay(this IReadOnlyCollection<DiscordMessageFile> Files)
        {
            Dictionary<string, Stream> FilesDict = new Dictionary<string, Stream>();
            foreach (var File in Files)
                FilesDict.Add(File.FileName, File.Stream);

            return FilesDict;
        }

        /// <summary>
        /// Converts this <see cref="DiscordMessageBuilder"/> to a <see cref="DiscordInteractionResponseBuilder"/>.
        /// </summary>
        /// <param name="builder">The <see cref="DiscordMessageBuilder"/> to base the <see cref="DiscordInteractionResponseBuilder"/> from.</param>
        /// <returns>A <see cref="DiscordInteractionResponseBuilder"/> that it's parameters are based off of the provided <paramref name="builder"/>.</returns>
        public static DiscordInteractionResponseBuilder ToResponseBuilder(this DiscordMessageBuilder builder)
            => new DiscordInteractionResponseBuilder(builder);

        /// <summary>
        /// Converts this <see cref="DiscordMessageBuilder"/> to a <see cref="DiscordWebhookBuilder"/>.
        /// </summary>
        /// <param name="builder">The <see cref="DiscordMessageBuilder"/> to base the <see cref="DiscordWebhookBuilder"/> from.</param>
        /// <returns>A <see cref="DiscordWebhookBuilder"/> that it's parameters are based off of the provided <paramref name="builder"/>.</returns>
        public static DiscordWebhookBuilder ToWebhookBuilder(this DiscordMessageBuilder builder)
        {
            return new DiscordWebhookBuilder()
            .AddComponents(builder.Components.ElementAtOrDefault(0).Components).AddComponents(builder.Components.ElementAtOrDefault(1).Components).AddComponents(builder.Components.ElementAtOrDefault(2).Components).AddComponents(builder.Components.ElementAtOrDefault(3).Components).AddComponents(builder.Components.ElementAtOrDefault(4).Components)
            .AddEmbeds(builder.Embeds)
            .WithContent(builder.Content)
            .WithTTS(builder.IsTTS);
            //.AddFiles(builder.Files.ToFilesDisctionay());
        }

        /// <summary>
        /// Disables the abillity of a user to invoke any interaction by setting all the interactions to <c>"Disabled = true"</c>.
        /// </summary>
        /// <param name="msg">The <see cref="DiscordMessage"/> to edit it's components.</param>
        /// <param name="builder">The <see cref="DiscordMessageBuilder"/> to get the components to disable from.</param>
        /// <returns>The edited <see cref="DiscordMessage"/> after it's components were disabled.</returns>
        public static async Task<DiscordMessage> LockAllMessageComponentsAsync(this DiscordMessage msg, DiscordMessageBuilder builder)
        {
            if (builder.Components.Any())
            {
                DiscordMessageBuilder Response = new DiscordMessageBuilder();
                foreach (var ActionRow in builder.Components)
                {
                    List<DiscordComponent> components = new List<DiscordComponent>();
                    foreach (var Component in ActionRow.Components!)
                    {
                        if (Component is DiscordButtonComponent button)
                        {
                            button!.Disabled = true;
                            components.Add(button!);
                        }
                    }
                    Response.AddComponents(components!);
                }
                return await msg.ModifyAsync(Response.WithContent(builder.Content!).AddEmbeds(builder.Embeds!)).ConfigureAwait(false);
            }
            else
            {
                throw new ArgumentException("Message doesn't contain any components!");
            }
        }

        /// <summary>
        /// Handles <see cref="InteractivityResult{T}"/> time-outs
        /// </summary>
        /// <typeparam name="T">Type of <see cref="InteractivityResult{T}"/>.</typeparam>
        /// <param name="result">The result of this interactivity.</param>
        /// <param name="msg">The message to handle the interactivity time-outs for.</param>
        /// <param name="builder">If this is an interaction of waiting for a <see cref="DiscordButtonComponent"/>, this provides a way to lock the button/s so users won't be able to still use it.</param>
        /// <returns>The interactivity result if wasn't timed-out. If was, returns <see cref="null"/>.</returns>
        public static async Task<InteractivityResult<T>?> HandleTimeouts<T>(this InteractivityResult<T> result, DiscordMessage msg, DiscordMessageBuilder builder = null)
        {
            if (result.TimedOut)
            {
                //DiscordMessageBuilder Response = new DiscordMessageBuilder();
                msg = await msg.Channel.GetMessageAsync(msg.Id).ConfigureAwait(false);
                if (typeof(T) == typeof(ComponentInteractionCreateEventArgs))
                    msg = await msg.LockAllMessageComponentsAsync(builder).ConfigureAwait(false);
                else if (typeof(T) == typeof(InteractionCreateEventArgs))
                    await msg.DeleteAllReactionsAsync().ConfigureAwait(false);

                await msg.RespondAsync(":alarm_clock: This intercativity timed out.").ConfigureAwait(false);
                return null;
            }
            else
            {
                return result;
            }
        }

        /// <summary>
        /// Attempts to get the message of an invoked slash command interaction response.
        /// </summary>
        /// <param name="ctx">The command's context.</param>
        /// <returns>The message this slash command response is attached to.</returns>
        public static async Task<DiscordMessage> GetSlashCMessage(this InteractionContext ctx)
            => await ctx.Channel.GetMessageAsync(ctx.Interaction.Data.Id).ConfigureAwait(false);

        /// <summary>
        /// Similar to <seealso cref="CommandContext.TriggerTypingAsync"/> but for slash commands <see cref="InteractionContext"/> :D
        /// <br />
        /// which is literally just:
        /// <br />
        /// <br />
        /// <code>
        /// await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource).ConfigureAwait(false);
        /// </code>
        /// </summary>
        /// <param name="ctx">The command's context.</param>
        /// <returns></returns>
        public static async Task TriggerThinkingAsync(this InteractionContext ctx)
            => await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource).ConfigureAwait(false);

        /// <summary>
        /// Retrieves only non-null command data options.
        /// </summary>
        /// <param name="RawOptions">The non-null parameters from this slash command intercaction.</param>
        /// <returns></returns>
        public static IEnumerable<DiscordInteractionDataOption> PurifyOptions(this IEnumerable<DiscordInteractionDataOption> RawOptions)
            => RawOptions.Where(x => x.Type != (ApplicationCommandOptionType.SubCommand | ApplicationCommandOptionType.SubCommandGroup)).FirstOrDefault().Options;

        /// <summary>
        /// Gets all the specified <see cref="DiscordEmoji"/>es by their unicodes.
        /// </summary>
        /// <param name="client">If being invoked in a command context type of method, just do ctx.Client :)</param>
        /// <param name="EmojisUnicodes">The <see cref="DiscordEmoji"/>es unicodes.</param>
        /// <returns>The requested <see cref="DiscordEmoji"/>es from their unicodes.</returns>
        public static IEnumerable<DiscordEmoji> GetDiscordEmojisByUnicodes(this DiscordClient client, params string[] EmojisUnicodes)
        {
            List<DiscordEmoji> emotes = new List<DiscordEmoji>();

            foreach (string EmoteUnicode in EmojisUnicodes)
                emotes.Add(DiscordEmoji.FromUnicode(client, EmoteUnicode));

            return emotes;
        }

        /// <summary>
        /// Gets all the specified <see cref="DiscordEmoji"/>es by their names.
        /// </summary>
        /// <param name="client">If being invoked in a command context type of method, just do ctx.Client :)</param>
        /// <param name="EmojisNames">The <see cref="DiscordEmoji"/>es names.</param>
        /// <returns>The requested <see cref="DiscordEmoji"/>es from their names.</returns>
        public static IEnumerable<DiscordEmoji> GetDiscordEmojisByNames(this DiscordClient client, params string[] EmojisNames)
        {
            List<DiscordEmoji> emotes = new List<DiscordEmoji>();

            foreach (string EmoteName in EmojisNames)
                emotes.Add(DiscordEmoji.FromName(client, EmoteName));

            return emotes;
        }

        /// <summary>
        /// Adds multiple reactions to a <see cref="DiscordMessage"/>.
        /// </summary>
        /// <param name="msg">The <see cref="DiscordMessage"/> to react on.</param>
        /// <param name="emojis">The <see cref="DiscordEmoji"/>es to react with on the <see cref="DiscordMessage"/>.</param>
        /// <returns>The message reacted on (Just incase you wanted to do some chaining xD)</returns>
        public static async Task<DiscordMessage> AddReactionsAsync(this DiscordMessage msg, IEnumerable<DiscordEmoji> emojis)
        {
            foreach (DiscordEmoji emoji in emojis)
                await msg.CreateReactionAsync(emoji).ConfigureAwait(false);

            return msg;
        }

        /// <summary>
        /// Adds multiple reactions to a <see cref="DiscordMessage"/>.
        /// </summary>
        /// <param name="msg">The <see cref="DiscordMessage"/> to react on.</param>
        /// <param name="emojis">The <see cref="DiscordEmoji"/>es to react with on the <see cref="DiscordMessage"/>.</param>
        /// <returns>The message reacted on (Just incase you wanted to do some chaining xD)</returns>
        public static async Task<DiscordMessage> AddReactionsAsync(this DiscordMessage msg, params DiscordEmoji[] emojis)
        {
            foreach (DiscordEmoji emoji in emojis)
                await msg.CreateReactionAsync(emoji).ConfigureAwait(false);

            return msg;
        }

        /// <summary>
        /// Logs an error that had occured while a user tried to invoke a command.
        /// </summary>
        /// <param name="errorArgs">Error event arguments.</param>
        public static void LogBotError(CommandErrorEventArgs errorArgs)
        {
            errorArgs.Context.Client.Logger.LogError
            (EventId,
                $"{errorArgs.Context.User.Username} tried executing '{errorArgs.Command?.QualifiedName ?? $"{errorArgs.Context.Message.Content} (unknown command)"}' " +
                $"but it errored: {errorArgs.Exception.GetType()}: {errorArgs.Exception.Message ?? "<no message>"}",
                DateTime.Now
            );
        }

        /// <summary>
        /// Logs an error that had occured while a user tried to invoke a command.
        /// </summary>
        /// <param name="ctx">The context of the command that had errored.</param>
        /// <param name="exception">The <paramref name="exception"/> that occured during the command's execution</param>
        public static void LogBotError(this CommandContext ctx, Exception exception)
            => ctx.Client.Logger.LogError(EventId, $"{ctx.User.Username} tried executing '{ctx.Command?.QualifiedName ?? $"{ctx.Message.Content} (unknown command)"}' but it errored: {exception.GetType()}: {exception.Message ?? "<no message>"}", DateTime.Now);

        /// <summary>
        /// Logs an error that had occured while a user tried to invoke a command.
        /// </summary>
        /// <param name="ctx">The context of the command that had errored.</param>
        /// <param name="exception">The <paramref name="exception"/> that occured during the command's execution</param>
        public static void LogBotError(this InteractionContext ctx, Exception exception)
            => ctx.Client.Logger.LogError(EventId, $"{ctx.User.Username} tried executing '{ctx.CommandName}' but it errored: {exception.GetType()}: {exception.Message ?? "<no message>"}", DateTime.Now);
    }
}