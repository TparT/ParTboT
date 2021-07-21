using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;
using System.Collections.Generic;

namespace ParTboT.DbModels.ParTboTModels
{
    /// <summary>
    /// Keys are the users Ids and the values are the names of the users that the Id belongs to.
    /// </summary>
    public class SocialsFollows
    {
        public List<KeyValuePair<string, string>> TwitchStreamers { get; set; }
        public List<KeyValuePair<string, string>> YouTubers { get; set; }
        public List<KeyValuePair<string, string>> FloatPlane { get; set; }
        public List<KeyValuePair<string, string>> Twitter { get; set; }
        public List<KeyValuePair<string, string>> Instagram { get; set; }
        public List<KeyValuePair<string, string>> Facebook { get; set; }
        public List<KeyValuePair<string, string>> LinkedIn { get; set; }
    }
}
