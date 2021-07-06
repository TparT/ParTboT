using Newtonsoft.Json;

namespace ParTboT
{
    public struct ConfigJson
    {
        [JsonProperty("token")]
        public string Token { get; private set; }

        [JsonProperty("csprefix")]
        public string Prefix { get; private set; }

        [JsonProperty("uniprefix")]
        public bool UniversalPrefix { get; private set; }

        [JsonProperty("pymodules")]
        public bool Pymodules { get; private set; }

        [JsonProperty("MongoDBAtlasConnectionString")]
        public string mongoDBAtlasConnectionString { get; private set; }

        [JsonProperty("LocalMongoDBConnectionString")]
        public string LocalMongoDBConnectionString { get; private set; }

        [JsonProperty("LocalMongoDBName")]
        public string LocalMongoDBName { get; private set; }

        [JsonProperty("LocalMongoDB_Streamers")]
        public string LocalMongoDB_Streamers { get; private set; }

        [JsonProperty("CurrencyConverterAPIKey")]
        public string CurrencyConverterAPIKey { get; private set; }

        [JsonProperty("TwitchAPI_ClientID")]
        public string TwitchAPI_ClientID { get; private set; }

        [JsonProperty("TwitchAPI_AccessToken")]
        public string TwitchAPI_AccessToken { get; private set; }

        [JsonProperty("GeniusAPI_ApiKey")]
        public string GeniusAPI_ApiKey { get; private set; }

        [JsonProperty("TwitterAPI_ApiKey")]
        public string TwitterAPI_ApiKey { get; private set; }

        [JsonProperty("TwitterAPI_SecretKey")]
        public string TwitterAPI_SecretKey { get; private set; }

        [JsonProperty("TwitterAPIUser_AccessToken")]
        public string TwitterAPIUser_AccessToken { get; private set; }

        [JsonProperty("TwitterAPIUser_AccessTokenSecret")]
        public string TwitterAPIUser_AccessTokenSecret { get; private set; }
    }
}
