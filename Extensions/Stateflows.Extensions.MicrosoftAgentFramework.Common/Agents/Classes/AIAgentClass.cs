using Newtonsoft.Json;
using Stateflows.Common.Utilities;

namespace Stateflows
{
    public struct AIAgentClass(string name)
    {
        public static readonly string Type = MAFBehaviorType.AIAgent;

        public string Name { get; set; } = name;


        [JsonIgnore]
        public readonly BehaviorClass BehaviorClass => new(Type, Name);

        public static bool operator ==(AIAgentClass class1, AIAgentClass class2)
            => class1.Equals(class2);

        public static bool operator !=(AIAgentClass class1, AIAgentClass class2)
            => !class1.Equals(class2);

        public static bool operator ==(AIAgentClass aiAgentClass, BehaviorClass behaviorClass)
            => aiAgentClass.BehaviorClass.Equals(behaviorClass);

        public static bool operator !=(AIAgentClass aiAgentClass, BehaviorClass behaviorClass)
            => !aiAgentClass.BehaviorClass.Equals(behaviorClass);

        public static bool operator ==(BehaviorClass behaviorClass, AIAgentClass aiAgentClass)
            => behaviorClass.Equals(aiAgentClass.BehaviorClass);

        public static bool operator !=(BehaviorClass behaviorClass, AIAgentClass aiAgentClass)
            => !behaviorClass.Equals(aiAgentClass.BehaviorClass);

        public static implicit operator BehaviorClass(AIAgentClass aiAgentClass)
            => aiAgentClass.BehaviorClass;

        public static implicit operator AIAgentClass(BehaviorClass behaviorClass)
            => new(behaviorClass.Name);

        public readonly override bool Equals(object? obj)
            =>
                obj is AIAgentClass @class &&
                Name == @class.Name;

        public readonly override int GetHashCode()
            => Name.GetHashCode();

        public readonly override string ToString()
            => StateflowsJsonConverter.SerializeObject(this);

        public static implicit operator string(AIAgentClass aiAgentClass)
            => StateflowsJsonConverter.SerializeObject(aiAgentClass);
    }
}
