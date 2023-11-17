using Newtonsoft.Json;
using Stateflows.Common.Utilities;

namespace Stateflows
{
    public struct StateMachineClass
    {
        public static readonly string Type = "StateMachine";

        public StateMachineClass(string name)
        {
            Name = name;
        }

        public string Name { get; set; }

        [JsonIgnore]
        public readonly BehaviorClass BehaviorClass => new BehaviorClass(Type, Name);

        public static bool operator ==(StateMachineClass class1, StateMachineClass class2)
            => class1.Equals(class2);

        public static bool operator !=(StateMachineClass class1, StateMachineClass class2)
            => !class1.Equals(class2);

        public static bool operator ==(StateMachineClass stateMachineClass, BehaviorClass behaviorClass)
            => stateMachineClass.BehaviorClass.Equals(behaviorClass);

        public static bool operator !=(StateMachineClass stateMachineClass, BehaviorClass behaviorClass)
            => !stateMachineClass.BehaviorClass.Equals(behaviorClass);

        public static bool operator ==(BehaviorClass behaviorClass, StateMachineClass stateMachineClass)
            => behaviorClass.Equals(stateMachineClass.BehaviorClass);

        public static bool operator !=(BehaviorClass behaviorClass, StateMachineClass stateMachineClass)
            => !behaviorClass.Equals(stateMachineClass.BehaviorClass);

        public readonly override bool Equals(object obj)
            =>
                obj is StateMachineClass @class &&
                Name == @class.Name;

        public readonly override int GetHashCode()
            => Name.GetHashCode();

        public readonly override string ToString()
            => StateflowsJsonConverter.SerializeObject(this);
    }
}
