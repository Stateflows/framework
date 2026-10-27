using Newtonsoft.Json;
using Stateflows.Common.Utilities;

namespace Stateflows
{
    /// <summary>Identifies a Microsoft Agent Framework agent behavior class by name.</summary>
    /// <param name="name">The registered agent behavior name.</param>
    public struct AIAgentClass(string name)
    {
        /// <summary>Gets the fixed Stateflows behavior type used for Microsoft Agent Framework agents.</summary>
        public static readonly string Type = MAFBehaviorType.AIAgent;

        /// <summary>Gets or sets the registered agent behavior name.</summary>
        public string Name { get; set; } = name;


        /// <summary>Gets the equivalent Stateflows behavior class identity.</summary>
        [JsonIgnore]
        public readonly BehaviorClass BehaviorClass => new(Type, Name);

        /// <summary>Compares two agent class identifiers.</summary>
        /// <param name="class1">The first identifier.</param>
        /// <param name="class2">The second identifier.</param>
        /// <returns><see langword="true"/> when the identifiers have the same name.</returns>
        public static bool operator ==(AIAgentClass class1, AIAgentClass class2)
            => class1.Equals(class2);

        /// <summary>Compares two agent class identifiers for inequality.</summary>
        /// <param name="class1">The first identifier.</param>
        /// <param name="class2">The second identifier.</param>
        /// <returns><see langword="true"/> when the identifiers have different names.</returns>
        public static bool operator !=(AIAgentClass class1, AIAgentClass class2)
            => !class1.Equals(class2);

        /// <summary>Compares an agent class identifier with a Stateflows behavior class.</summary>
        /// <param name="aiAgentClass">The agent class identifier.</param>
        /// <param name="behaviorClass">The Stateflows behavior class.</param>
        /// <returns><see langword="true"/> when both identify the same behavior class.</returns>
        public static bool operator ==(AIAgentClass aiAgentClass, BehaviorClass behaviorClass)
            => aiAgentClass.BehaviorClass.Equals(behaviorClass);

        /// <summary>Compares an agent class identifier with a Stateflows behavior class for inequality.</summary>
        /// <param name="aiAgentClass">The agent class identifier.</param>
        /// <param name="behaviorClass">The Stateflows behavior class.</param>
        /// <returns><see langword="true"/> when the identifiers differ.</returns>
        public static bool operator !=(AIAgentClass aiAgentClass, BehaviorClass behaviorClass)
            => !aiAgentClass.BehaviorClass.Equals(behaviorClass);

        /// <summary>Compares a Stateflows behavior class with an agent class identifier.</summary>
        /// <param name="behaviorClass">The Stateflows behavior class.</param>
        /// <param name="aiAgentClass">The agent class identifier.</param>
        /// <returns><see langword="true"/> when both identify the same behavior class.</returns>
        public static bool operator ==(BehaviorClass behaviorClass, AIAgentClass aiAgentClass)
            => behaviorClass.Equals(aiAgentClass.BehaviorClass);

        /// <summary>Compares a Stateflows behavior class with an agent class identifier for inequality.</summary>
        /// <param name="behaviorClass">The Stateflows behavior class.</param>
        /// <param name="aiAgentClass">The agent class identifier.</param>
        /// <returns><see langword="true"/> when the identifiers differ.</returns>
        public static bool operator !=(BehaviorClass behaviorClass, AIAgentClass aiAgentClass)
            => !behaviorClass.Equals(aiAgentClass.BehaviorClass);

        /// <summary>Converts the agent class identifier to a Stateflows behavior class.</summary>
        /// <param name="aiAgentClass">The agent class identifier.</param>
        /// <returns>The equivalent behavior class.</returns>
        public static implicit operator BehaviorClass(AIAgentClass aiAgentClass)
            => aiAgentClass.BehaviorClass;

        /// <summary>Converts a Stateflows behavior class to an agent class identifier.</summary>
        /// <param name="behaviorClass">The behavior class to convert.</param>
        /// <returns>The agent class identifier with the behavior name.</returns>
        public static implicit operator AIAgentClass(BehaviorClass behaviorClass)
            => new(behaviorClass.Name);

        /// <summary>Determines whether the specified object is an agent class with the same name.</summary>
        /// <param name="obj">The object to compare.</param>
        /// <returns><see langword="true"/> when the object is an equal <see cref="AIAgentClass"/>.</returns>
        public readonly override bool Equals(object? obj)
            =>
                obj is AIAgentClass @class &&
                Name == @class.Name;

        /// <summary>Returns a hash code for this agent class identifier.</summary>
        /// <returns>A hash code based on the agent name.</returns>
        public readonly override int GetHashCode()
            => Name.GetHashCode();

        /// <summary>Serializes this identifier using the Stateflows JSON converter.</summary>
        /// <returns>The serialized agent class identifier.</returns>
        public readonly override string ToString()
            => StateflowsJsonConverter.SerializeObject(this);

        /// <summary>Converts the identifier to its Stateflows JSON representation.</summary>
        /// <param name="aiAgentClass">The identifier to serialize.</param>
        /// <returns>The serialized agent class identifier.</returns>
        public static implicit operator string(AIAgentClass aiAgentClass)
            => StateflowsJsonConverter.SerializeObject(aiAgentClass);
    }
}
