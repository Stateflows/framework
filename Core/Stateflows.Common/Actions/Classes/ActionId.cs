using System;
using Newtonsoft.Json;
using Stateflows.Common.Exceptions;
using Stateflows.Common.Utilities;

namespace Stateflows
{
    public struct ActionId
    {
        public ActionId(string name, string instance)
        {
            Name = name;
            Instance = instance;
        }

        public ActionId(BehaviorId id)
        {
            if (id.Type != BehaviorType.Action)
            {
                throw new StateflowsDefinitionException("BehaviorId doesn't represent Action");
            }

            Name = id.Name;
            Instance = id.Instance;
        }

        public string Name { get; set; }

        public string Instance { get; set; }

        
        [JsonIgnore]
        public readonly ActionClass ActionClass => new ActionClass(Name);

        
        [JsonIgnore]
        public readonly BehaviorId BehaviorId => new BehaviorId(BehaviorType.Action, Name, Instance);

        public static bool operator ==(ActionId id1, ActionId id2)
            => id1.Equals(id2);

        public static bool operator !=(ActionId id1, ActionId id2)
            => !id1.Equals(id2);

        public static bool operator ==(ActionId id1, BehaviorId id2)
            => id1.BehaviorId == id2;

        public static bool operator !=(ActionId id1, BehaviorId id2)
            => id1.BehaviorId != id2;

        public static implicit operator BehaviorId(ActionId actionId)
            => actionId.BehaviorId;

        public static implicit operator ActionId(BehaviorId behaviorId)
            => new ActionId(behaviorId);

        public readonly override bool Equals(object obj)
            =>
                obj is ActionId id &&
                Name == id.Name &&
                Instance == id.Instance;

        public readonly override int GetHashCode()
            => Tuple.Create(Name, Instance).GetHashCode();

        public readonly override string ToString()
            => StateflowsJsonConverter.SerializeObject(this);

        public static implicit operator string(ActionId actionId)
            => StateflowsJsonConverter.SerializeObject(actionId);
    }
}
