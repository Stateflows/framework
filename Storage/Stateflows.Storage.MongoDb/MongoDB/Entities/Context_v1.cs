using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Stateflows.Storage.MongoDB.MongoDB.Entities
{
    internal class Context_v1
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public string BehaviorId { get; set; }
        public string Data { get; set; }
        public Context_v1(string behaviorId, string data)
        {
            BehaviorId = behaviorId;
            Data = data;
        }
    }
}
