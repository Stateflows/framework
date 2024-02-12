using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Stateflows.Storage.MongoDB.MongoDB.Entities
{
    internal class StateflowsContext_v1
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public string BehaviorId { get; set; }
        public string BehaviorClass { get; set; }
        public DateTime? TriggerTime { get; set; }
        public string Data { get; set; }
        public StateflowsContext_v1(BehaviorId behaviorId)
        {
            BehaviorId = behaviorId;
            BehaviorClass = behaviorId.BehaviorClass;
            Data = string.Empty;
        }
    }
}
