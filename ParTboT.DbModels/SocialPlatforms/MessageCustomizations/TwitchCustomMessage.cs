using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParTboT.DbModels.SocialPlatforms.MessageCustomizations
{
    public class TwitchCustomMessage : CustomFollowageMessage
    {
        public string CustomText { get; set; }
        public string PreviewSize { get; set; }
        public int EmbedColor { get; set; }
        public bool ShowTitle { get; set; }
        public bool ShowViewersCount { get; set; }
        public bool ShowGame { get; set; }
        public bool ShowProfilePicture { get; set; }
        public bool SetEmbedTitleLinkToTheRelevantThing { get; set; }
        public List<DiscordEmoji> DummyReactions { get; set; }
    }
}
