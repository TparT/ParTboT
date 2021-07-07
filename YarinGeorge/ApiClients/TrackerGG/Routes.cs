namespace YarinGeorge.ApiClients.TrackerGG
{
    public record BaseRoutes
    {
        public const string Version = "v2";
        public const string BaseURL = "https://public-api.tracker.gg/" + Version;

        public const string Standard = "/standard";

        public const string Profile = "/profile";
        public const string Search = "/search";
    }

    public record GameRoutes
    {
        public const string ApexLegends = "/apex";
        public const string CSGO = "/csgo";
        public const string Devision2 = "/division-2";
        public const string Overwatch = "/overwatch";
        public const string Splitgate = "/splitgate";
        public const string HyperScape = "/hyper-scape";
    }

    public enum Game
    {
        ApexLegends,
        CSGO,
        Devision2,
        Overwatch,
        Splitgate,
        HyperScape
    }

    public record PlatformRoutes
    {
        public const string Battlenet = "/battlenet";
        public const string Origin = "/origin";
        public const string Playstation = "/psn";
        public const string Steam = "/steam";
        public const string Uplay = "/uplay";
        public const string XboxLive = "/xbl";
    }
}
