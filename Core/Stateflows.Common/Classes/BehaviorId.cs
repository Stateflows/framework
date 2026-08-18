using System;
using System.Text.Json.Serialization;
using Stateflows.Common.Utilities;

namespace Stateflows
{
    public struct BehaviorId : IEquatable<BehaviorId>
    {
        public BehaviorId(BehaviorClass behaviorClass, string instance)
        {
            BehaviorClass = new BehaviorClass(behaviorClass.Type, behaviorClass.Name);
            Instance = instance;
        }

        public BehaviorId(string type, string name, string instance)
        {
            BehaviorClass = new BehaviorClass(type, name);
            Instance = instance;
        }

        
        [Newtonsoft.Json.JsonIgnore]
        [JsonIgnore]
        public readonly string Type => BehaviorClass.Type;

        
        [Newtonsoft.Json.JsonIgnore]
        [JsonIgnore]
        public readonly string Name => BehaviorClass.Name;

        public string Instance { get; set; }

        [Newtonsoft.Json.JsonIgnore]
        [JsonIgnore]
        public string InstanceText => string.IsNullOrEmpty(Instance)
            ? "<default>"
            : Instance;

        public BehaviorClass BehaviorClass { get; set; }

        public readonly override string ToString()
            => StateflowsJsonConverter.SerializeObject(this);

        public static implicit operator string(BehaviorId behaviorId)
            => StateflowsJsonConverter.SerializeObject(behaviorId);

        public static bool operator ==(BehaviorId id1, BehaviorId id2)
            => id1.Equals(id2);

        public static bool operator !=(BehaviorId id1, BehaviorId id2)
            => !id1.Equals(id2);

        public readonly override bool Equals(object obj)
        {
            return obj is BehaviorId other && Equals(other);
        }

        public readonly override int GetHashCode()
            => HashCode.Combine(Instance, BehaviorClass);

        public bool Equals(BehaviorId other)
            => Instance == other.Instance && BehaviorClass.Equals(other.BehaviorClass);
    }
}
