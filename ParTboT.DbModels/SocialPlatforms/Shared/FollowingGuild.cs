using MongoDB.Bson.Serialization.Attributes;
using ParTboT.DbModels.SocialPlatforms.MessageCustomizations;
using System;
using System.Collections.Generic;

namespace ParTboT.DbModels.SocialPlatforms.Shared
{
    public class FollowingGuild
    {
        [BsonId]
        public ulong GuildIDToSend { get; set; }
        public string GuildNameToSend { get; set; }
        public ChannelToSendTo ChannelToSendTo { get; set; } // with the new update?
        public DateTime DateTimeStartedFollowing { get; set; }
        public CustomFollowageMessage MessageCustomizationsSettings { get; set; }
        //public int EmbedColor { get; set; } = 0;
    }
}
