using Newtonsoft.Json;
using Stateflows.Common;
using Stateflows.Common.Utilities;

namespace Stateflows
{
    /// <summary>Identifies an action definition independently of an instance key.</summary>
    public struct ActionClass
    {
        /// <summary>The behavior type discriminator for actions.</summary>
        public static readonly string Type = BehaviorType.Action;

        /// <summary>Creates an action class identifier.</summary>
        /// <param name="name">The registered action definition name.</param>
        public ActionClass(string name)
        {
            Name = name;
        }

        /// <summary>Gets or sets the action definition name.</summary>
        public string Name { get; set; }

        
        /// <summary>Gets the action-typed general behavior class with this name.</summary>
        [JsonIgnore]
        public readonly BehaviorClass BehaviorClass => new BehaviorClass(Type, Name);

        /// <summary>Compares action classes by name.</summary>
        /// <param name="class1">The first class.</param>
        /// <param name="class2">The second class.</param>
        /// <returns>Whether the names match.</returns>
        public static bool operator ==(ActionClass class1, ActionClass class2)
            => class1.Equals(class2);

        /// <summary>Tests whether action class names differ.</summary>
        /// <param name="class1">The first class.</param>
        /// <param name="class2">The second class.</param>
        /// <returns>Whether the names differ.</returns>
        public static bool operator !=(ActionClass class1, ActionClass class2)
            => !class1.Equals(class2);

        /// <summary>Compares an action class with a general class, including the behavior type.</summary>
        /// <param name="actionClass">The action class.</param>
        /// <param name="behaviorClass">The general behavior class.</param>
        /// <returns>Whether both describe the same action definition.</returns>
        public static bool operator ==(ActionClass actionClass, BehaviorClass behaviorClass)
            => actionClass.BehaviorClass.Equals(behaviorClass);

        /// <summary>Tests whether an action class and a general class differ in type or name.</summary>
        /// <param name="actionClass">The action class.</param>
        /// <param name="behaviorClass">The general behavior class.</param>
        /// <returns>Whether the classes differ.</returns>
        public static bool operator !=(ActionClass actionClass, BehaviorClass behaviorClass)
            => !actionClass.BehaviorClass.Equals(behaviorClass);

        /// <summary>Compares a general class with an action class, including the behavior type.</summary>
        /// <param name="behaviorClass">The general behavior class.</param>
        /// <param name="actionClass">The action class.</param>
        /// <returns>Whether both describe the same action definition.</returns>
        public static bool operator ==(BehaviorClass behaviorClass, ActionClass actionClass)
            => behaviorClass.Equals(actionClass.BehaviorClass);

        /// <summary>Tests whether a general class and an action class differ in type or name.</summary>
        /// <param name="behaviorClass">The general behavior class.</param>
        /// <param name="actionClass">The action class.</param>
        /// <returns>Whether the classes differ.</returns>
        public static bool operator !=(BehaviorClass behaviorClass, ActionClass actionClass)
            => !behaviorClass.Equals(actionClass.BehaviorClass);

        /// <summary>Converts an action class to an action-typed general behavior class.</summary>
        /// <param name="actionClass">The class to convert.</param>
        /// <returns>The general class with the action type and the same name.</returns>
        public static implicit operator BehaviorClass(ActionClass actionClass)
            => actionClass.BehaviorClass;

        /// <summary>Creates an action class from a general class's name without validating its type.</summary>
        /// <param name="behaviorClass">The general class whose name is copied.</param>
        /// <returns>An action class with the supplied name.</returns>
        public static implicit operator ActionClass(BehaviorClass behaviorClass)
            => new ActionClass(behaviorClass.Name);

        /// <summary>Tests equality with another boxed action class by name.</summary>
        /// <param name="obj">The value to compare.</param>
        /// <returns>Whether the object is an action class with the same name.</returns>
        public readonly override bool Equals(object obj)
            =>
                obj is ActionClass @class &&
                Name == @class.Name;

        /// <summary>Computes the hash of the action definition name.</summary>
        /// <returns>The class hash code.</returns>
        public readonly override int GetHashCode()
            => Name.GetHashCode();

        /// <summary>Serializes this action class to JSON.</summary>
        /// <returns>The serialized class identifier.</returns>
        public readonly override string ToString()
            => StateflowsJsonConverter.SerializeObject(this);

        /// <summary>Serializes an action class to JSON.</summary>
        /// <param name="actionClass">The class to serialize.</param>
        /// <returns>The serialized class identifier.</returns>
        public static implicit operator string(ActionClass actionClass)
            => StateflowsJsonConverter.SerializeObject(actionClass);
    }
}
