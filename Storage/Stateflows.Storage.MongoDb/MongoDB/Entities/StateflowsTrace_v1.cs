using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Stateflows.Storage.MongoDB.MongoDB.Entities
{
    internal class StateflowsTrace_v1
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public string BehaviorId { get; set; }
        public DateTime ExecutionTime { get; set; }
        public string Data { get; set; }
        public StateflowsTrace_v1(BehaviorId behaviorId, DateTime executionTime, string data)
        {
            BehaviorId = behaviorId;
            ExecutionTime = executionTime;
            Data = data;
        }
    }
}
