using Newtonsoft.Json;

namespace MongoSync
{
    public struct ConfigJson
    {
        [JsonProperty("FromDatabaseConnectionString")]
        public string FromDatabaseConnectionString { get; private set; }
        
        [JsonProperty("FromDatabaseName")]
        public string FromDatabaseName { get; private set; }
        
        [JsonProperty("FromCollectionName")]
        public string FromCollectionName { get; private set; }
        
        ////////////////////////////////////////////////////////////////////
        
        [JsonProperty("ToDatabaseConnectionString")]
        public string ToDatabaseConnectionString { get; private set; }
        
        [JsonProperty("ToDatabaseName")]
        public string ToDatabaseName { get; private set; }
        
        [JsonProperty("ToCollectionName")]
        public string ToCollectionName { get; private set; }
        
        ////////////////////////////////////////////////////////////////////
        
        [JsonProperty("BackupIntervalInMinutes")]
        public int BackupIntervalInMinutes { get; private set; }
    }
}
