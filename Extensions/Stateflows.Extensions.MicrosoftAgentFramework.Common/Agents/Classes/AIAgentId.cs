using System.Text.Json.Serialization;
using Stateflows.Common.Exceptions;
using Stateflows.Common.Utilities;

namespace Stateflows
{
    public struct AIAgentId
    {
        public AIAgentId(string name, string instance)
        {
            Name = name;
            Instance = instance;
        }

        public AIAgentId(BehaviorId id)
        {
            if (id.Type != MAFBehaviorType.AIAgent)
            {
                throw new StateflowsDefinitionException($"BehaviorId doesn't represent {MAFBehaviorType.AIAgent}");
            }

            Name = id.Name;
            Instance = id.Instance;
        }

        public string Name { get; set; }

        public string Instance { get; set; }

        [Newtonsoft.Json.JsonIgnore]
        [JsonIgnore]
        public string InstanceText => string.IsNullOrEmpty(Instance)
            ? "<default>"
            : Instance;
        
        [Newtonsoft.Json.JsonIgnore]
        [JsonIgnore]
        public readonly AIAgentClass AiAgentClass => new(Name);

        
        [Newtonsoft.Json.JsonIgnore]
        [JsonIgnore]
        public readonly BehaviorId BehaviorId => new(MAFBehaviorType.AIAgent, Name, Instance);

        public static bool operator ==(AIAgentId id1, AIAgentId id2)
            => id1.Equals(id2);

        public static bool operator !=(AIAgentId id1, AIAgentId id2)
            => !id1.Equals(id2);

        public static bool operator ==(AIAgentId id1, BehaviorId id2)
            => id1.BehaviorId == id2;

        public static bool operator !=(AIAgentId id1, BehaviorId id2)
            => id1.BehaviorId != id2;

        public static implicit operator BehaviorId(AIAgentId aiAgentId)
            => aiAgentId.BehaviorId;

        public static implicit operator AIAgentId(BehaviorId behaviorId)
            => new(behaviorId);

        public readonly override bool Equals(object? obj)
            =>
                obj is AIAgentId id &&
                Name == id.Name &&
                Instance == id.Instance;

        public readonly override int GetHashCode()
            => Tuple.Create(Name, Instance).GetHashCode();

        public readonly override string ToString()
            => StateflowsJsonConverter.SerializeObject(this);

        public static implicit operator string(AIAgentId aiAgentId)
            => StateflowsJsonConverter.SerializeObject(aiAgentId);
    }
}
