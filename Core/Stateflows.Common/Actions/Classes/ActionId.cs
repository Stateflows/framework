using System;
using System.Text.Json.Serialization;
using Stateflows.Common.Exceptions;
using Stateflows.Common.Utilities;

namespace Stateflows
{
    public struct ActionId : IEquatable<ActionId>
    {
        public ActionId(string name, string instance)
        {
            Name = name;
            Instance = instance;
        }

        public ActionId(BehaviorId id)
        {
            if (id.Type is BehaviorType.Activity or BehaviorType.Entity or BehaviorType.StateMachine)
            {
                throw new StateflowsDefinitionException("BehaviorId doesn't represent Action");
            }

            Name = id.Name;
            Instance = id.Instance;
        }

        public string Name { get; set; }

        public string Instance { get; set; }

        [Newtonsoft.Json.JsonIgnore]
        [JsonIgnore]
        public string InstanceText => string.IsNullOrEmpty(Instance)
            ? "<default>"
            : Instance;
        
        [Newtonsoft.Json.JsonIgnore]
        [JsonIgnore]
        public readonly ActionClass ActionClass => new ActionClass(Name);

        
        [Newtonsoft.Json.JsonIgnore]
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
        {
            return obj is ActionId other && Equals(other);
        }

        public readonly override int GetHashCode()
        {
            return HashCode.Combine(Name, Instance);
        }

        public readonly override string ToString()
            => StateflowsJsonConverter.SerializeObject(this);

        public static implicit operator string(ActionId actionId)
            => StateflowsJsonConverter.SerializeObject(actionId);
        
        public ActionId ToId(string instance)
            => new ActionId(this, instance);

        public bool Equals(ActionId other)
            => Name == other.Name && Instance == other.Instance;
    }
}
