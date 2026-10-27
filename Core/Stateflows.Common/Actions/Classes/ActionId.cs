using System;
using System.Text.Json.Serialization;
using Stateflows.Common.Exceptions;
using Stateflows.Common.Utilities;

namespace Stateflows
{
    /// <summary>Identifies an action instance by its definition name and instance key.</summary>
    /// <remarks>Converts to <see cref="BehaviorId"/> for general behavior APIs. See <see href="https://github.com/Stateflows/framework/wiki/behaviors">behaviors</see>.</remarks>
    public struct ActionId : IEquatable<ActionId>
    {
        /// <summary>Creates an action identifier.</summary>
        /// <param name="name">The registered action definition name.</param>
        /// <param name="instance">The instance key; an empty key denotes the default instance.</param>
        public ActionId(string name, string instance)
        {
            Name = name;
            Instance = instance;
        }

        /// <summary>Creates an action identifier from a general behavior identifier.</summary>
        /// <param name="id">The identifier to interpret as an action.</param>
        /// <exception cref="StateflowsDefinitionException">The identifier denotes an activity, entity, or state machine.</exception>
        public ActionId(BehaviorId id)
        {
            if (id.Type is BehaviorType.Activity or BehaviorType.Entity or BehaviorType.StateMachine)
            {
                throw new StateflowsDefinitionException("BehaviorId doesn't represent Action");
            }

            Name = id.Name;
            Instance = id.Instance;
        }

        /// <summary>Gets or sets the registered action definition name.</summary>
        public string Name { get; set; }

        /// <summary>Gets or sets the action instance key.</summary>
        public string Instance { get; set; }

        /// <summary>Gets the instance key, or <c>&lt;default&gt;</c> when it is null or empty.</summary>
        [Newtonsoft.Json.JsonIgnore]
        [JsonIgnore]
        public string InstanceText => string.IsNullOrEmpty(Instance)
            ? "<default>"
            : Instance;
        
        /// <summary>Gets the action class represented by this identifier.</summary>
        [Newtonsoft.Json.JsonIgnore]
        [JsonIgnore]
        public readonly ActionClass ActionClass => new ActionClass(Name);

        
        /// <summary>Gets this identifier expressed as an action-typed general behavior identifier.</summary>
        [Newtonsoft.Json.JsonIgnore]
        [JsonIgnore]
        public readonly BehaviorId BehaviorId => new BehaviorId(BehaviorType.Action, Name, Instance);

        /// <summary>Compares action identifiers by definition name and instance key.</summary>
        /// <param name="id1">The first identifier.</param>
        /// <param name="id2">The second identifier.</param>
        /// <returns>Whether both identifiers represent the same action instance.</returns>
        public static bool operator ==(ActionId id1, ActionId id2)
            => id1.Equals(id2);

        /// <summary>Tests whether action identifiers differ by definition name or instance key.</summary>
        /// <param name="id1">The first identifier.</param>
        /// <param name="id2">The second identifier.</param>
        /// <returns>Whether the identifiers differ.</returns>
        public static bool operator !=(ActionId id1, ActionId id2)
            => !id1.Equals(id2);

        /// <summary>Compares an action identifier with a general behavior identifier, including the behavior type.</summary>
        /// <param name="id1">The action identifier.</param>
        /// <param name="id2">The general identifier.</param>
        /// <returns>Whether the identifiers represent the same behavior.</returns>
        public static bool operator ==(ActionId id1, BehaviorId id2)
            => id1.BehaviorId == id2;

        /// <summary>Tests whether an action and general behavior identifier represent different behaviors.</summary>
        /// <param name="id1">The action identifier.</param>
        /// <param name="id2">The general identifier.</param>
        /// <returns>Whether the behavior type, name, or instance differs.</returns>
        public static bool operator !=(ActionId id1, BehaviorId id2)
            => id1.BehaviorId != id2;

        /// <summary>Converts an action identifier to a general behavior identifier.</summary>
        /// <param name="actionId">The action identifier to convert.</param>
        /// <returns>An action-typed behavior identifier with the same name and instance.</returns>
        public static implicit operator BehaviorId(ActionId actionId)
            => actionId.BehaviorId;

        /// <summary>Converts a general behavior identifier to an action identifier.</summary>
        /// <param name="behaviorId">The identifier to convert.</param>
        /// <returns>The action identifier.</returns>
        /// <exception cref="StateflowsDefinitionException">The identifier denotes an activity, entity, or state machine.</exception>
        public static implicit operator ActionId(BehaviorId behaviorId)
            => new ActionId(behaviorId);

        /// <summary>Tests equality with another boxed action identifier.</summary>
        /// <param name="obj">The value to compare.</param>
        /// <returns>Whether the object is an action identifier with the same name and instance.</returns>
        public readonly override bool Equals(object obj)
        {
            return obj is ActionId other && Equals(other);
        }

        /// <summary>Computes a hash from the definition name and instance key.</summary>
        /// <returns>The identifier hash code.</returns>
        public readonly override int GetHashCode()
        {
            return HashCode.Combine(Name, Instance);
        }

        /// <summary>Serializes this action identifier to its JSON representation.</summary>
        /// <returns>The serialized identifier.</returns>
        public readonly override string ToString()
            => StateflowsJsonConverter.SerializeObject(this);

        /// <summary>Serializes an action identifier to JSON.</summary>
        /// <param name="actionId">The identifier to serialize.</param>
        /// <returns>The serialized identifier.</returns>
        public static implicit operator string(ActionId actionId)
            => StateflowsJsonConverter.SerializeObject(actionId);
        
        /// <summary>Creates an action identifier using this identifier's implicit string representation as its name.</summary>
        /// <param name="instance">The new instance key.</param>
        /// <returns>An identifier with this identifier's serialized JSON as its name and the supplied instance key.</returns>
        /// <remarks>The current implementation passes this value through its implicit string conversion; it does not copy <see cref="Name"/>.</remarks>
        public ActionId ToId(string instance)
            => new ActionId(this, instance);

        /// <summary>Compares this identifier by definition name and instance key.</summary>
        /// <param name="other">The action identifier to compare.</param>
        /// <returns>Whether both identifiers refer to the same action instance.</returns>
        public bool Equals(ActionId other)
            => Name == other.Name && Instance == other.Instance;
    }
}
