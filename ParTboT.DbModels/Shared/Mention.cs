namespace ParTboT.DbModels.Shared
{
    public record Mention
    {
        public ulong Id { get; set; }
        public string MentionString { get; set; }

        public Mention(ulong id, string mentionString)
        {
            Id = id;
            MentionString = mentionString;
        }
    }
}
