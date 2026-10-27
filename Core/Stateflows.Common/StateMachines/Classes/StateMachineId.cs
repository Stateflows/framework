using System;
using System.Text.Json.Serialization;
using Stateflows.Common.Exceptions;
using Stateflows.Common.Utilities;

namespace Stateflows
{
    /// <summary>Identifies a state-machine instance by its registered definition name and instance key.</summary>
    /// <remarks>Use the identifier with a state-machine locator to obtain a client handle.</remarks>
    public struct StateMachineId : IEquatable<StateMachineId>
    {
        /// <summary>Creates a state-machine identifier.</summary>
        /// <param name="name">The registered state-machine definition name.</param>
        /// <param name="instance">The instance key; an empty key denotes the default instance.</param>
        public StateMachineId(string name, string instance)
        {
            Name = name;
            Instance = instance;
        }

        /// <summary>Creates a state-machine identifier from a general behavior identifier.</summary>
        /// <param name="id">The state-machine-typed behavior identifier.</param>
        /// <exception cref="StateflowsDefinitionException">The identifier does not denote a state machine.</exception>
        public StateMachineId(BehaviorId id)
        {
            if (id.Type != StateMachineClass.Type)
            {
                throw new StateflowsDefinitionException("BehaviorId doesn't represent State Machine");
            }

            Name = id.Name;
            Instance = id.Instance;
        }

        /// <summary>Gets or sets the registered state-machine definition name.</summary>
        public string Name { get; set; }

        /// <summary>Gets or sets the state-machine instance key.</summary>
        public string Instance { get; set; }

        /// <summary>Gets the instance key, or <c>&lt;default&gt;</c> when it is null or empty.</summary>
        [Newtonsoft.Json.JsonIgnore]
        [JsonIgnore]
        public string InstanceText => string.IsNullOrEmpty(Instance)
            ? "<default>"
            : Instance;
        
        /// <summary>Gets the state-machine class represented by this identifier.</summary>
        [Newtonsoft.Json.JsonIgnore]
        [JsonIgnore]
        public readonly StateMachineClass StateMachineClass => new StateMachineClass(Name);

        
        /// <summary>Gets this identifier expressed as a general behavior identifier.</summary>
        [Newtonsoft.Json.JsonIgnore]
        [JsonIgnore]
        public readonly BehaviorId BehaviorId => new BehaviorId(StateMachineClass.Type, Name, Instance);

        /// <summary>Compares state-machine identifiers by definition name and instance key.</summary>
        /// <param name="id1">The first identifier.</param>
        /// <param name="id2">The second identifier.</param>
        /// <returns>Whether both identifiers represent the same state-machine instance.</returns>
        public static bool operator ==(StateMachineId id1, StateMachineId id2)
            => id1.Equals(id2);

        /// <summary>Tests whether state-machine identifiers differ by name or instance key.</summary>
        /// <param name="id1">The first identifier.</param>
        /// <param name="id2">The second identifier.</param>
        /// <returns>Whether the identifiers differ.</returns>
        public static bool operator !=(StateMachineId id1, StateMachineId id2)
            => !id1.Equals(id2);

        /// <summary>Compares a state-machine identifier with a general identifier, including behavior type.</summary>
        /// <param name="stateMachineId">The state-machine identifier.</param>
        /// <param name="behaviorId">The general identifier.</param>
        /// <returns>Whether both identify the same behavior.</returns>
        public static bool operator ==(StateMachineId stateMachineId, BehaviorId behaviorId)
            => stateMachineId.BehaviorId == behaviorId;

        /// <summary>Tests whether a state-machine and general identifier represent different behaviors.</summary>
        /// <param name="stateMachineId">The state-machine identifier.</param>
        /// <param name="behaviorId">The general identifier.</param>
        /// <returns>Whether the behavior type, name, or instance differs.</returns>
        public static bool operator !=(StateMachineId stateMachineId, BehaviorId behaviorId)
            => stateMachineId.BehaviorId != behaviorId;

        /// <summary>Compares a general identifier with a state-machine identifier, including behavior type.</summary>
        /// <param name="behaviorId">The general identifier.</param>
        /// <param name="stateMachineId">The state-machine identifier.</param>
        /// <returns>Whether both identify the same behavior.</returns>
        public static bool operator ==(BehaviorId behaviorId, StateMachineId stateMachineId)
            => behaviorId == stateMachineId.BehaviorId;

        /// <summary>Tests whether a general and state-machine identifier represent different behaviors.</summary>
        /// <param name="behaviorId">The general identifier.</param>
        /// <param name="stateMachineId">The state-machine identifier.</param>
        /// <returns>Whether the behavior type, name, or instance differs.</returns>
        public static bool operator !=(BehaviorId behaviorId, StateMachineId stateMachineId)
            => behaviorId != stateMachineId.BehaviorId;

        /// <summary>Converts a state-machine identifier to a general identifier.</summary>
        /// <param name="stateMachineId">The identifier to convert.</param>
        /// <returns>A state-machine-typed identifier with the same name and instance key.</returns>
        public static implicit operator BehaviorId(StateMachineId stateMachineId)
            => stateMachineId.BehaviorId;

        /// <summary>Converts a general identifier to a state-machine identifier.</summary>
        /// <param name="behaviorId">The identifier to convert.</param>
        /// <returns>The state-machine identifier.</returns>
        /// <exception cref="StateflowsDefinitionException">The identifier does not denote a state machine.</exception>
        public static implicit operator StateMachineId(BehaviorId behaviorId)
            => new StateMachineId(behaviorId);

        /// <summary>Tests equality with a boxed state-machine identifier.</summary>
        /// <param name="obj">The value to compare.</param>
        /// <returns>Whether the object is a state-machine identifier with the same name and instance.</returns>
        public readonly override bool Equals(object obj)
        {
            return obj is StateMachineId other && Equals(other);
        }

        /// <summary>Computes a hash from the definition name and instance key.</summary>
        /// <returns>The identifier hash code.</returns>
        public readonly override int GetHashCode()
        {
            return HashCode.Combine(Name, Instance);
        }

        /// <summary>Serializes this identifier to JSON.</summary>
        /// <returns>The serialized state-machine identifier.</returns>
        public readonly override string ToString()
            => StateflowsJsonConverter.SerializeObject(this);

        /// <summary>Serializes a state-machine identifier to JSON.</summary>
        /// <param name="stateMachineId">The identifier to serialize.</param>
        /// <returns>The serialized identifier.</returns>
        public static implicit operator string(StateMachineId stateMachineId)
            => StateflowsJsonConverter.SerializeObject(stateMachineId);

        /// <summary>Compares this identifier by definition name and instance key.</summary>
        /// <param name="other">The state-machine identifier to compare.</param>
        /// <returns>Whether both identify the same state-machine instance.</returns>
        public bool Equals(StateMachineId other)
            => Name == other.Name && Instance == other.Instance;
    }
}
