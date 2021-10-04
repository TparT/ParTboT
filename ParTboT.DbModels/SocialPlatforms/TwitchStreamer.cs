using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;
using ParTboT.DbModels.SocialPlatforms.CustomMessages;
using ParTboT.DbModels.SocialPlatforms.Shared;
using System;
using System.Collections.Generic;

namespace ParTboT.DbModels.SocialPlatforms
{
    /// <summary>
    /// ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    /// <br>
    /// <see cref="string"/> _id
    /// </br>
    /// <br>
    /// <see cref="string"/> StreamerName
    /// </br>
    /// <br>
    /// <see cref="string"/> ChannelURL
    /// </br>
    /// <br>
    /// <see cref="string"/> ChannelIconURL
    /// </br>
    /// <br>
    /// <see cref="List{FollowingGuild}"/> FollowingGuilds
    /// </br>
    /// <br>
    /// <see cref="DateTime"/> DateTimeAddedToTheDatabase
    /// </br>
    /// </summary>
    public record TwitchStreamer
    {
        [BsonId]
        public string _id { get; set; }
        public string StreamerName { get; set; }
        public string ChannelURL { get; set; }
        public string ChannelIconURL { get; set; }
        [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfDocuments)]
        public Dictionary<string, FollowingGuild<TwitchCustomMessage>> FollowingGuilds { get; set; }
        public DateTime DateTimeAddedToTheDatabase { get; set; }

        [BsonIgnore]
        public bool IsEditing { get; set; }
    }
}
