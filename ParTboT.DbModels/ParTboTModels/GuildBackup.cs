using DSharpPlus.Entities;
using System;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using ParTboT.DbModels.DSharpPlus;

namespace ParTboT.DbModels.ParTboTModels
{
    public class GuildBackup
    {
        [BsonId]
        [BsonRepresentation(BsonType.Int64)]
        //
        // Summary:
        //     Gets the template code.
        public ulong GuildId { get; set; }
        //
        // Summary:
        //     Gets the name of the template.
        public string Name { get; set; }
        //
        // Summary:
        //     Gets the description of the template.
        public string Description { get; set; }

        //
        // Summary:
        //     Gets the creator of the template.
        public ExtendedMemberModel Creator { get; set; }
        //
        // Summary:
        //     Date the template was created.
        public DateTime CreatedAt { get; set; }

        //
        // Summary:
        //     Gets the ID of the source guild.
        public ulong SourceGuildId { get; set; }
        //
        // Summary:
        //     Gets the source guild.
        public ExtendedGuildModel SourceGuild { get; set; }
    }
}
