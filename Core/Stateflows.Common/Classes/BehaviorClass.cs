using System;
using Stateflows.Common.Utilities;

namespace Stateflows
{
    public struct BehaviorClass
    {
        public BehaviorClass(string type, string name)
        {
            Type = type;
            Name = name;
        }

        public string Type { get; set; }

        public string Name { get; set; }

        public static bool operator ==(BehaviorClass class1, BehaviorClass class2)
            => class1.Equals(class2);

        public static bool operator !=(BehaviorClass class1, BehaviorClass class2)
            => !class1.Equals(class2);

        public readonly override bool Equals(object obj)
            =>
                obj is BehaviorClass @class &&
                Type == @class.Type &&
                Name == @class.Name;

        public readonly override int GetHashCode()
            => Tuple.Create(Type, Name).GetHashCode();

        public readonly override string ToString()
            => StateflowsJsonConverter.SerializeObject(this);
    }
}
