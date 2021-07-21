using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;
using ParTboT.DbModels.SocialPlatforms.Shared;
using System;
using System.Collections.Generic;

namespace ParTboT.DbModels.SocialPlatforms
{
    public class TwitterTweeter
    {
        [BsonId]
        public long _id { get; set; }
        public string TweeterAccountName { get; set; }
        //public long LastTweetID { get; set; }
        public string UserPageURL { get; set; }
        public string UserProfileImgURL { get; set; }
        [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfDocuments)]
        public Dictionary<string, FollowingGuild> FollowingGuilds { get; set; }
        public DateTime DateTimeAddedToTheDatabase { get; set; }
    }
}
