using System;
using System.Collections.Generic;

using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace YarinGeorge.ApiClients.TrackerGG.StatsModels
{
    public class ApexLegendsStats
    {
        [JsonProperty("data", NullValueHandling = NullValueHandling.Ignore)]
        public Data data { get; set; }
    }

    public class Data
    {
        [JsonProperty("platformInfo", NullValueHandling = NullValueHandling.Ignore)]
        public PlatformInfo PlatformInfo { get; set; }

        [JsonProperty("userInfo", NullValueHandling = NullValueHandling.Ignore)]
        public UserInfo UserInfo { get; set; }

        [JsonProperty("metadata", NullValueHandling = NullValueHandling.Ignore)]
        public DataMetadata Metadata { get; set; }

        [JsonProperty("segments", NullValueHandling = NullValueHandling.Ignore)]
        public List<Segment> Segments { get; set; }

        [JsonProperty("availableSegments", NullValueHandling = NullValueHandling.Ignore)]
        public List<AvailableSegment> AvailableSegments { get; set; }

        [JsonProperty("expiryDate", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? ExpiryDate { get; set; }
    }

    public class AvailableSegment
    {
        [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
        public TypeEnum? Type { get; set; }

        [JsonProperty("attributes", NullValueHandling = NullValueHandling.Ignore)]
        public MetadataClass Attributes { get; set; }

        [JsonProperty("metadata", NullValueHandling = NullValueHandling.Ignore)]
        public MetadataClass Metadata { get; set; }
    }

    public class MetadataClass
    {
    }

    public class DataMetadata
    {
        [JsonProperty("currentSeason", NullValueHandling = NullValueHandling.Ignore)]
        public long? CurrentSeason { get; set; }

        [JsonProperty("activeLegend", NullValueHandling = NullValueHandling.Ignore)]
        public string ActiveLegend { get; set; }

        [JsonProperty("activeLegendName", NullValueHandling = NullValueHandling.Ignore)]
        public string ActiveLegendName { get; set; }

        [JsonProperty("activeLegendStats", NullValueHandling = NullValueHandling.Ignore)]
        public List<ActiveLegendStat> ActiveLegendStats { get; set; }
    }

    public class PlatformInfo
    {
        [JsonProperty("platformSlug", NullValueHandling = NullValueHandling.Ignore)]
        public string PlatformSlug { get; set; }

        [JsonProperty("platformUserId", NullValueHandling = NullValueHandling.Ignore)]
        public string PlatformUserId { get; set; }

        [JsonProperty("platformUserHandle", NullValueHandling = NullValueHandling.Ignore)]
        public string PlatformUserHandle { get; set; }

        [JsonProperty("platformUserIdentifier", NullValueHandling = NullValueHandling.Ignore)]
        public string PlatformUserIdentifier { get; set; }

        [JsonProperty("avatarUrl", NullValueHandling = NullValueHandling.Ignore)]
        public Uri AvatarUrl { get; set; }

        [JsonProperty("additionalParameters")]
        public object AdditionalParameters { get; set; }
    }

    public class Segment
    {
        [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
        public TypeEnum? Type { get; set; }

        [JsonProperty("attributes", NullValueHandling = NullValueHandling.Ignore)]
        public SegmentAttributes Attributes { get; set; }

        [JsonProperty("metadata", NullValueHandling = NullValueHandling.Ignore)]
        public SegmentMetadata Metadata { get; set; }

        [JsonProperty("expiryDate", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? ExpiryDate { get; set; }

        [JsonProperty("stats", NullValueHandling = NullValueHandling.Ignore)]
        public Stats Stats { get; set; }
    }

    public class SegmentAttributes
    {
        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public string Id { get; set; }
    }

    public class SegmentMetadata
    {
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        [JsonProperty("imageUrl", NullValueHandling = NullValueHandling.Ignore)]
        public Uri ImageUrl { get; set; }

        [JsonProperty("tallImageUrl", NullValueHandling = NullValueHandling.Ignore)]
        public Uri TallImageUrl { get; set; }

        [JsonProperty("bgImageUrl", NullValueHandling = NullValueHandling.Ignore)]
        public Uri BgImageUrl { get; set; }

        [JsonProperty("isActive", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsActive { get; set; }
    }

    public class Stats
    {
        [JsonProperty("level", NullValueHandling = NullValueHandling.Ignore)]
        public Damage Level { get; set; }

        [JsonProperty("kills", NullValueHandling = NullValueHandling.Ignore)]
        public Kills Kills { get; set; }

        [JsonProperty("killsPerMatch", NullValueHandling = NullValueHandling.Ignore)]
        public KillsPerMatch KillsPerMatch { get; set; }

        [JsonProperty("damagePerMatch", NullValueHandling = NullValueHandling.Ignore)]
        public DamagePerMatch DamagePerMatch { get; set; }

        [JsonProperty("winningKills", NullValueHandling = NullValueHandling.Ignore)]
        public KillsAsKillLeader WinningKills { get; set; }

        [JsonProperty("killsAsKillLeader", NullValueHandling = NullValueHandling.Ignore)]
        public KillsAsKillLeader KillsAsKillLeader { get; set; }

        [JsonProperty("damage", NullValueHandling = NullValueHandling.Ignore)]
        public Damage Damage { get; set; }

        [JsonProperty("headshots", NullValueHandling = NullValueHandling.Ignore)]
        public Damage Headshots { get; set; }

        [JsonProperty("matchesPlayed", NullValueHandling = NullValueHandling.Ignore)]
        public Finishers MatchesPlayed { get; set; }

        [JsonProperty("finishers", NullValueHandling = NullValueHandling.Ignore)]
        public Finishers Finishers { get; set; }

        [JsonProperty("revives", NullValueHandling = NullValueHandling.Ignore)]
        public GrappleTravelDistance Revives { get; set; }

        [JsonProperty("winsWithFullSquad", NullValueHandling = NullValueHandling.Ignore)]
        public Finishers WinsWithFullSquad { get; set; }

        [JsonProperty("shotgunKills", NullValueHandling = NullValueHandling.Ignore)]
        public ArKills ShotgunKills { get; set; }

        [JsonProperty("smgKills", NullValueHandling = NullValueHandling.Ignore)]
        public ArKills SmgKills { get; set; }

        [JsonProperty("arKills", NullValueHandling = NullValueHandling.Ignore)]
        public ArKills ArKills { get; set; }

        [JsonProperty("rankScore", NullValueHandling = NullValueHandling.Ignore)]
        public RankScore RankScore { get; set; }

        [JsonProperty("season5Wins", NullValueHandling = NullValueHandling.Ignore)]
        public ArKills Season5Wins { get; set; }

        [JsonProperty("season5Kills", NullValueHandling = NullValueHandling.Ignore)]
        public ArKills Season5Kills { get; set; }

        [JsonProperty("season6Wins", NullValueHandling = NullValueHandling.Ignore)]
        public ArKills Season6Wins { get; set; }

        [JsonProperty("season6Kills", NullValueHandling = NullValueHandling.Ignore)]
        public ArKills Season6Kills { get; set; }

        [JsonProperty("season7Wins", NullValueHandling = NullValueHandling.Ignore)]
        public ArKills Season7Wins { get; set; }

        [JsonProperty("season7Kills", NullValueHandling = NullValueHandling.Ignore)]
        public ArKills Season7Kills { get; set; }

        [JsonProperty("season8Wins", NullValueHandling = NullValueHandling.Ignore)]
        public ArKills Season8Wins { get; set; }

        [JsonProperty("season8Kills", NullValueHandling = NullValueHandling.Ignore)]
        public ArKills Season8Kills { get; set; }

        [JsonProperty("creepingBarrageDamage", NullValueHandling = NullValueHandling.Ignore)]
        public ArKills CreepingBarrageDamage { get; set; }

        [JsonProperty("ultimateLootTakenByAllies", NullValueHandling = NullValueHandling.Ignore)]
        public Finishers UltimateLootTakenByAllies { get; set; }

        [JsonProperty("voicesWarningsHeard", NullValueHandling = NullValueHandling.Ignore)]
        public PassiveHealthRegenerated VoicesWarningsHeard { get; set; }

        [JsonProperty("passiveHealthRegenerated", NullValueHandling = NullValueHandling.Ignore)]
        public PassiveHealthRegenerated PassiveHealthRegenerated { get; set; }
    }

    public class DamagePerMatch
    {
        [JsonProperty("rank")]
        public object Rank { get; set; }

        [JsonProperty("percentile", NullValueHandling = NullValueHandling.Ignore)]
        public double? Percentile { get; set; }

        [JsonProperty("displayName", NullValueHandling = NullValueHandling.Ignore)]
        public string DisplayName { get; set; }

        [JsonProperty("displayCategory", NullValueHandling = NullValueHandling.Ignore)]
        public string DisplayCategory { get; set; }

        [JsonProperty("category")]
        public object Category { get; set; }

        [JsonProperty("metadata", NullValueHandling = NullValueHandling.Ignore)]
        public MetadataClass Metadata { get; set; }

        [JsonProperty("value", NullValueHandling = NullValueHandling.Ignore)]
        public long? Value { get; set; }

        [JsonProperty("displayValue", NullValueHandling = NullValueHandling.Ignore)]
        public string DisplayValue { get; set; }

        [JsonProperty("displayType", NullValueHandling = NullValueHandling.Ignore)]
        public DisplayType? DisplayType { get; set; }
    }

    public class KillsPerMatch
    {
        [JsonProperty("rank")]
        public object Rank { get; set; }

        [JsonProperty("percentile", NullValueHandling = NullValueHandling.Ignore)]
        public double? Percentile { get; set; }

        [JsonProperty("displayName", NullValueHandling = NullValueHandling.Ignore)]
        public string DisplayName { get; set; }

        [JsonProperty("displayCategory", NullValueHandling = NullValueHandling.Ignore)]
        public string DisplayCategory { get; set; }

        [JsonProperty("category")]
        public object Category { get; set; }

        [JsonProperty("metadata", NullValueHandling = NullValueHandling.Ignore)]
        public MetadataClass Metadata { get; set; }

        [JsonProperty("value", NullValueHandling = NullValueHandling.Ignore)]
        public long? Value { get; set; }

        [JsonProperty("displayValue", NullValueHandling = NullValueHandling.Ignore)]
        public string DisplayValue { get; set; }

        [JsonProperty("displayType", NullValueHandling = NullValueHandling.Ignore)]
        public DisplayType? DisplayType { get; set; }
    }

    public class ArKills
    {
        [JsonProperty("rank")]
        public object Rank { get; set; }

        [JsonProperty("percentile", NullValueHandling = NullValueHandling.Ignore)]
        public double? Percentile { get; set; }

        [JsonProperty("displayName", NullValueHandling = NullValueHandling.Ignore)]
        public string DisplayName { get; set; }

        [JsonProperty("displayCategory", NullValueHandling = NullValueHandling.Ignore)]
        public string DisplayCategory { get; set; }

        [JsonProperty("category")]
        public object Category { get; set; }

        [JsonProperty("metadata", NullValueHandling = NullValueHandling.Ignore)]
        public MetadataClass Metadata { get; set; }

        [JsonProperty("value", NullValueHandling = NullValueHandling.Ignore)]
        public long? Value { get; set; }

        [JsonProperty("displayValue", NullValueHandling = NullValueHandling.Ignore)]
        public string DisplayValue { get; set; }

        [JsonProperty("displayType", NullValueHandling = NullValueHandling.Ignore)]
        public DisplayType? DisplayType { get; set; }
    }

    public class Damage
    {
        [JsonProperty("rank", NullValueHandling = NullValueHandling.Ignore)]
        public long? Rank { get; set; }

        [JsonProperty("percentile", NullValueHandling = NullValueHandling.Ignore)]
        public double? Percentile { get; set; }

        [JsonProperty("displayName", NullValueHandling = NullValueHandling.Ignore)]
        public string DisplayName { get; set; }

        [JsonProperty("displayCategory", NullValueHandling = NullValueHandling.Ignore)]
        public DisplayCategory? DisplayCategory { get; set; }

        [JsonProperty("category")]
        public object Category { get; set; }

        [JsonProperty("metadata", NullValueHandling = NullValueHandling.Ignore)]
        public MetadataClass Metadata { get; set; }

        [JsonProperty("value", NullValueHandling = NullValueHandling.Ignore)]
        public long? Value { get; set; }

        [JsonProperty("displayValue", NullValueHandling = NullValueHandling.Ignore)]
        public string DisplayValue { get; set; }

        [JsonProperty("displayType", NullValueHandling = NullValueHandling.Ignore)]
        public DisplayType? DisplayType { get; set; }
    }

    public class GrappleTravelDistance
    {
        [JsonProperty("rank")]
        public object Rank { get; set; }

        [JsonProperty("percentile", NullValueHandling = NullValueHandling.Ignore)]
        public double? Percentile { get; set; }

        [JsonProperty("displayName", NullValueHandling = NullValueHandling.Ignore)]
        public string DisplayName { get; set; }

        [JsonProperty("displayCategory", NullValueHandling = NullValueHandling.Ignore)]
        public string DisplayCategory { get; set; }

        [JsonProperty("category")]
        public object Category { get; set; }

        [JsonProperty("metadata", NullValueHandling = NullValueHandling.Ignore)]
        public MetadataClass Metadata { get; set; }

        [JsonProperty("value", NullValueHandling = NullValueHandling.Ignore)]
        public long? Value { get; set; }

        [JsonProperty("displayValue", NullValueHandling = NullValueHandling.Ignore)]
        public string DisplayValue { get; set; }

        [JsonProperty("displayType", NullValueHandling = NullValueHandling.Ignore)]
        public DisplayType? DisplayType { get; set; }
    }

    public class Finishers
    {
        [JsonProperty("rank")]
        public object Rank { get; set; }

        [JsonProperty("percentile", NullValueHandling = NullValueHandling.Ignore)]
        public long? Percentile { get; set; }

        [JsonProperty("displayName", NullValueHandling = NullValueHandling.Ignore)]
        public string DisplayName { get; set; }

        [JsonProperty("displayCategory", NullValueHandling = NullValueHandling.Ignore)]
        public string DisplayCategory { get; set; }

        [JsonProperty("category")]
        public object Category { get; set; }

        [JsonProperty("metadata", NullValueHandling = NullValueHandling.Ignore)]
        public MetadataClass Metadata { get; set; }

        [JsonProperty("value", NullValueHandling = NullValueHandling.Ignore)]
        public long? Value { get; set; }

        [JsonProperty("displayValue", NullValueHandling = NullValueHandling.Ignore)]
        public long? DisplayValue { get; set; }

        [JsonProperty("displayType", NullValueHandling = NullValueHandling.Ignore)]
        public DisplayType? DisplayType { get; set; }
    }

    public class Kills
    {
        [JsonProperty("rank")]
        public long? Rank { get; set; }

        [JsonProperty("percentile")]
        public double? Percentile { get; set; }

        [JsonProperty("displayName", NullValueHandling = NullValueHandling.Ignore)]
        public ActiveLegendStat? DisplayName { get; set; }

        [JsonProperty("displayCategory", NullValueHandling = NullValueHandling.Ignore)]
        public DisplayCategory? DisplayCategory { get; set; }

        [JsonProperty("category")]
        public object Category { get; set; }

        [JsonProperty("metadata", NullValueHandling = NullValueHandling.Ignore)]
        public MetadataClass Metadata { get; set; }

        [JsonProperty("value", NullValueHandling = NullValueHandling.Ignore)]
        public long? Value { get; set; }

        [JsonProperty("displayValue", NullValueHandling = NullValueHandling.Ignore)]
        public string DisplayValue { get; set; }

        [JsonProperty("displayType", NullValueHandling = NullValueHandling.Ignore)]
        public DisplayType? DisplayType { get; set; }
    }

    public class KillsAsKillLeader
    {
        [JsonProperty("rank")]
        public object Rank { get; set; }

        [JsonProperty("percentile", NullValueHandling = NullValueHandling.Ignore)]
        public double? Percentile { get; set; }

        [JsonProperty("displayName", NullValueHandling = NullValueHandling.Ignore)]
        public string DisplayName { get; set; }

        [JsonProperty("displayCategory", NullValueHandling = NullValueHandling.Ignore)]
        public DisplayCategory? DisplayCategory { get; set; }

        [JsonProperty("category")]
        public object Category { get; set; }

        [JsonProperty("metadata", NullValueHandling = NullValueHandling.Ignore)]
        public MetadataClass Metadata { get; set; }

        [JsonProperty("value", NullValueHandling = NullValueHandling.Ignore)]
        public long? Value { get; set; }

        [JsonProperty("displayValue", NullValueHandling = NullValueHandling.Ignore)]
        public string DisplayValue { get; set; }

        [JsonProperty("displayType", NullValueHandling = NullValueHandling.Ignore)]
        public DisplayType? DisplayType { get; set; }
    }

    public class PassiveHealthRegenerated
    {
        [JsonProperty("rank")]
        public object Rank { get; set; }

        [JsonProperty("percentile", NullValueHandling = NullValueHandling.Ignore)]
        public long? Percentile { get; set; }

        [JsonProperty("displayName", NullValueHandling = NullValueHandling.Ignore)]
        public string DisplayName { get; set; }

        [JsonProperty("displayCategory", NullValueHandling = NullValueHandling.Ignore)]
        public string DisplayCategory { get; set; }

        [JsonProperty("category")]
        public object Category { get; set; }

        [JsonProperty("metadata", NullValueHandling = NullValueHandling.Ignore)]
        public MetadataClass Metadata { get; set; }

        [JsonProperty("value", NullValueHandling = NullValueHandling.Ignore)]
        public long? Value { get; set; }

        [JsonProperty("displayValue", NullValueHandling = NullValueHandling.Ignore)]
        public string DisplayValue { get; set; }

        [JsonProperty("displayType", NullValueHandling = NullValueHandling.Ignore)]
        public DisplayType? DisplayType { get; set; }
    }

    public class RankScore
    {
        [JsonProperty("rank", NullValueHandling = NullValueHandling.Ignore)]
        public long? Rank { get; set; }

        [JsonProperty("percentile", NullValueHandling = NullValueHandling.Ignore)]
        public long? Percentile { get; set; }

        [JsonProperty("displayName", NullValueHandling = NullValueHandling.Ignore)]
        public string DisplayName { get; set; }

        [JsonProperty("displayCategory", NullValueHandling = NullValueHandling.Ignore)]
        public string DisplayCategory { get; set; }

        [JsonProperty("category")]
        public object Category { get; set; }

        [JsonProperty("metadata", NullValueHandling = NullValueHandling.Ignore)]
        public RankScoreMetadata Metadata { get; set; }

        [JsonProperty("value", NullValueHandling = NullValueHandling.Ignore)]
        public long? Value { get; set; }

        [JsonProperty("displayValue", NullValueHandling = NullValueHandling.Ignore)]
        public long? DisplayValue { get; set; }

        [JsonProperty("displayType", NullValueHandling = NullValueHandling.Ignore)]
        public DisplayType? DisplayType { get; set; }
    }

    public class RankScoreMetadata
    {
        [JsonProperty("iconUrl", NullValueHandling = NullValueHandling.Ignore)]
        public Uri IconUrl { get; set; }

        [JsonProperty("rankName", NullValueHandling = NullValueHandling.Ignore)]
        public string RankName { get; set; }
    }

    public class UserInfo
    {
        [JsonProperty("userId")]
        public object UserId { get; set; }

        [JsonProperty("isPremium", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsPremium { get; set; }

        [JsonProperty("isVerified", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsVerified { get; set; }

        [JsonProperty("isInfluencer", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsInfluencer { get; set; }

        [JsonProperty("isPartner", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsPartner { get; set; }

        [JsonProperty("countryCode")]
        public object CountryCode { get; set; }

        [JsonProperty("customAvatarUrl")]
        public object CustomAvatarUrl { get; set; }

        [JsonProperty("customHeroUrl")]
        public object CustomHeroUrl { get; set; }

        [JsonProperty("socialAccounts", NullValueHandling = NullValueHandling.Ignore)]
        public List<object> SocialAccounts { get; set; }

        [JsonProperty("pageviews", NullValueHandling = NullValueHandling.Ignore)]
        public long? Pageviews { get; set; }

        [JsonProperty("isSuspicious")]
        public object IsSuspicious { get; set; }
    }

    public enum TypeEnum { Legend, Overview };

    public enum ActiveLegendStat { Kills };

    public enum DisplayType { Unspecified };

    public enum DisplayCategory { Combat };
}
