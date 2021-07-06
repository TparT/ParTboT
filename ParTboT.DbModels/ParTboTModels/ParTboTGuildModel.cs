using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace ParTboT.DbModels.ParTboTModels
{
    public class ParTboTGuildModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.Int64)]
        public ulong Id { get; set; }
        public string Name { get; set; }
        public int MemberCount { get; set; }
        public string[] Prefixes { get; set; }
        public SocialsFollows SocialsFollows { get; set; }
        public SortedList<long, GuildBackup> GuildBackups { get; set; }
        public bool VerifyMembersOnJoin { get; set; }
    }
}
