using System;
using Newtonsoft.Json;
using Stateflows.Common.Utilities;

namespace Stateflows
{
    public struct BehaviorId
    {
        public BehaviorId(BehaviorClass behaviorClass, string instance)
        {
            BehaviorClass = behaviorClass;
            Instance = instance;
        }

        public BehaviorId(string type, string name, string instance)
        {
            BehaviorClass = new BehaviorClass(type, name);
            Instance = instance;
        }

        [JsonIgnore]
        public string Type => BehaviorClass.Type;

        [JsonIgnore]
        public string Name => BehaviorClass.Name;

        public string Instance { get; set; }

        public BehaviorClass BehaviorClass { get; set; }

        public override string ToString()
            => StateflowsJsonConverter.SerializeObject(this);

        public static bool operator ==(BehaviorId id1, BehaviorId id2)
            => id1.Equals(id2);

        public static bool operator !=(BehaviorId id1, BehaviorId id2)
            => !id1.Equals(id2);

        public override bool Equals(object obj)
            =>
                obj is BehaviorId id &&
                BehaviorClass == id.BehaviorClass &&
                Instance == id.Instance;

        public override int GetHashCode()
            => Tuple.Create(BehaviorClass, Instance).GetHashCode();
    }
}
