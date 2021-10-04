namespace ParTboT.DbModels.SocialPlatforms.CustomMessages
{
    public record TwitterCustomMessage : CustomFollowageMessage
    {
        public bool PreviewMedia { get; set; }
    }
}
