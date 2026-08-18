using System;
using Stateflows.Common.Utilities;

namespace Stateflows
{
    public struct BehaviorClass(string type, string name) : IEquatable<BehaviorClass>
    {
        public string Type { get; set; } = type;

        public string Name { get; set; } = name;

        public static bool operator ==(BehaviorClass class1, BehaviorClass class2)
            => class1.Equals(class2);

        public static bool operator !=(BehaviorClass class1, BehaviorClass class2)
            => !class1.Equals(class2);

        public readonly override bool Equals(object obj)
        {
            return obj is BehaviorClass other && Equals(other);
        }

        public readonly override int GetHashCode()
        {
            return HashCode.Combine(Type, Name);
        }

        public readonly override string ToString()
            => StateflowsJsonConverter.SerializeObject(this);

        public static implicit operator string(BehaviorClass behaviorClass)
            => StateflowsJsonConverter.SerializeObject(behaviorClass);
        
        public BehaviorId ToId(string instance)
            => new BehaviorId(this, instance);

        public bool Equals(BehaviorClass other)
            => Type == other.Type && Name == other.Name;
    }
}
