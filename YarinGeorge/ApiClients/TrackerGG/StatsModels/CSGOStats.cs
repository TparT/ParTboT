using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YarinGeorge.ApiClients.TrackerGG.StatsModels
{
    public class CsGoStats
    {
        [JsonProperty("data", NullValueHandling = NullValueHandling.Ignore)]
        public Data data { get; set; }

        public class Data
        {
            [JsonProperty("platformInfo", NullValueHandling = NullValueHandling.Ignore)]
            public PlatformInfo PlatformInfo { get; set; }

            [JsonProperty("userInfo", NullValueHandling = NullValueHandling.Ignore)]
            public Dictionary<string, bool?> UserInfo { get; set; }

            [JsonProperty("metadata", NullValueHandling = NullValueHandling.Ignore)]
            public AttributesClass Metadata { get; set; }

            [JsonProperty("segments", NullValueHandling = NullValueHandling.Ignore)]
            public List<Segment> Segments { get; set; }

            [JsonProperty("availableSegments", NullValueHandling = NullValueHandling.Ignore)]
            public List<object> AvailableSegments { get; set; }

            [JsonProperty("expiryDate", NullValueHandling = NullValueHandling.Ignore)]
            public DateTimeOffset? ExpiryDate { get; set; }
        }

        public class AttributesClass
        {
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
            public string Type { get; set; }

            [JsonProperty("attributes", NullValueHandling = NullValueHandling.Ignore)]
            public AttributesClass Attributes { get; set; }

            [JsonProperty("metadata", NullValueHandling = NullValueHandling.Ignore)]
            public PurpleMetadata Metadata { get; set; }

            [JsonProperty("expiryDate", NullValueHandling = NullValueHandling.Ignore)]
            public DateTimeOffset? ExpiryDate { get; set; }

            [JsonProperty("stats", NullValueHandling = NullValueHandling.Ignore)]
            public Dictionary<string, Stat> Stats { get; set; }
        }

        public class PurpleMetadata
        {
            [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
            public string Name { get; set; }
        }

        public class Stat
        {
            [JsonProperty("rank")]
            public object Rank { get; set; }

            [JsonProperty("percentile")]
            public long? Percentile { get; set; }

            [JsonProperty("displayName", NullValueHandling = NullValueHandling.Ignore)]
            public string DisplayName { get; set; }

            [JsonProperty("displayCategory", NullValueHandling = NullValueHandling.Ignore)]
            public DisplayCategory? DisplayCategory { get; set; }

            [JsonProperty("category", NullValueHandling = NullValueHandling.Ignore)]
            public Category? Category { get; set; }

            [JsonProperty("metadata", NullValueHandling = NullValueHandling.Ignore)]
            public AttributesClass Metadata { get; set; }

            [JsonProperty("value", NullValueHandling = NullValueHandling.Ignore)]
            public double? Value { get; set; }

            [JsonProperty("displayValue", NullValueHandling = NullValueHandling.Ignore)]
            public string DisplayValue { get; set; }

            [JsonProperty("displayType", NullValueHandling = NullValueHandling.Ignore)]
            public DisplayType? DisplayType { get; set; }
        }

        public enum Category { Combat, General, Objective, Round };

        public enum DisplayCategory { Combat, General, Objective, Round };

        public enum DisplayType { Number, NumberPercentage, NumberPrecision2, TimeSeconds };
    }


}
