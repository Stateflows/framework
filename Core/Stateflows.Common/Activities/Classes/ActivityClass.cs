using Newtonsoft.Json;
using Stateflows.Common;
using Stateflows.Common.Utilities;

namespace Stateflows
{
    /// <summary>Identifies an activity definition independently of an instance key.</summary>
    public struct ActivityClass
    {
        /// <summary>The behavior type discriminator for activities.</summary>
        public static readonly string Type = BehaviorType.Activity;

        /// <summary>Creates an activity class identifier.</summary>
        /// <param name="name">The registered activity definition name.</param>
        public ActivityClass(string name)
        {
            Name = name;
        }

        /// <summary>Gets or sets the activity definition name.</summary>
        public string Name { get; set; }

        
        /// <summary>Gets the activity-typed general behavior class with this name.</summary>
        [JsonIgnore]
        public readonly BehaviorClass BehaviorClass => new BehaviorClass(Type, Name);

        /// <summary>Compares activity classes by name.</summary>
        /// <param name="class1">The first class.</param>
        /// <param name="class2">The second class.</param>
        /// <returns>Whether the names match.</returns>
        public static bool operator ==(ActivityClass class1, ActivityClass class2)
            => class1.Equals(class2);

        /// <summary>Tests whether activity class names differ.</summary>
        /// <param name="class1">The first class.</param>
        /// <param name="class2">The second class.</param>
        /// <returns>Whether the names differ.</returns>
        public static bool operator !=(ActivityClass class1, ActivityClass class2)
            => !class1.Equals(class2);

        /// <summary>Compares an activity class with a general class, including the behavior type.</summary>
        /// <param name="activityClass">The activity class.</param>
        /// <param name="behaviorClass">The general behavior class.</param>
        /// <returns>Whether both describe the same activity definition.</returns>
        public static bool operator ==(ActivityClass activityClass, BehaviorClass behaviorClass)
            => activityClass.BehaviorClass.Equals(behaviorClass);

        /// <summary>Tests whether an activity class and a general class differ in type or name.</summary>
        /// <param name="activityClass">The activity class.</param>
        /// <param name="behaviorClass">The general behavior class.</param>
        /// <returns>Whether the classes differ.</returns>
        public static bool operator !=(ActivityClass activityClass, BehaviorClass behaviorClass)
            => !activityClass.BehaviorClass.Equals(behaviorClass);

        /// <summary>Compares a general class with an activity class, including the behavior type.</summary>
        /// <param name="behaviorClass">The general behavior class.</param>
        /// <param name="activityClass">The activity class.</param>
        /// <returns>Whether both describe the same activity definition.</returns>
        public static bool operator ==(BehaviorClass behaviorClass, ActivityClass activityClass)
            => behaviorClass.Equals(activityClass.BehaviorClass);

        /// <summary>Tests whether a general class and an activity class differ in type or name.</summary>
        /// <param name="behaviorClass">The general behavior class.</param>
        /// <param name="activityClass">The activity class.</param>
        /// <returns>Whether the classes differ.</returns>
        public static bool operator !=(BehaviorClass behaviorClass, ActivityClass activityClass)
            => !behaviorClass.Equals(activityClass.BehaviorClass);

        /// <summary>Converts an activity class to an activity-typed general behavior class.</summary>
        /// <param name="activityClass">The class to convert.</param>
        /// <returns>The general class with the activity type and the same name.</returns>
        public static implicit operator BehaviorClass(ActivityClass activityClass)
            => activityClass.BehaviorClass;

        /// <summary>Creates an activity class from a general class's name without validating its type.</summary>
        /// <param name="behaviorClass">The general class whose name is copied.</param>
        /// <returns>An activity class with the supplied name.</returns>
        public static implicit operator ActivityClass(BehaviorClass behaviorClass)
            => new ActivityClass(behaviorClass.Name);

        /// <summary>Tests equality with another boxed activity class by name.</summary>
        /// <param name="obj">The value to compare.</param>
        /// <returns>Whether the object is an activity class with the same name.</returns>
        public readonly override bool Equals(object obj)
            =>
                obj is ActivityClass @class &&
                Name == @class.Name;

        /// <summary>Computes the hash of the activity definition name.</summary>
        /// <returns>The class hash code.</returns>
        public readonly override int GetHashCode()
            => Name.GetHashCode();

        /// <summary>Serializes this activity class to JSON.</summary>
        /// <returns>The serialized class identifier.</returns>
        public readonly override string ToString()
            => StateflowsJsonConverter.SerializeObject(this);

        /// <summary>Serializes an activity class to JSON.</summary>
        /// <param name="activityClass">The class to serialize.</param>
        /// <returns>The serialized class identifier.</returns>
        public static implicit operator string(ActivityClass activityClass)
            => StateflowsJsonConverter.SerializeObject(activityClass);
        
        /// <summary>Creates an activity identifier using this class's implicit string representation as its name.</summary>
        /// <param name="instance">The instance key.</param>
        /// <returns>An identifier with the class's serialized JSON as its name and the supplied instance key.</returns>
        /// <remarks>The current implementation uses the implicit string conversion rather than copying <see cref="Name"/>.</remarks>
        public ActivityId ToId(string instance)
            => new ActivityId(this, instance);
    }
}
