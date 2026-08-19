using Newtonsoft.Json;
using Stateflows.Common.Utilities;

namespace Stateflows
{
    public struct AgentClass(string name)
    {
        public static readonly string Type = MAFBehaviorType.AIAgent;

        public string Name { get; set; } = name;


        [JsonIgnore]
        public readonly BehaviorClass BehaviorClass => new(Type, Name);

        public static bool operator ==(AgentClass class1, AgentClass class2)
            => class1.Equals(class2);

        public static bool operator !=(AgentClass class1, AgentClass class2)
            => !class1.Equals(class2);

        public static bool operator ==(AgentClass agentClass, BehaviorClass behaviorClass)
            => agentClass.BehaviorClass.Equals(behaviorClass);

        public static bool operator !=(AgentClass agentClass, BehaviorClass behaviorClass)
            => !agentClass.BehaviorClass.Equals(behaviorClass);

        public static bool operator ==(BehaviorClass behaviorClass, AgentClass agentClass)
            => behaviorClass.Equals(agentClass.BehaviorClass);

        public static bool operator !=(BehaviorClass behaviorClass, AgentClass agentClass)
            => !behaviorClass.Equals(agentClass.BehaviorClass);

        public static implicit operator BehaviorClass(AgentClass agentClass)
            => agentClass.BehaviorClass;

        public static implicit operator AgentClass(BehaviorClass behaviorClass)
            => new(behaviorClass.Name);

        public readonly override bool Equals(object? obj)
            =>
                obj is AgentClass @class &&
                Name == @class.Name;

        public readonly override int GetHashCode()
            => Name.GetHashCode();

        public readonly override string ToString()
            => StateflowsJsonConverter.SerializeObject(this);

        public static implicit operator string(AgentClass agentClass)
            => StateflowsJsonConverter.SerializeObject(agentClass);
    }
}
