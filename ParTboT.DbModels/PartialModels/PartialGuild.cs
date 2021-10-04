using MongoDB.Bson.Serialization.Attributes;
using ParTboT.DbModels.Shared;

namespace ParTboT.DbModels.PartialModels
{
    public record PartialGuild : Identification<ulong>
    {
        public Mention Owner { get; set; }
        public Mention @Everyone { get; set; }

        [BsonIgnore]
        public bool IsEditing { get; set; }
    }
}
