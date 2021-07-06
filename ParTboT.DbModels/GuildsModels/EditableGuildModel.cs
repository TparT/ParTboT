using DSharpPlus;
using DSharpPlus.Entities;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ParTboT.DbModels.DSharpPlus
{
    public class ExtendedGuildModel
    {
        //
        // Summary:
        //     Gets the amount of members that boosted this guild.
        //("premium_subscription_count", NullValueHandling = NullValueHandling.Ignore)]
        public int? PremiumSubscriptionCount { get; set; }
        //
        // Summary:
        //     Gets the rules channel for this guild.
        //     This is only available if the guild is considered "discoverable".

        public DiscordChannel RulesChannel { get; set; } = null;
        //
        // Summary:
        //     Gets the widget channel for this guild.

        public DiscordChannel WidgetChannel { get; set; } = null;
        //
        // Summary:
        //     Gets whether this guild's widget is enabled.
        public bool? WidgetEnabled { get; set; }
        //
        // Summary:
        //     Gets the settings for this guild's system channel.
        public SystemChannelFlags SystemChannelFlags { get; set; }
        //
        // Summary:
        //     Gets the channel where system messages (such as boost and welcome messages) are
        //     sent.
        
        public DiscordChannel SystemChannel { get; set; }
        //
        // Summary:
        //     Gets the guild's explicit content filter settings.
        public ExplicitContentFilter ExplicitContentFilter { get; set; }
        //
        // Summary:
        //     Gets the guild's default notification settings.
        public DefaultMessageNotifications DefaultMessageNotifications { get; set; }
        //
        // Summary:
        //     Gets the guild's verification level.
        public VerificationLevel VerificationLevel { get; set; }
        //
        // Summary:
        //     Gets the guild's AFK timeout.
        public int AfkTimeout { get; set; }
        //
        // Summary:
        //     Gets the guild's AFK voice channel.
        
        public DiscordChannel AfkChannel { get; set; }
        //
        // Summary:
        //     Gets the guild's voice region.
        
        public DiscordVoiceRegion VoiceRegion { get; set; }
        //
        // Summary:
        //     Gets the guild's owner.
        public ExtendedMemberModel Owner { get; set; }
        //
        // Summary:
        //     Gets permissions for the user in the guild (does not include channel overrides)
        public Permissions? Permissions { get; set; }
        //
        // Summary:
        //     Gets the ID of the guild's owner.
        //("owner_id", NullValueHandling = NullValueHandling.Ignore)]
        public ulong OwnerId { get; set; }
        //
        // Summary:
        //     Gets the preferred locale of this guild.
        //     This is used for server discovery and notices from Discord. Defaults to en-US.
        public string PreferredLocale { get; set; }
        //
        // Summary:
        //     Gets the guild discovery splash's url.

        public string DiscoverySplashUrl { get; set; } = null;
        //
        // Summary:
        //     Gets the guild discovery splash's hash.
        public string DiscoverySplashHash { get; set; } = null;
        //
        // Summary:
        //     Gets the guild splash's url.

        public string SplashUrl { get; set; } = null;
        //
        // Summary:
        //     Gets the guild splash's hash.
        public string SplashHash { get; set; } = null;
        //
        // Summary:
        //     Gets the guild icon's url.

        public string IconUrl { get; set; } = null;
        //
        // Summary:
        //     Gets the guild icon's hash.
        public string IconHash { get; set; } = null;
        //
        // Summary:
        //     Gets the public updates channel (where admins and moderators receive messages
        //     from Discord) for this guild.
        //     This is only available if the guild is considered "discoverable".
        
        public DiscordChannel PublicUpdatesChannel { get; set; }
        //
        // Summary:
        //     Gets the guild's name.
        public string Name { get; set; }
        //
        // Summary:
        //     Gets the application id of this guild if it is bot created.
        public ulong? ApplicationId { get; set; }
        //
        // Summary:
        //     Gets a collection of this guild's emojis.

        public Dictionary<string, ExtendedEmoteModel> Emojis { get; set; } = null;
        //
        // Summary:
        //     Gets this guild's banner in url form.

        public string BannerUrl { get; set; } = null;
        //
        // Summary:
        //     Gets this guild's banner hash, when applicable.
        public string Banner { get; set; } = null;
        //
        // Summary:
        //     Gets the guild description, when applicable.
        public string Description { get; set; } = null;
        //
        // Summary:
        //     Gets vanity URL code for this guild, when applicable.
        public string VanityUrlCode { get; set; } = null;
        //
        // Summary:
        //     Gets whether the current user is the guild's owner.
        public bool IsOwner { get; set; }
        //
        // Summary:
        //     Gets the @everyone role for this guild.
        public DiscordRole EveryoneRole { get; set; }
        //
        // Summary:
        //     Gets the guild member for current user.
        public DiscordMember CurrentMember { get; set; }
        //
        // Summary:
        //     Gets a dictionary of all the channels associated with this guild. The dictionary's
        //     key is the channel ID.
        public Dictionary<string, DiscordChannel> Channels { get; set; }
        
        //
        // Summary:
        //     Gets a dictionary of all the members that belong to this guild. The dictionary's
        //     key is the member ID.
        //public Dictionary<string, DiscordMember> Members { get; set; }

        //
        // Summary:
        //     Gets a dictionary of all the voice states for this guilds. The key for this dictionary
        //     is the ID of the user the voice state corresponds to.
        //public Dictionary<string, DiscordVoiceState> VoiceStates { get; set; }
        //
        // Summary:
        //     Gets the maximum amount of users allowed per video channel.
        public int? MaxVideoChannelUsers { get; set; }
        //
        // Summary:
        //     Gets the approximate number of presences in this guild, when using DSharpPlus.DiscordClient.GetGuildAsync(System.UInt64,System.Nullable{System.Boolean})
        //     and having withCounts set to true.
        public int? ApproximatePresenceCount { get; set; }
        //
        // Summary:
        //     Gets the approximate number of members in this guild, when using DSharpPlus.DiscordClient.GetGuildAsync(System.UInt64,System.Nullable{System.Boolean})
        //     and having withCounts set to true.
        public int? ApproximateMemberCount { get; set; }
        //
        // Summary:
        //     Gets the maximum amount of presences allowed for this guild.
        public int? MaxPresences { get; set; }
        //
        // Summary:
        //     Gets the maximum amount of members allowed for this guild.
        public int? MaxMembers { get; set; }
        //
        // Summary:
        //     Gets the total number of members in this guild.
        public int MemberCount { get; set; }
        //
        // Summary:
        //     Gets whether this guild is unavailable.
        public bool IsUnavailable { get; set; }
        //
        // Summary:
        //     Gets whether this guild is considered to be a large guild.
        public bool IsLarge { get; set; }
        //
        // Summary:
        //     Gets this guild's join date.
        public DateTime JoinedAt { get; set; }
        //
        // Summary:
        //     Gets the required multi-factor authentication level for this guild.
        public MfaLevel MfaLevel { get; set; }
        //
        // Summary:
        //     Gets a collection of this guild's features.
        public List<string> Features { get; set; }
        //
        // Summary:
        //     Gets a collection of this guild's roles.
        public Dictionary<string, ExtendedRoleModel> Roles { get; set; }
        //
        // Summary:
        //     Gets this guild's premium tier (Nitro boosting).
        public PremiumTier PremiumTier { get; set; }

        public List<ExtendedBotIntegration> Bots { get; set; }
    }
}
