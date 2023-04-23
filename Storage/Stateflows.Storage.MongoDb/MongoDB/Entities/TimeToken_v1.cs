using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Stateflows.Storage.MongoDB.MongoDB.Entities
{
    internal class TimeToken_v1
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public string BehaviorClass { get; set; }
        public string Data { get; set; }
        public TimeToken_v1(string behaviorClass, string data)
        {
            BehaviorClass = behaviorClass;
            Data = data;
        }
    }
}
