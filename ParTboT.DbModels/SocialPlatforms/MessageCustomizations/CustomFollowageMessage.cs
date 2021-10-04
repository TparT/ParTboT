using DSharpPlus.Entities;
using MongoDB.Bson.Serialization.Attributes;
using ParTboT.DbModels.PartialModels;
using System.Collections.Generic;

namespace ParTboT.DbModels.SocialPlatforms.CustomMessages
{
    public record CustomFollowageMessage
    {
        public PartialChannel ChannelToSendTo { get; set; }
        public string CustomText { get; set; }
        public int EmbedColor { get; set; }
        public bool ShowProfilePicture { get; set; }
        public bool SetEmbedTitleLinkToTheRelevantThing { get; set; }
        public List<DiscordEmoji> DummyReactions { get; set; }

        [BsonIgnore]
        public bool IsEditing { get; set; }
    }
}
