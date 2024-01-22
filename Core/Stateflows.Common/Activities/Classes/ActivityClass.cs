using Newtonsoft.Json;
using Stateflows.Common.Utilities;

namespace Stateflows
{
    public struct ActivityClass
    {
        public static readonly string Type = "Activity";

        public ActivityClass(string name)
        {
            Name = name;
        }

        public string Name { get; set; }

        [JsonIgnore]
        public readonly BehaviorClass BehaviorClass => new BehaviorClass(Type, Name);

        public static bool operator ==(ActivityClass class1, ActivityClass class2)
            => class1.Equals(class2);

        public static bool operator !=(ActivityClass class1, ActivityClass class2)
            => !class1.Equals(class2);

        public static bool operator ==(ActivityClass stateMachineClass, BehaviorClass behaviorClass)
            => stateMachineClass.BehaviorClass.Equals(behaviorClass);

        public static bool operator !=(ActivityClass stateMachineClass, BehaviorClass behaviorClass)
            => !stateMachineClass.BehaviorClass.Equals(behaviorClass);

        public static bool operator ==(BehaviorClass behaviorClass, ActivityClass stateMachineClass)
            => behaviorClass.Equals(stateMachineClass.BehaviorClass);

        public static bool operator !=(BehaviorClass behaviorClass, ActivityClass stateMachineClass)
            => !behaviorClass.Equals(stateMachineClass.BehaviorClass);

        public static implicit operator BehaviorClass(ActivityClass activityClass)
            => activityClass.BehaviorClass;

        public static implicit operator ActivityClass(BehaviorClass behaviorClass)
            => new ActivityClass(behaviorClass.Name);

        public readonly override bool Equals(object obj)
            =>
                obj is ActivityClass @class &&
                Name == @class.Name;

        public readonly override int GetHashCode()
            => Name.GetHashCode();

        public readonly override string ToString()
            => StateflowsJsonConverter.SerializeObject(this);
    }
}
