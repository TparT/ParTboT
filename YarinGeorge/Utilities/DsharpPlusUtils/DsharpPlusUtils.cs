using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.Logging;
using ParTboT.DbModels.DSharpPlus;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using YarinGeorge.Utilities.Extra;
using YarinGeorge.Utilities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using System.Reflection;
using Emzi0767.Utilities;
using System.IO;

namespace YarinGeorge.Utilities.DsharpPlusUtils
{

    public class PagedMessage
    {
        public int CurrentPage { get; set; } = 1;
        public ConcurrentDictionary<int, DiscordMessageBuilder> MessagePages { get; set; } = new();

        public PagedMessage() { }

        public PagedMessage(DiscordMessage CmdMessage)
        {
            MessagePages.TryAdd(CurrentPage, new DiscordMessageBuilder()
                .AddComponents(CmdMessage.Components.FirstOrDefault()).WithEmbed(CmdMessage.Embeds[0]).WithContent(CmdMessage.Content));
        }


        public DiscordMessageBuilder NextPage()
        {
            CurrentPage++;
            return MessagePages.GetValueOrDefault(CurrentPage);
        }

        public DiscordMessageBuilder PreviousPage()
        {
            CurrentPage--;
            return MessagePages.GetValueOrDefault(CurrentPage);
        }
    }

    public static class DSharpPlusUtils
    {

        private static EventId EventId;

        public static DiscordClient GetClient(this SnowflakeObject snowflake)
        {
            var type = snowflake.GetType() == typeof(SnowflakeObject) ? snowflake.GetType() : snowflake.GetType().BaseType!;

            var client = type
                .GetProperty("Discord", BindingFlags.NonPublic | BindingFlags.Instance)!
                .GetValue(snowflake) as DiscordClient;
            return client!;
        }

        public static Dictionary<string, Stream> ToFilesDisctionay(this IReadOnlyCollection<DiscordMessageFile> Files)
        {
            Dictionary<string, Stream> FilesDict = new Dictionary<string, Stream>();
            foreach (var File in Files)
                FilesDict.Add(File.FileName, File.Stream);

            return FilesDict;
        }

        public static DiscordInteractionResponseBuilder ToResponseBuilder(this DiscordMessageBuilder builder)
        {
            return new DiscordInteractionResponseBuilder(builder);
            //.AddComponents(builder.Components[0]).AddComponents(builder.Components[1]).AddComponents(builder.Components[2]).AddComponents(builder.Components[3]).AddComponents(builder.Components[4])
            //.AddEmbeds(builder.Embeds)
            //.WithContent(builder.Content)
            //.WithTTS(builder.IsTTS);
        }

        public static DiscordWebhookBuilder ToWebhookBuilder(this DiscordMessageBuilder builder)
        {
            return new DiscordWebhookBuilder()
            .AddComponents(builder.Components.ElementAtOrDefault(0).Components).AddComponents(builder.Components.ElementAtOrDefault(1).Components).AddComponents(builder.Components.ElementAtOrDefault(2).Components).AddComponents(builder.Components.ElementAtOrDefault(3).Components).AddComponents(builder.Components.ElementAtOrDefault(4).Components)
            .AddEmbeds(builder.Embeds)
            .WithContent(builder.Content)
            .WithTTS(builder.IsTTS);
            //.AddFiles(builder.Files.ToFilesDisctionay());
        }

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

        public static async Task<InteractivityResult<T>?> HandleTimeouts<T>(this InteractivityResult<T> e, DiscordMessage msg, DiscordMessageBuilder builder)
        {
            if (e.TimedOut)
            {
                DiscordMessageBuilder Response = new DiscordMessageBuilder();
                msg = await msg.Channel.GetMessageAsync(msg.Id).ConfigureAwait(false);
                msg = await msg.LockAllMessageComponentsAsync(builder).ConfigureAwait(false);
                await msg.RespondAsync(":alarm_clock: This intercativity timed out.").ConfigureAwait(false);
                return null;
            }
            else
            {
                return e;
            }
        }

        public static async Task<DiscordMessage> GetSlashCMessage(this InteractionContext ctx)
            => await ctx.Channel.GetMessageAsync(ctx.Interaction.Data.Id).ConfigureAwait(false);

        public static async Task TriggerThinkingAsync(this InteractionContext ctx)
            => await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource).ConfigureAwait(false);

        public static IEnumerable<DiscordEmoji> GetDiscordEmojisByUnicodes(this DiscordClient client, params string[] EmojisUnicodes)
        {
            List<DiscordEmoji> emotes = new();

            foreach (string EmoteUnicode in EmojisUnicodes)
                emotes.Add(DiscordEmoji.FromUnicode(client, EmoteUnicode));

            return emotes;
        }

        public static IEnumerable<DiscordEmoji> GetDiscordEmojisByNames(this DiscordClient client, params string[] EmojisNames)
        {
            List<DiscordEmoji> emotes = new();

            foreach (string EmoteName in EmojisNames)
                emotes.Add(DiscordEmoji.FromName(client, EmoteName));

            return emotes;
        }

        public static async Task<DiscordMessage> AddReactionsAsync(this DiscordMessage msg, IEnumerable<DiscordEmoji> emojis)
        {
            foreach (DiscordEmoji emoji in emojis)
                await msg.CreateReactionAsync(emoji).ConfigureAwait(false);

            return msg;
        }

        public static async Task AddReactionsAsync(this DiscordMessage msg, params DiscordEmoji[] emojis)
        {
            foreach (DiscordEmoji emoji in emojis)
                await msg.CreateReactionAsync(emoji).ConfigureAwait(false);
        }

        public async static Task LogBotError(CommandErrorEventArgs e)
        {
            e.Context.Client.Logger.LogError
            (EventId,
                $"{e.Context.User.Username} tried executing '{e.Command?.QualifiedName ?? $"{e.Context.Message.Content} (unknown command)"}' " +
                $"but it errored: {e.Exception.GetType()}: {e.Exception.Message ?? "<no message>"}",
                DateTime.Now
            );
        }

        public async static Task LogBotError(this CommandContext ctx, Exception exception)
            => ctx.Client.Logger.LogError(EventId, $"{ctx.User.Username} tried executing '{ctx.Command?.QualifiedName ?? $"{ctx.Message.Content} (unknown command)"}' but it errored: {exception.GetType()}: {exception.Message ?? "<no message>"}", DateTime.Now);

        public static async Task<DiscordGuildTemplate> RestoreFromTemplateAsync(string TemplateCode, DiscordClient ctx)
            => (await ctx.GetTemplateAsync(TemplateCode));

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


        private static bool TryGetMember(this DiscordGuild guild, DiscordUser user, out DiscordMember member)
        {
            try
            {
                member = guild.GetMemberAsync(user.Id).GetAwaiter().GetResult();
                if (member is not null)
                    return true;
                else
                {
                    member = null;
                    return false;
                }
            }
            catch
            {
                member = null;
                return false;
            }

        }

        public static IEnumerable<DiscordInteractionDataOption> PurifyOptions(this IEnumerable<DiscordInteractionDataOption> RawOptions)
            => RawOptions.Where(x => x.Type != (ApplicationCommandOptionType.SubCommand | ApplicationCommandOptionType.SubCommandGroup)).FirstOrDefault().Options;

        public static Dictionary<string, ExtendedEmoteModel> Optimize(this IReadOnlyCollection<DiscordGuildEmoji> Emotes)
        {
            Dictionary<string, ExtendedEmoteModel> OptimizedVer = new Dictionary<string, ExtendedEmoteModel>();

            foreach (DiscordGuildEmoji Emote in Emotes)
            {
                OptimizedVer.Add(Emote.Id.ToString(), new ExtendedEmoteModel
                {
                    CreationTimestamp = Emote.CreationTimestamp,
                    Id = Emote.Id,
                    IsAnimated = Emote.IsAnimated,
                    IsAvailable = Emote.IsAvailable,
                    IsManaged = Emote.IsManaged,
                    Name = Emote.Name,
                    RequiresColons = Emote.RequiresColons,
                    Roles = Emote.Roles.ToList(),
                    Url = Emote.Url,
                    User = Emote.Guild.Members.TryGetValue(Emote.User.Id, out var Member) ? Member.Optimize() : null
                });
            }

            return OptimizedVer;
        }

        public static Dictionary<ulong, ExtendedRoleModel> Optimize(this IReadOnlyDictionary<ulong, DiscordRole> Roles)
        {
            Dictionary<ulong, ExtendedRoleModel> OptimizedVer = new Dictionary<ulong, ExtendedRoleModel>();

            foreach (KeyValuePair<ulong, DiscordRole> Role in Roles)
            {
                OptimizedVer.Add(Role.Key, new ExtendedRoleModel
                {
                    Color = Role.Value.Color.Value,
                    CreationTimeStamp = Role.Value.CreationTimestamp.DateTime,
                    Id = Role.Value.Id,
                    IsManaged = Role.Value.IsManaged,
                    IsMentionable = Role.Value.IsMentionable,
                    Name = Role.Value.Name,
                    Permissions = Role.Value.Permissions,
                    Position = Role.Value.Position
                });
            }

            return OptimizedVer;
        }

        public static async Task<ExtendedGuildModel> OptimizeAsync(this DiscordGuild Guild)
        {
            return new ExtendedGuildModel
            {
                PremiumSubscriptionCount = Guild.PremiumSubscriptionCount,
                RulesChannel = Guild.RulesChannel,
                WidgetChannel = Guild.WidgetChannel,
                WidgetEnabled = Guild.WidgetEnabled,
                SystemChannelFlags = Guild.SystemChannelFlags,
                SystemChannel = Guild.SystemChannel,
                ExplicitContentFilter = Guild.ExplicitContentFilter,
                DefaultMessageNotifications = Guild.DefaultMessageNotifications,
                VerificationLevel = Guild.VerificationLevel,
                AfkTimeout = Guild.AfkTimeout,
                AfkChannel = Guild.AfkChannel,
                VoiceRegion = Guild.VoiceRegion,
                Owner = Guild.Owner.Optimize(),
                Permissions = Guild.Permissions,
                OwnerId = Guild.OwnerId,
                PreferredLocale = Guild.PreferredLocale,
                DiscoverySplashUrl = Guild.DiscoverySplashUrl,
                DiscoverySplashHash = Guild.DiscoverySplashHash,
                SplashUrl = Guild.SplashUrl,
                SplashHash = Guild.SplashHash,
                IconUrl = Guild.IconUrl,
                IconHash = Guild.IconHash,
                PublicUpdatesChannel = Guild.PublicUpdatesChannel,
                Name = Guild.Name,
                ApplicationId = Guild.ApplicationId,
                Emojis = (await Guild.GetEmojisAsync()).Optimize(),
                BannerUrl = Guild.BannerUrl,
                Banner = Guild.Banner,
                Description = Guild.Description,
                VanityUrlCode = Guild.VanityUrlCode,
                EveryoneRole = Guild.EveryoneRole,
                Channels = Guild.Channels.ToDictionary().ToDictionary(),
                MaxVideoChannelUsers = Guild.MaxVideoChannelUsers,
                ApproximatePresenceCount = Guild.ApproximatePresenceCount,
                ApproximateMemberCount = Guild.ApproximateMemberCount,
                MaxPresences = Guild.MaxPresences,
                MaxMembers = Guild.MaxMembers,
                MemberCount = Guild.MemberCount,
                IsUnavailable = Guild.IsUnavailable,
                IsLarge = Guild.IsLarge,
                MfaLevel = Guild.MfaLevel,
                Roles = (Guild.Roles.Optimize()).ToDictionary(),
                PremiumTier = Guild.PremiumTier,
                Bots = await Guild.GetBotsAsync()
            };
        }

        public static Dictionary<ulong, ExtendedRoleModel> ToRolesDictionary(this IEnumerable<DiscordRole> Roles)
        {
            Dictionary<ulong, ExtendedRoleModel> RolesDict = new Dictionary<ulong, ExtendedRoleModel>();

            foreach (DiscordRole Role in Roles)
                RolesDict.Add(Role.Id, Role.Optimize());

            return RolesDict;
        }

        public static ExtendedRoleModel Optimize(this DiscordRole Role)
        {
            return new ExtendedRoleModel
            {
                Id = Role.Id,
                Name = Role.Name,
                Color = Role.Color.Value,
                Position = Role.Position,
                Permissions = Role.Permissions,
                IsManaged = Role.IsManaged,
                IsMentionable = Role.IsMentionable,
            };
        }

        public static ExtendedMemberModel Optimize(this DiscordMember Member)
        {
            return new ExtendedMemberModel
            {
                Id = Member.Id,
                MentionString = Member.Mention,
                AvatarHash = Member.AvatarHash,
                Color = Member.Color.Value,
                Discriminator = Member.Discriminator,
                DisplayName = Member.DisplayName,
                Email = Member.Email,
                Hierarchy = Member.Hierarchy,
                IsBot = Member.IsBot,
                IsMuted = Member.IsMuted,
                IsOwner = Member.IsOwner,
                IsPending = Member.IsPending,
                JoinedAt = Member.JoinedAt,
                Locale = Member.Locale,
                MfaEnabled = Member.MfaEnabled,
                Nickname = Member.Nickname,
                PremiumSince = Member.PremiumSince,
                Roles = Member.Roles.ToRolesDictionary().ToDictionary(),
                Username = Member.Username,
                Verified = Member.Verified
            };
        }

        public static async Task<List<ExtendedBotIntegration>> GetBotsAsync(this DiscordGuild Guild)
        {
            List<ExtendedBotIntegration> Bots = new List<ExtendedBotIntegration>();
            IReadOnlyList<DiscordIntegration> integrations = (await Guild.GetIntegrationsAsync().ConfigureAwait(false));

            foreach (DiscordIntegration integration in integrations)
            {
                DiscordMember IntegrationMember = await Guild.GetMemberAsync(integration.Account.Id).ConfigureAwait(false);

                if (integration.Type.ToLower() == "Discord".ToLower() && (IntegrationMember.IsBot == true))
                {
                    Bots.Add
                        (
                            new ExtendedBotIntegration
                            {
                                Name = integration.Name,
                                Type = integration.Type,
                                IsEnabled = integration.IsEnabled,
                                IsSyncing = integration.IsSyncing,
                                RoleId = integration.RoleId,
                                ExpireBehavior = integration.ExpireBehavior,
                                ExpireGracePeriod = integration.ExpireGracePeriod,
                                IntegrationOwnerUser = integration.User,
                                Account = integration.Account,
                                SyncedAt = integration.SyncedAt,
                                BotAsMemberInGuild = new ExtendedMemberModel
                                {
                                    AvatarHash = IntegrationMember.AvatarHash,
                                    Color = IntegrationMember.Color.Value,
                                    Discriminator = IntegrationMember.Discriminator,
                                    DisplayName = IntegrationMember.DisplayName,
                                    Email = IntegrationMember.Email,
                                    Hierarchy = IntegrationMember.Hierarchy,
                                    IsBot = IntegrationMember.IsBot,
                                    IsMuted = IntegrationMember.IsMuted,
                                    IsOwner = IntegrationMember.IsOwner,
                                    IsPending = IntegrationMember.IsPending,
                                    JoinedAt = IntegrationMember.JoinedAt,
                                    Locale = IntegrationMember.Locale,
                                    MfaEnabled = IntegrationMember.MfaEnabled,
                                    Nickname = IntegrationMember.Nickname,
                                    PremiumSince = IntegrationMember.PremiumSince,
                                    Roles = IntegrationMember.Roles.ToRolesDictionary().ToDictionary(),
                                    Username = IntegrationMember.Username,
                                    Verified = IntegrationMember.Verified

                                }
                            }
                        );
                }
            }

            return Bots;
        }

        public static async Task LockDownGuildAsync(this DiscordGuild Guild, DiscordClient ctx, List<ulong> ExcludedChannels = null)
        {
            IReadOnlyList<DiscordChannel> GuildChannels = await Guild.GetChannelsAsync().ConfigureAwait(false);
            DiscordRole EveryoneRole = Guild.GetRole(Guild.EveryoneRole.Id);
            DiscordEmoji LockEmoji = DiscordEmoji.FromName(ctx, ":lock:");

            foreach (DiscordChannel Channel in GuildChannels)
            {
                if (!ExcludedChannels.Contains(Channel.Id))
                {
                    if (Channel.Type == ChannelType.Voice & !ExcludedChannels.Contains(Channel.Id))
                    {
                        foreach (DiscordMember User in Channel.Users)
                        {
                            await User.SetDeafAsync(true).ConfigureAwait(false);
                        }
                    }

                    foreach (KeyValuePair<ulong, DiscordRole> Role in Guild.Roles)
                    {
                        if (!ExcludedChannels.Contains(Channel.Id))
                        {
                            await Channel.AddOverwriteAsync(Role.Value, Permissions.None, Permissions.All)
                                .ConfigureAwait(false);

                            await Channel.ModifyAsync(x => x.Name = $"{Channel.Name}-{LockEmoji}")
                                .ConfigureAwait(false);
                        }
                    }
                }
            }
        }

        public static async Task UNLockGuildAsync(this DiscordGuild Guild, DiscordClient ctx, List<ulong> ExcludedChannels = null)
        {
            IReadOnlyList<DiscordChannel> GuildChannels = await Guild.GetChannelsAsync().ConfigureAwait(false);
            DiscordRole EveryoneRole = Guild.GetRole(Guild.EveryoneRole.Id);
            DiscordEmoji LockEmoji = DiscordEmoji.FromName(ctx, ":lock:");

            foreach (DiscordChannel Channel in GuildChannels)
            {
                if (!ExcludedChannels.Contains(Channel.Id))
                {
                    await Channel.ModifyAsync(x => x.Name = Channel.Name.Replace($"-{LockEmoji}", ""))
                        .ConfigureAwait(false);
                    foreach (KeyValuePair<ulong, DiscordRole> Role in Guild.Roles)
                    {
                        Channel.AddOverwriteAsync(Role.Value, Permissions.SendMessages, Permissions.None)
                            .ConfigureAwait(false);
                    }
                }
            }
        }
    }
}