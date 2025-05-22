using System;
using System.Text.Json.Serialization;
using Stateflows.Common.Utilities;

namespace Stateflows
{
    public struct BehaviorId
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
            =>
                obj is BehaviorId id &&
                BehaviorClass == id.BehaviorClass &&
                Instance == id.Instance;

        public readonly override int GetHashCode()
            => Tuple.Create(BehaviorClass, Instance).GetHashCode();
    }
}
