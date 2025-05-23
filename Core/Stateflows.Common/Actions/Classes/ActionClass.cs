using Newtonsoft.Json;
using Stateflows.Common;
using Stateflows.Common.Utilities;

namespace Stateflows
{
    public struct ActionClass
    {
        public static readonly string Type = BehaviorType.Action;

        public ActionClass(string name)
        {
            Name = name;
        }

        public string Name { get; set; }

        
        [JsonIgnore]
        public readonly BehaviorClass BehaviorClass => new BehaviorClass(Type, Name);

        public static bool operator ==(ActionClass class1, ActionClass class2)
            => class1.Equals(class2);

        public static bool operator !=(ActionClass class1, ActionClass class2)
            => !class1.Equals(class2);

        public static bool operator ==(ActionClass actionClass, BehaviorClass behaviorClass)
            => actionClass.BehaviorClass.Equals(behaviorClass);

        public static bool operator !=(ActionClass actionClass, BehaviorClass behaviorClass)
            => !actionClass.BehaviorClass.Equals(behaviorClass);

        public static bool operator ==(BehaviorClass behaviorClass, ActionClass actionClass)
            => behaviorClass.Equals(actionClass.BehaviorClass);

        public static bool operator !=(BehaviorClass behaviorClass, ActionClass actionClass)
            => !behaviorClass.Equals(actionClass.BehaviorClass);

        public static implicit operator BehaviorClass(ActionClass actionClass)
            => actionClass.BehaviorClass;

        public static implicit operator ActionClass(BehaviorClass behaviorClass)
            => new ActionClass(behaviorClass.Name);

        public readonly override bool Equals(object obj)
            =>
                obj is ActionClass @class &&
                Name == @class.Name;

        public readonly override int GetHashCode()
            => Name.GetHashCode();

        public readonly override string ToString()
            => StateflowsJsonConverter.SerializeObject(this);

        public static implicit operator string(ActionClass actionClass)
            => StateflowsJsonConverter.SerializeObject(actionClass);
    }
}
