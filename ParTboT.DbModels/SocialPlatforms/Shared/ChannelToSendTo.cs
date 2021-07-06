using MongoDB.Bson.Serialization.Attributes;
using System;

namespace ParTboT.DbModels.SocialPlatforms.Shared
{
    public class ChannelToSendTo
    {
        [BsonId]
        public ulong ChannelIDToSend { get; set; }
        public string ChannelNameToSend { get; set; }
        public string CustomMessage { get; set; }
        public DateTime DateTimeSetThisAlertsChannel { get; set; }
    }
}
