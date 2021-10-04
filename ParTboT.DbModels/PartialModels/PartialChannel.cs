using MongoDB.Bson.Serialization.Attributes;

namespace ParTboT.DbModels.PartialModels
{
    public record PartialChannel
    {
        public ulong Id { get; set; }
        public string Name { get; set; }

        [BsonIgnore]
        public bool IsEditing { get; set; }
    }
}
