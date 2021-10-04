using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;
using ParTboT.DbModels.SocialPlatforms.CustomMessages;
using ParTboT.DbModels.SocialPlatforms.Shared;
using System;
using System.Collections.Generic;

namespace ParTboT.DbModels.SocialPlatforms
{
    public record TwitterTweeter
    {
        [BsonId]
        public long _id { get; set; }
        public string TweeterAccountName { get; set; }
        public string UserPageURL { get; set; }
        public string UserProfileImgURL { get; set; }
        [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfDocuments)]
        public Dictionary<string, FollowingGuild<TwitterCustomMessage>> FollowingGuilds { get; set; }
        public DateTime DateTimeAddedToTheDatabase { get; set; }

        [BsonIgnore]
        public bool IsEditing { get; set; }
    }
}
