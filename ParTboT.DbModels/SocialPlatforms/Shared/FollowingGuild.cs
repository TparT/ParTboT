using ParTboT.DbModels.PartialModels;
using ParTboT.DbModels.SocialPlatforms.CustomMessages;
using System;

namespace ParTboT.DbModels.SocialPlatforms.Shared
{
    public record FollowingGuild<T> : PartialGuild where T : CustomFollowageMessage
    {
        public T CustomMessage { get; set; }
        public DateTime DateTimeStartedFollowing { get; set; }
    }
}
