using System.Text.Json.Serialization;
using Stateflows.Common.Exceptions;
using Stateflows.Common.Utilities;

namespace Stateflows
{
    /// <summary>Identifies one Microsoft Agent Framework behavior instance.</summary>
    public struct AIAgentId
    {
        /// <summary>Creates an agent identifier from its registered type name and instance key.</summary>
        /// <param name="name">The agent behavior name.</param>
        /// <param name="instance">The agent instance key; empty identifies the default instance.</param>
        public AIAgentId(string name, string instance)
        {
            Name = name;
            Instance = instance;
        }

        /// <summary>Creates an agent identifier from a Stateflows behavior identifier.</summary>
        /// <param name="id">A behavior identifier whose type is the Microsoft Agent Framework agent type.</param>
        /// <exception cref="Stateflows.Common.Exceptions.StateflowsDefinitionException">The behavior identifier does not identify a Microsoft Agent Framework agent.</exception>
        public AIAgentId(BehaviorId id)
        {
            if (id.Type != MAFBehaviorType.AIAgent)
            {
                throw new StateflowsDefinitionException($"BehaviorId doesn't represent {MAFBehaviorType.AIAgent}");
            }

            Name = id.Name;
            Instance = id.Instance;
        }

        /// <summary>Gets or sets the registered agent behavior name.</summary>
        public string Name { get; set; }

        /// <summary>Gets or sets the agent instance key.</summary>
        public string Instance { get; set; }

        /// <summary>Gets the instance key formatted for display, using <c>&lt;default&gt;</c> for the default instance.</summary>
        [Newtonsoft.Json.JsonIgnore]
        [JsonIgnore]
        public string InstanceText => string.IsNullOrEmpty(Instance)
            ? "<default>"
            : Instance;
        
        /// <summary>Gets the behavior class represented by this identifier.</summary>
        [Newtonsoft.Json.JsonIgnore]
        [JsonIgnore]
        public readonly AIAgentClass AiAgentClass => new(Name);

        
        /// <summary>Gets the equivalent Stateflows behavior identifier.</summary>
        [Newtonsoft.Json.JsonIgnore]
        [JsonIgnore]
        public readonly BehaviorId BehaviorId => new(MAFBehaviorType.AIAgent, Name, Instance);

        /// <summary>Compares two agent identifiers.</summary>
        /// <param name="id1">The first identifier.</param>
        /// <param name="id2">The second identifier.</param>
        /// <returns><see langword="true"/> when both name and instance match.</returns>
        public static bool operator ==(AIAgentId id1, AIAgentId id2)
            => id1.Equals(id2);

        /// <summary>Compares two agent identifiers for inequality.</summary>
        /// <param name="id1">The first identifier.</param>
        /// <param name="id2">The second identifier.</param>
        /// <returns><see langword="true"/> when either name or instance differs.</returns>
        public static bool operator !=(AIAgentId id1, AIAgentId id2)
            => !id1.Equals(id2);

        /// <summary>Compares an agent identifier with a Stateflows behavior identifier.</summary>
        /// <param name="id1">The agent identifier.</param>
        /// <param name="id2">The behavior identifier.</param>
        /// <returns><see langword="true"/> when both identify the same behavior instance.</returns>
        public static bool operator ==(AIAgentId id1, BehaviorId id2)
            => id1.BehaviorId == id2;

        /// <summary>Compares an agent identifier with a Stateflows behavior identifier for inequality.</summary>
        /// <param name="id1">The agent identifier.</param>
        /// <param name="id2">The behavior identifier.</param>
        /// <returns><see langword="true"/> when they identify different behavior instances.</returns>
        public static bool operator !=(AIAgentId id1, BehaviorId id2)
            => id1.BehaviorId != id2;

        /// <summary>Converts an agent identifier to its Stateflows behavior identifier.</summary>
        /// <param name="aiAgentId">The agent identifier.</param>
        /// <returns>The equivalent behavior identifier.</returns>
        public static implicit operator BehaviorId(AIAgentId aiAgentId)
            => aiAgentId.BehaviorId;

        /// <summary>Converts a Stateflows behavior identifier to an agent identifier.</summary>
        /// <param name="behaviorId">The behavior identifier to convert.</param>
        /// <returns>The equivalent agent identifier.</returns>
        public static implicit operator AIAgentId(BehaviorId behaviorId)
            => new(behaviorId);

        /// <summary>Determines whether the specified object is an agent identifier with matching name and instance.</summary>
        /// <param name="obj">The object to compare.</param>
        /// <returns><see langword="true"/> when the object is an equal <see cref="AIAgentId"/>.</returns>
        public readonly override bool Equals(object? obj)
            =>
                obj is AIAgentId id &&
                Name == id.Name &&
                Instance == id.Instance;

        /// <summary>Returns a hash code based on this agent name and instance key.</summary>
        /// <returns>The identifier's hash code.</returns>
        public readonly override int GetHashCode()
            => Tuple.Create(Name, Instance).GetHashCode();

        /// <summary>Serializes this identifier using the Stateflows JSON converter.</summary>
        /// <returns>The serialized agent identifier.</returns>
        public readonly override string ToString()
            => StateflowsJsonConverter.SerializeObject(this);

        /// <summary>Converts the identifier to its Stateflows JSON representation.</summary>
        /// <param name="aiAgentId">The identifier to serialize.</param>
        /// <returns>The serialized agent identifier.</returns>
        public static implicit operator string(AIAgentId aiAgentId)
            => StateflowsJsonConverter.SerializeObject(aiAgentId);
    }
}
