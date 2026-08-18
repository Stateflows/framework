using Newtonsoft.Json;
using Stateflows.Common.Utilities;

namespace Stateflows
{
    public struct EntityClass
    {
        public static readonly string Type = BehaviorType.Entity;

        public EntityClass(string name)
        {
            Name = name;
        }

        public string Name { get; set; }

        [JsonIgnore]
        public readonly BehaviorClass BehaviorClass => new BehaviorClass(Type, Name);

        public static bool operator ==(EntityClass class1, EntityClass class2)
            => class1.Equals(class2);

        public static bool operator !=(EntityClass class1, EntityClass class2)
            => !class1.Equals(class2);

        public static bool operator ==(EntityClass entityClass, BehaviorClass behaviorClass)
            => entityClass.BehaviorClass.Equals(behaviorClass);

        public static bool operator !=(EntityClass entityClass, BehaviorClass behaviorClass)
            => !entityClass.BehaviorClass.Equals(behaviorClass);

        public static bool operator ==(BehaviorClass behaviorClass, EntityClass entityClass)
            => behaviorClass.Equals(entityClass.BehaviorClass);

        public static bool operator !=(BehaviorClass behaviorClass, EntityClass entityClass)
            => !behaviorClass.Equals(entityClass.BehaviorClass);

        public static implicit operator BehaviorClass(EntityClass entityClass)
            => entityClass.BehaviorClass;

        public static implicit operator EntityClass(BehaviorClass behaviorClass)
            => new EntityClass(behaviorClass.Name);

        public readonly override bool Equals(object obj)
            =>
                obj is EntityClass @class &&
                Name == @class.Name;

        public readonly override int GetHashCode()
            => Name.GetHashCode();

        public readonly override string ToString()
            => StateflowsJsonConverter.SerializeObject(this);

        public static implicit operator string(EntityClass entityClass)
            => StateflowsJsonConverter.SerializeObject(entityClass);

        public BehaviorId ToId(string instance)
            => new BehaviorId(this, instance);
    }
}

