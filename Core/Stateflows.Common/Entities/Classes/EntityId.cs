using System;
using System.Text.Json.Serialization;
using Stateflows.Common.Exceptions;
using Stateflows.Common.Utilities;

namespace Stateflows
{
    /// <summary>Identifies an entity instance by registered entity name and instance key.</summary>
    /// <remarks>An empty instance key denotes the default instance.</remarks>
    public struct EntityId : IEquatable<EntityId>
    {
        /// <summary>Creates an entity identifier from its registered name and instance key.</summary>
        /// <param name="name">The registered entity name.</param>
        /// <param name="instance">The instance key; an empty value denotes the default instance.</param>
        public EntityId(string name, string instance)
        {
            Name = name;
            Instance = instance;
        }

        /// <summary>Creates an entity identifier from a behavior identifier.</summary>
        /// <param name="id">An identifier whose behavior type is Entity.</param>
        /// <exception cref="Stateflows.Common.Exceptions.StateflowsDefinitionException">The identifier does not identify an entity.</exception>
        public EntityId(BehaviorId id)
        {
            if (id.Type != EntityClass.Type)
            {
                throw new StateflowsDefinitionException("BehaviorId doesn't represent Entity");
            }

            Name = id.Name;
            Instance = id.Instance;
        }

        /// <summary>Gets or sets the registered entity name.</summary>
        public string Name { get; set; }

        /// <summary>Gets or sets the entity instance key.</summary>
        public string Instance { get; set; }

        /// <summary>Gets a display-safe instance key, using <c>&lt;default&gt;</c> for an empty key.</summary>
        [Newtonsoft.Json.JsonIgnore]
        [JsonIgnore]
        public string InstanceText => string.IsNullOrEmpty(Instance)
            ? "<default>"
            : Instance;
        
        /// <summary>Gets the entity behavior class represented by this identifier.</summary>
        [Newtonsoft.Json.JsonIgnore]
        [JsonIgnore]
        public readonly EntityClass EntityClass => new EntityClass(Name);

        
        /// <summary>Gets the equivalent behavior identifier.</summary>
        [Newtonsoft.Json.JsonIgnore]
        [JsonIgnore]
        public readonly BehaviorId BehaviorId => new BehaviorId(EntityClass.Type, Name, Instance);

        /// <summary>Compares two entity identifiers for equality.</summary>
        /// <param name="id1">The first identifier.</param>
        /// <param name="id2">The second identifier.</param>
        /// <returns><see langword="true"/> when name and instance key match.</returns>
        public static bool operator ==(EntityId id1, EntityId id2)
            => id1.Equals(id2);

        /// <summary>Compares two entity identifiers for inequality.</summary>
        /// <param name="id1">The first identifier.</param>
        /// <param name="id2">The second identifier.</param>
        /// <returns><see langword="true"/> when the identifiers differ.</returns>
        public static bool operator !=(EntityId id1, EntityId id2)
            => !id1.Equals(id2);

        /// <summary>Compares an entity identifier with a behavior identifier.</summary>
        /// <param name="stateMachineId">The entity identifier.</param>
        /// <param name="behaviorId">The general behavior identifier.</param>
        /// <returns><see langword="true"/> when both identify the same instance.</returns>
        public static bool operator ==(EntityId stateMachineId, BehaviorId behaviorId)
            => stateMachineId.BehaviorId == behaviorId;

        /// <summary>Determines whether an entity identifier differs from a behavior identifier.</summary>
        /// <param name="stateMachineId">The entity identifier.</param>
        /// <param name="behaviorId">The general behavior identifier.</param>
        /// <returns><see langword="true"/> when they identify different instances.</returns>
        public static bool operator !=(EntityId stateMachineId, BehaviorId behaviorId)
            => stateMachineId.BehaviorId != behaviorId;

        /// <summary>Compares a behavior identifier with an entity identifier.</summary>
        /// <param name="behaviorId">The general behavior identifier.</param>
        /// <param name="stateMachineId">The entity identifier.</param>
        /// <returns><see langword="true"/> when both identify the same instance.</returns>
        public static bool operator ==(BehaviorId behaviorId, EntityId stateMachineId)
            => behaviorId == stateMachineId.BehaviorId;

        /// <summary>Determines whether a behavior identifier differs from an entity identifier.</summary>
        /// <param name="behaviorId">The general behavior identifier.</param>
        /// <param name="stateMachineId">The entity identifier.</param>
        /// <returns><see langword="true"/> when they identify different instances.</returns>
        public static bool operator !=(BehaviorId behaviorId, EntityId stateMachineId)
            => behaviorId != stateMachineId.BehaviorId;

        /// <summary>Converts an entity identifier to its behavior identifier.</summary>
        /// <param name="stateMachineId">The entity identifier.</param>
        /// <returns>The equivalent behavior identifier.</returns>
        public static implicit operator BehaviorId(EntityId stateMachineId)
            => stateMachineId.BehaviorId;

        /// <summary>Converts a behavior identifier to an entity identifier.</summary>
        /// <param name="behaviorId">The behavior identifier to convert.</param>
        /// <returns>The equivalent entity identifier.</returns>
        /// <exception cref="Stateflows.Common.Exceptions.StateflowsDefinitionException">The identifier does not identify an entity.</exception>
        public static implicit operator EntityId(BehaviorId behaviorId)
            => new EntityId(behaviorId);

        /// <summary>Determines whether this identifier equals the specified object.</summary>
        /// <param name="obj">The object to compare.</param>
        /// <returns><see langword="true"/> when <paramref name="obj"/> is an entity identifier with the same name and instance.</returns>
        public readonly override bool Equals(object obj)
        {
            return obj is EntityId other && Equals(other);
        }

        /// <summary>Returns a hash code for this identifier.</summary>
        /// <returns>A hash code based on the entity name and instance.</returns>
        public readonly override int GetHashCode()
        {
            return HashCode.Combine(Name, Instance);
        }

        /// <summary>Serializes this identifier using the Stateflows JSON representation.</summary>
        /// <returns>The serialized entity identifier.</returns>
        public readonly override string ToString()
            => StateflowsJsonConverter.SerializeObject(this);

        /// <summary>Converts an entity identifier to its serialized representation.</summary>
        /// <param name="stateMachineId">The entity identifier to serialize.</param>
        /// <returns>The Stateflows JSON representation.</returns>
        public static implicit operator string(EntityId stateMachineId)
            => StateflowsJsonConverter.SerializeObject(stateMachineId);

        /// <summary>Determines whether this identifier equals another entity identifier.</summary>
        /// <param name="other">The identifier to compare.</param>
        /// <returns><see langword="true"/> when name and instance key match.</returns>
        public bool Equals(EntityId other)
            => Name == other.Name && Instance == other.Instance;
    }
}
