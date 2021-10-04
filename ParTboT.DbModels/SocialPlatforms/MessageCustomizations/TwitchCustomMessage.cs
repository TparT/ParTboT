namespace ParTboT.DbModels.SocialPlatforms.CustomMessages
{
    public record TwitchCustomMessage : CustomFollowageMessage
    {
        public string PreviewSize { get; set; }
        public bool ShowTitle { get; set; }
        public bool ShowViewersCount { get; set; }
        public bool ShowGame { get; set; }
    }
}
