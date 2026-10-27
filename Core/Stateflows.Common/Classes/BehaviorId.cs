using System;
using System.Text.Json.Serialization;
using Stateflows.Common.Utilities;

namespace Stateflows
{
    /// <summary>
    /// Identifies one behavior instance by its behavior class and instance key.
    /// </summary>
    /// <remarks>See the <see href="https://github.com/Stateflows/framework/wiki/behaviors">behavior documentation</see>.</remarks>
    public struct BehaviorId : IEquatable<BehaviorId>
    {
        /// <summary>Creates an identifier from a behavior class and instance key.</summary>
        /// <param name="behaviorClass">The behavior type and registered name.</param>
        /// <param name="instance">The instance key; an empty value identifies the default instance.</param>
        public BehaviorId(BehaviorClass behaviorClass, string instance)
        {
            BehaviorClass = new BehaviorClass(behaviorClass.Type, behaviorClass.Name);
            Instance = instance;
        }

        /// <summary>Creates an identifier from its type, name, and instance key.</summary>
        /// <param name="type">The behavior kind.</param>
        /// <param name="name">The registered behavior name.</param>
        /// <param name="instance">The instance key; an empty value identifies the default instance.</param>
        public BehaviorId(string type, string name, string instance)
        {
            BehaviorClass = new BehaviorClass(type, name);
            Instance = instance;
        }
        /// <summary>Gets the behavior kind.</summary>
        [Newtonsoft.Json.JsonIgnore]
        [JsonIgnore]
        public readonly string Type => BehaviorClass.Type;

        /// <summary>Gets the registered behavior name.</summary>
        [Newtonsoft.Json.JsonIgnore]
        [JsonIgnore]
        public readonly string Name => BehaviorClass.Name;

        /// <summary>Gets or sets the instance key; an empty value denotes the default instance.</summary>
        public string Instance { get; set; }

        /// <summary>Gets a display-safe instance key, using <c>&lt;default&gt;</c> for an empty key.</summary>
        [Newtonsoft.Json.JsonIgnore]
        [JsonIgnore]
        public string InstanceText => string.IsNullOrEmpty(Instance)
            ? "<default>"
            : Instance;

        /// <summary>Gets or sets the type and name that identify this behavior class.</summary>
        public BehaviorClass BehaviorClass { get; set; }

        /// <summary>Serializes this identifier using the Stateflows JSON representation.</summary>
        /// <returns>The serialized behavior identifier.</returns>
        public readonly override string ToString()
            => StateflowsJsonConverter.SerializeObject(this);

        /// <summary>Converts a behavior identifier to its serialized representation.</summary>
        /// <param name="behaviorId">The identifier to serialize.</param>
        /// <returns>The Stateflows JSON representation.</returns>
        public static implicit operator string(BehaviorId behaviorId)
            => StateflowsJsonConverter.SerializeObject(behaviorId);

        /// <summary>Compares two behavior identifiers for equality.</summary>
        /// <param name="id1">The first identifier.</param>
        /// <param name="id2">The second identifier.</param>
        /// <returns><see langword="true"/> when the identifiers match.</returns>
        public static bool operator ==(BehaviorId id1, BehaviorId id2)
            => id1.Equals(id2);

        /// <summary>Compares two behavior identifiers for inequality.</summary>
        /// <param name="id1">The first identifier.</param>
        /// <param name="id2">The second identifier.</param>
        /// <returns><see langword="true"/> when the identifiers differ.</returns>
        public static bool operator !=(BehaviorId id1, BehaviorId id2)
            => !id1.Equals(id2);

        /// <summary>Determines whether this identifier equals the specified object.</summary>
        /// <param name="obj">The object to compare.</param>
        /// <returns><see langword="true"/> when <paramref name="obj"/> is an equal behavior identifier.</returns>
        public readonly override bool Equals(object obj)
        {
            return obj is BehaviorId other && Equals(other);
        }

        /// <summary>Returns a hash code for this identifier.</summary>
        /// <returns>A hash code based on the behavior class and instance.</returns>
        public readonly override int GetHashCode()
            => HashCode.Combine(Instance, BehaviorClass);

        /// <summary>Determines whether this identifier equals another identifier.</summary>
        /// <param name="other">The identifier to compare.</param>
        /// <returns><see langword="true"/> when both the behavior class and instance key match.</returns>
        public bool Equals(BehaviorId other)
            => Instance == other.Instance && BehaviorClass.Equals(other.BehaviorClass);
    }
}
