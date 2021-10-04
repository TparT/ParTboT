using DSharpPlus.Entities;
using MongoDB.Bson.Serialization.Attributes;
using ParTboT.DbModels.DSharpPlus;
using System;

namespace ParTboT.DbModels.ParTboTModels
{
    public class Reminder
    {
        [BsonId]
        public Guid Id { get; set; }
        public ExtendedMemberModel MemberToRemindTo { get; set; }
        public DiscordGuild? GuildRequestedFrom { get; set; } = null;
        public string Description  { get; set; }
        public DateTime RequestedAt { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public ulong ChannelToSendTo { get; set; }
    }
}
