using System;
using System.Text.Json.Serialization;
using Stateflows.Common;
using Stateflows.Common.Exceptions;
using Stateflows.Common.Utilities;

namespace Stateflows
{
    /// <summary>Identifies an activity instance by its registered definition name and instance key.</summary>
    /// <remarks>Use the identifier with an activity locator to obtain a client handle.</remarks>
    public struct ActivityId : IEquatable<ActivityId>
    {
        /// <summary>Creates an activity identifier.</summary>
        /// <param name="name">The registered activity definition name.</param>
        /// <param name="instance">The instance key; an empty key denotes the default instance.</param>
        public ActivityId(string name, string instance)
        {
            Name = name;
            Instance = instance;
        }

        /// <summary>Creates an activity identifier from a general behavior identifier.</summary>
        /// <param name="id">The activity-typed behavior identifier.</param>
        /// <exception cref="StateflowsDefinitionException">The identifier's type is not an activity.</exception>
        public ActivityId(BehaviorId id)
        {
            if (id.Type != BehaviorType.Activity)
            {
                throw new StateflowsDefinitionException("BehaviorId doesn't represent Activity");
            }

            Name = id.Name;
            Instance = id.Instance;
        }

        /// <summary>Gets or sets the registered activity definition name.</summary>
        public string Name { get; set; }

        /// <summary>Gets or sets the activity instance key.</summary>
        public string Instance { get; set; }

        /// <summary>Gets the instance key, or <c>&lt;default&gt;</c> when it is null or empty.</summary>
        [Newtonsoft.Json.JsonIgnore]
        [JsonIgnore]
        public string InstanceText => string.IsNullOrEmpty(Instance)
            ? "<default>"
            : Instance;
        
        /// <summary>Gets the activity class represented by this identifier.</summary>
        [Newtonsoft.Json.JsonIgnore]
        [JsonIgnore]
        public readonly ActivityClass ActivityClass => new ActivityClass(Name);
        
        /// <summary>Gets this identifier expressed as an activity-typed general behavior identifier.</summary>
        [Newtonsoft.Json.JsonIgnore]
        [JsonIgnore]
        public readonly BehaviorId BehaviorId => new BehaviorId(BehaviorType.Activity, Name, Instance);

        /// <summary>Compares activity identifiers by definition name and instance key.</summary>
        /// <param name="id1">The first identifier.</param>
        /// <param name="id2">The second identifier.</param>
        /// <returns>Whether the identifiers represent the same activity instance.</returns>
        public static bool operator ==(ActivityId id1, ActivityId id2)
            => id1.Equals(id2);

        /// <summary>Tests whether activity identifiers differ by definition name or instance key.</summary>
        /// <param name="id1">The first identifier.</param>
        /// <param name="id2">The second identifier.</param>
        /// <returns>Whether the identifiers differ.</returns>
        public static bool operator !=(ActivityId id1, ActivityId id2)
            => !id1.Equals(id2);

        /// <summary>Compares an activity identifier with a general identifier, including the behavior type.</summary>
        /// <param name="id1">The activity identifier.</param>
        /// <param name="id2">The general behavior identifier.</param>
        /// <returns>Whether the identifiers represent the same behavior.</returns>
        public static bool operator ==(ActivityId id1, BehaviorId id2)
            => id1.BehaviorId == id2;

        /// <summary>Tests whether an activity and general identifier represent different behaviors.</summary>
        /// <param name="id1">The activity identifier.</param>
        /// <param name="id2">The general behavior identifier.</param>
        /// <returns>Whether the behavior type, name, or instance differs.</returns>
        public static bool operator !=(ActivityId id1, BehaviorId id2)
            => id1.BehaviorId != id2;

        /// <summary>Converts an activity identifier to a general behavior identifier.</summary>
        /// <param name="activityId">The identifier to convert.</param>
        /// <returns>An activity-typed identifier with the same name and instance key.</returns>
        public static implicit operator BehaviorId(ActivityId activityId)
            => activityId.BehaviorId;

        /// <summary>Converts a general behavior identifier to an activity identifier.</summary>
        /// <param name="behaviorId">The identifier to convert.</param>
        /// <returns>The activity identifier.</returns>
        /// <exception cref="StateflowsDefinitionException">The identifier's type is not an activity.</exception>
        public static implicit operator ActivityId(BehaviorId behaviorId)
            => new ActivityId(behaviorId);

        /// <summary>Tests equality with a boxed activity identifier.</summary>
        /// <param name="obj">The value to compare.</param>
        /// <returns>Whether the object is an activity identifier with the same name and instance.</returns>
        public readonly override bool Equals(object obj)
        {
            return obj is ActivityId other && Equals(other);
        }

        /// <summary>Computes a hash from the activity name and instance key.</summary>
        /// <returns>The identifier hash code.</returns>
        public readonly override int GetHashCode()
        {
            return HashCode.Combine(Name, Instance);
        }

        /// <summary>Serializes this activity identifier to JSON.</summary>
        /// <returns>The serialized identifier.</returns>
        public readonly override string ToString()
            => StateflowsJsonConverter.SerializeObject(this);

        /// <summary>Serializes an activity identifier to JSON.</summary>
        /// <param name="activityId">The identifier to serialize.</param>
        /// <returns>The serialized identifier.</returns>
        public static implicit operator string(ActivityId activityId)
            => StateflowsJsonConverter.SerializeObject(activityId);

        /// <summary>Compares this identifier by activity name and instance key.</summary>
        /// <param name="other">The identifier to compare.</param>
        /// <returns>Whether both identifiers represent the same activity instance.</returns>
        public bool Equals(ActivityId other)
            => Name == other.Name && Instance == other.Instance;
    }
}
