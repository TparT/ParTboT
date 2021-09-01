using DSharpPlus.Entities;
using ParTboT.DbModels.DSharpPlus;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YarinGeorge.Utilities.Extensions;

namespace ParTboT.Converters
{
    public static class DatabaseModelsOptimizers
    {
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
                DiscordMember IntegrationMember = await Guild.GetMemberAsync(ulong.Parse(integration.Account.Id)).ConfigureAwait(false);

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
    }
}
