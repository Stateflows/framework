using System;
using Stateflows.Common.Utilities;

namespace Stateflows
{
    /// <summary>Identifies a registered behavior definition by its kind and name.</summary>
    /// <remarks>Use <see cref="ToId(string)"/> to identify an instance of this class.</remarks>
    /// <param name="type">The behavior kind.</param>
    /// <param name="name">The registered behavior name.</param>
    public struct BehaviorClass(string type, string name) : IEquatable<BehaviorClass>
    {
        /// <summary>Gets or sets the behavior kind.</summary>
        public string Type { get; set; } = type;

        /// <summary>Gets or sets the registered behavior name.</summary>
        public string Name { get; set; } = name;

        /// <summary>Compares two behavior classes for equality.</summary>
        /// <param name="class1">The first behavior class.</param>
        /// <param name="class2">The second behavior class.</param>
        /// <returns><see langword="true"/> when both kind and name match.</returns>
        public static bool operator ==(BehaviorClass class1, BehaviorClass class2)
            => class1.Equals(class2);

        /// <summary>Compares two behavior classes for inequality.</summary>
        /// <param name="class1">The first behavior class.</param>
        /// <param name="class2">The second behavior class.</param>
        /// <returns><see langword="true"/> when the classes differ.</returns>
        public static bool operator !=(BehaviorClass class1, BehaviorClass class2)
            => !class1.Equals(class2);

        /// <summary>Determines whether this class equals the specified object.</summary>
        /// <param name="obj">The object to compare.</param>
        /// <returns><see langword="true"/> when <paramref name="obj"/> is an equal behavior class.</returns>
        public readonly override bool Equals(object obj)
        {
            return obj is BehaviorClass other && Equals(other);
        }

        /// <summary>Returns a hash code for this behavior class.</summary>
        /// <returns>A hash code based on the behavior kind and name.</returns>
        public readonly override int GetHashCode()
        {
            return HashCode.Combine(Type, Name);
        }

        /// <summary>Serializes this class using the Stateflows JSON representation.</summary>
        /// <returns>The serialized behavior class.</returns>
        public readonly override string ToString()
            => StateflowsJsonConverter.SerializeObject(this);

        /// <summary>Converts a behavior class to its serialized representation.</summary>
        /// <param name="behaviorClass">The class to serialize.</param>
        /// <returns>The Stateflows JSON representation.</returns>
        public static implicit operator string(BehaviorClass behaviorClass)
            => StateflowsJsonConverter.SerializeObject(behaviorClass);
        
        /// <summary>Creates an identifier for an instance of this behavior class.</summary>
        /// <param name="instance">The instance key; an empty value identifies the default instance.</param>
        /// <returns>The behavior instance identifier.</returns>
        public BehaviorId ToId(string instance)
            => new BehaviorId(this, instance);

        /// <summary>Determines whether this class equals another behavior class.</summary>
        /// <param name="other">The class to compare.</param>
        /// <returns><see langword="true"/> when both kind and name match.</returns>
        public bool Equals(BehaviorClass other)
            => Type == other.Type && Name == other.Name;
    }
}
