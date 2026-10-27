using Newtonsoft.Json;
using Stateflows.Common.Utilities;

namespace Stateflows
{
    /// <summary>Identifies an entity behavior definition by its registered name.</summary>
    public struct EntityClass
    {
        /// <summary>The behavior type used for entity instances.</summary>
        public static readonly string Type = BehaviorType.Entity;

        /// <summary>Creates an entity class identifier.</summary>
        /// <param name="name">The registered entity name.</param>
        public EntityClass(string name)
        {
            Name = name;
        }

        /// <summary>Gets or sets the registered entity name.</summary>
        public string Name { get; set; }

        /// <summary>Gets the corresponding general behavior class.</summary>
        [JsonIgnore]
        public readonly BehaviorClass BehaviorClass => new BehaviorClass(Type, Name);

        /// <summary>Compares two entity classes for equality.</summary>
        /// <param name="class1">The first entity class.</param>
        /// <param name="class2">The second entity class.</param>
        /// <returns><see langword="true"/> when the registered names match.</returns>
        public static bool operator ==(EntityClass class1, EntityClass class2)
            => class1.Equals(class2);

        /// <summary>Compares two entity classes for inequality.</summary>
        /// <param name="class1">The first entity class.</param>
        /// <param name="class2">The second entity class.</param>
        /// <returns><see langword="true"/> when the registered names differ.</returns>
        public static bool operator !=(EntityClass class1, EntityClass class2)
            => !class1.Equals(class2);

        /// <summary>Compares an entity class with a general behavior class.</summary>
        /// <param name="entityClass">The entity class.</param>
        /// <param name="behaviorClass">The general behavior class.</param>
        /// <returns><see langword="true"/> when both identify the same entity behavior class.</returns>
        public static bool operator ==(EntityClass entityClass, BehaviorClass behaviorClass)
            => entityClass.BehaviorClass.Equals(behaviorClass);

        /// <summary>Determines whether an entity class differs from a behavior class.</summary>
        /// <param name="entityClass">The entity class.</param>
        /// <param name="behaviorClass">The general behavior class.</param>
        /// <returns><see langword="true"/> when they identify different behavior classes.</returns>
        public static bool operator !=(EntityClass entityClass, BehaviorClass behaviorClass)
            => !entityClass.BehaviorClass.Equals(behaviorClass);

        /// <summary>Compares a behavior class with an entity class.</summary>
        /// <param name="behaviorClass">The general behavior class.</param>
        /// <param name="entityClass">The entity class.</param>
        /// <returns><see langword="true"/> when both identify the same entity behavior class.</returns>
        public static bool operator ==(BehaviorClass behaviorClass, EntityClass entityClass)
            => behaviorClass.Equals(entityClass.BehaviorClass);

        /// <summary>Determines whether a behavior class differs from an entity class.</summary>
        /// <param name="behaviorClass">The general behavior class.</param>
        /// <param name="entityClass">The entity class.</param>
        /// <returns><see langword="true"/> when they identify different behavior classes.</returns>
        public static bool operator !=(BehaviorClass behaviorClass, EntityClass entityClass)
            => !behaviorClass.Equals(entityClass.BehaviorClass);

        /// <summary>Converts an entity class to a behavior class.</summary>
        /// <param name="entityClass">The entity class to convert.</param>
        /// <returns>The equivalent general behavior class.</returns>
        public static implicit operator BehaviorClass(EntityClass entityClass)
            => entityClass.BehaviorClass;

        /// <summary>Converts a behavior class to an entity class.</summary>
        /// <param name="behaviorClass">The behavior class to convert.</param>
        /// <returns>An entity class using the behavior class name.</returns>
        public static implicit operator EntityClass(BehaviorClass behaviorClass)
            => new EntityClass(behaviorClass.Name);

        /// <summary>Determines whether this class equals the specified object.</summary>
        /// <param name="obj">The object to compare.</param>
        /// <returns><see langword="true"/> when <paramref name="obj"/> is an entity class with the same name.</returns>
        public readonly override bool Equals(object obj)
            =>
                obj is EntityClass @class &&
                Name == @class.Name;

        /// <summary>Returns a hash code for this entity class.</summary>
        /// <returns>A hash code based on the entity name.</returns>
        public readonly override int GetHashCode()
            => Name.GetHashCode();

        /// <summary>Serializes this class using the Stateflows JSON representation.</summary>
        /// <returns>The serialized entity class.</returns>
        public readonly override string ToString()
            => StateflowsJsonConverter.SerializeObject(this);

        /// <summary>Converts an entity class to its serialized representation.</summary>
        /// <param name="entityClass">The class to serialize.</param>
        /// <returns>The Stateflows JSON representation.</returns>
        public static implicit operator string(EntityClass entityClass)
            => StateflowsJsonConverter.SerializeObject(entityClass);

        /// <summary>Creates an identifier for an instance of this entity class.</summary>
        /// <param name="instance">The instance key; an empty value denotes the default instance.</param>
        /// <returns>The entity behavior identifier.</returns>
        public BehaviorId ToId(string instance)
            => new BehaviorId(this, instance);
    }
}
