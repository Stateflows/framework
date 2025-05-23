using Newtonsoft.Json;
using Stateflows.Common;
using Stateflows.Common.Utilities;

namespace Stateflows
{
    public struct ActivityClass
    {
        public static readonly string Type = BehaviorType.Activity;

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

        public static bool operator ==(ActivityClass activityClass, BehaviorClass behaviorClass)
            => activityClass.BehaviorClass.Equals(behaviorClass);

        public static bool operator !=(ActivityClass activityClass, BehaviorClass behaviorClass)
            => !activityClass.BehaviorClass.Equals(behaviorClass);

        public static bool operator ==(BehaviorClass behaviorClass, ActivityClass activityClass)
            => behaviorClass.Equals(activityClass.BehaviorClass);

        public static bool operator !=(BehaviorClass behaviorClass, ActivityClass activityClass)
            => !behaviorClass.Equals(activityClass.BehaviorClass);

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

        public static implicit operator string(ActivityClass activityClass)
            => StateflowsJsonConverter.SerializeObject(activityClass);
    }
}
