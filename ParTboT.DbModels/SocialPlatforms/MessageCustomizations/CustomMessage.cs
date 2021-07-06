using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParTboT.DbModels.SocialPlatforms.MessageCustomizations
{
    public interface CustomFollowageMessage
    {
        public string CustomText { get; set; }
        public int EmbedColor { get; set; }
        public bool ShowProfilePicture { get; set; }
        public bool SetEmbedTitleLinkToTheRelevantThing { get; set; }
        public List<DiscordEmoji> DummyReactions { get; set; }
    }
}
