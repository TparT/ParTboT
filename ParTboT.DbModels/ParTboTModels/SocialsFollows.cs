using System.Collections.Generic;

namespace ParTboT.DbModels.ParTboTModels
{
    /// <summary>
    /// Keys are the users Ids and the values are the names of the users that the Id belongs to.
    /// </summary>
    public class SocialsFollows
    {
        public Dictionary<string, string> TwitchStreamers { get; set; } = null;
        public Dictionary<string, string> YouTubers { get; set; } = null;
        public Dictionary<string, string> FloatPlane { get; set; } = null;
        public Dictionary<string, string> Twitter { get; set; } = null;
        public Dictionary<string, string> Instagram { get; set; } = null;
        public Dictionary<string, string> Facebook { get; set; } = null;
        public Dictionary<string, string> LinkedIn { get; set; } = null;
    }
}
