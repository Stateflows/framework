using System.Text.Json.Serialization;
using Stateflows.Common.Exceptions;
using Stateflows.Common.Utilities;

namespace Stateflows
{
    public struct AgentId
    {
        public AgentId(string name, string instance)
        {
            Name = name;
            Instance = instance;
        }

        public AgentId(BehaviorId id)
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
        public readonly AgentClass AgentClass => new(Name);

        
        [Newtonsoft.Json.JsonIgnore]
        [JsonIgnore]
        public readonly BehaviorId BehaviorId => new(MAFBehaviorType.AIAgent, Name, Instance);

        public static bool operator ==(AgentId id1, AgentId id2)
            => id1.Equals(id2);

        public static bool operator !=(AgentId id1, AgentId id2)
            => !id1.Equals(id2);

        public static bool operator ==(AgentId id1, BehaviorId id2)
            => id1.BehaviorId == id2;

        public static bool operator !=(AgentId id1, BehaviorId id2)
            => id1.BehaviorId != id2;

        public static implicit operator BehaviorId(AgentId agentId)
            => agentId.BehaviorId;

        public static implicit operator AgentId(BehaviorId behaviorId)
            => new(behaviorId);

        public readonly override bool Equals(object? obj)
            =>
                obj is AgentId id &&
                Name == id.Name &&
                Instance == id.Instance;

        public readonly override int GetHashCode()
            => Tuple.Create(Name, Instance).GetHashCode();

        public readonly override string ToString()
            => StateflowsJsonConverter.SerializeObject(this);

        public static implicit operator string(AgentId agentId)
            => StateflowsJsonConverter.SerializeObject(agentId);
    }
}
