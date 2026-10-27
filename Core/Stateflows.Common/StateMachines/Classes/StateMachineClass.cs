using Newtonsoft.Json;
using Stateflows.Common.Utilities;

namespace Stateflows
{
    /// <summary>Identifies a state-machine definition independently of an instance key.</summary>
    public struct StateMachineClass
    {
        /// <summary>The behavior type discriminator for state machines.</summary>
        public static readonly string Type = BehaviorType.StateMachine;

        /// <summary>Creates a state-machine class identifier.</summary>
        /// <param name="name">The registered state-machine definition name.</param>
        public StateMachineClass(string name)
        {
            Name = name;
        }

        /// <summary>Gets or sets the state-machine definition name.</summary>
        public string Name { get; set; }

        
        /// <summary>Gets the state-machine-typed general behavior class with this name.</summary>
        [JsonIgnore]
        public readonly BehaviorClass BehaviorClass => new BehaviorClass(Type, Name);

        /// <summary>Compares state-machine classes by name.</summary>
        /// <param name="class1">The first class.</param>
        /// <param name="class2">The second class.</param>
        /// <returns>Whether the names match.</returns>
        public static bool operator ==(StateMachineClass class1, StateMachineClass class2)
            => class1.Equals(class2);

        /// <summary>Tests whether state-machine class names differ.</summary>
        /// <param name="class1">The first class.</param>
        /// <param name="class2">The second class.</param>
        /// <returns>Whether the names differ.</returns>
        public static bool operator !=(StateMachineClass class1, StateMachineClass class2)
            => !class1.Equals(class2);

        /// <summary>Compares a state-machine class with a general class, including the behavior type.</summary>
        /// <param name="stateMachineClass">The state-machine class.</param>
        /// <param name="behaviorClass">The general behavior class.</param>
        /// <returns>Whether both describe the same state-machine definition.</returns>
        public static bool operator ==(StateMachineClass stateMachineClass, BehaviorClass behaviorClass)
            => stateMachineClass.BehaviorClass.Equals(behaviorClass);

        /// <summary>Tests whether a state-machine class and a general class differ in type or name.</summary>
        /// <param name="stateMachineClass">The state-machine class.</param>
        /// <param name="behaviorClass">The general behavior class.</param>
        /// <returns>Whether the classes differ.</returns>
        public static bool operator !=(StateMachineClass stateMachineClass, BehaviorClass behaviorClass)
            => !stateMachineClass.BehaviorClass.Equals(behaviorClass);

        /// <summary>Compares a general class with a state-machine class, including the behavior type.</summary>
        /// <param name="behaviorClass">The general behavior class.</param>
        /// <param name="stateMachineClass">The state-machine class.</param>
        /// <returns>Whether both describe the same state-machine definition.</returns>
        public static bool operator ==(BehaviorClass behaviorClass, StateMachineClass stateMachineClass)
            => behaviorClass.Equals(stateMachineClass.BehaviorClass);

        /// <summary>Tests whether a general class and a state-machine class differ in type or name.</summary>
        /// <param name="behaviorClass">The general behavior class.</param>
        /// <param name="stateMachineClass">The state-machine class.</param>
        /// <returns>Whether the classes differ.</returns>
        public static bool operator !=(BehaviorClass behaviorClass, StateMachineClass stateMachineClass)
            => !behaviorClass.Equals(stateMachineClass.BehaviorClass);

        /// <summary>Converts a state-machine class to a general class with the state-machine type.</summary>
        /// <param name="stateMachineClass">The class to convert.</param>
        /// <returns>The general class with the same name.</returns>
        public static implicit operator BehaviorClass(StateMachineClass stateMachineClass)
            => stateMachineClass.BehaviorClass;

        /// <summary>Creates a state-machine class from a general class's name without validating its type.</summary>
        /// <param name="behaviorClass">The general class whose name is copied.</param>
        /// <returns>A state-machine class with the supplied name.</returns>
        public static implicit operator StateMachineClass(BehaviorClass behaviorClass)
            => new StateMachineClass(behaviorClass.Name);

        /// <summary>Tests equality with another boxed state-machine class by name.</summary>
        /// <param name="obj">The value to compare.</param>
        /// <returns>Whether the object is a state-machine class with the same name.</returns>
        public readonly override bool Equals(object obj)
            =>
                obj is StateMachineClass @class &&
                Name == @class.Name;

        /// <summary>Computes the hash of the state-machine definition name.</summary>
        /// <returns>The class hash code.</returns>
        public readonly override int GetHashCode()
            => Name.GetHashCode();

        /// <summary>Serializes this state-machine class to JSON.</summary>
        /// <returns>The serialized class identifier.</returns>
        public readonly override string ToString()
            => StateflowsJsonConverter.SerializeObject(this);

        /// <summary>Serializes a state-machine class to JSON.</summary>
        /// <param name="stateMachineClass">The class to serialize.</param>
        /// <returns>The serialized class identifier.</returns>
        public static implicit operator string(StateMachineClass stateMachineClass)
            => StateflowsJsonConverter.SerializeObject(stateMachineClass);
        
        /// <summary>Creates a state-machine identifier using this class's implicit string representation as its name.</summary>
        /// <param name="instance">The instance key.</param>
        /// <returns>An identifier with the class's serialized JSON as its name and the supplied instance key.</returns>
        /// <remarks>The current implementation uses the implicit string conversion rather than copying <see cref="Name"/>.</remarks>
        public StateMachineId ToId(string instance)
            => new StateMachineId(this, instance);
    }
}
