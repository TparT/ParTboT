using MongoDB.Bson.Serialization.Attributes;
using ParTboT.DbModels.SocialPlatforms.Shared;
using System;
using System.Collections.Concurrent;
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
    public class TwitchStreamer
    {
        [BsonId]
        public string _id { get; set; }
        public string StreamerName { get; set; }
        public string ChannelURL { get; set; }
        public string ChannelIconURL { get; set; }
        public List<FollowingGuild> FollowingGuilds { get; set; }
        public DateTime DateTimeAddedToTheDatabase { get; set; }
    }
}
