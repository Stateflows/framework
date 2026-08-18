using System;
using System.Text.Json.Serialization;
using Stateflows.Common.Exceptions;
using Stateflows.Common.Utilities;

namespace Stateflows
{
    public struct EntityId : IEquatable<EntityId>
    {
        public EntityId(string name, string instance)
        {
            Name = name;
            Instance = instance;
        }

        public EntityId(BehaviorId id)
        {
            if (id.Type != EntityClass.Type)
            {
                throw new StateflowsDefinitionException("BehaviorId doesn't represent Entity");
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
        public readonly EntityClass EntityClass => new EntityClass(Name);

        
        [Newtonsoft.Json.JsonIgnore]
        [JsonIgnore]
        public readonly BehaviorId BehaviorId => new BehaviorId(EntityClass.Type, Name, Instance);

        public static bool operator ==(EntityId id1, EntityId id2)
            => id1.Equals(id2);

        public static bool operator !=(EntityId id1, EntityId id2)
            => !id1.Equals(id2);

        public static bool operator ==(EntityId stateMachineId, BehaviorId behaviorId)
            => stateMachineId.BehaviorId == behaviorId;

        public static bool operator !=(EntityId stateMachineId, BehaviorId behaviorId)
            => stateMachineId.BehaviorId != behaviorId;

        public static bool operator ==(BehaviorId behaviorId, EntityId stateMachineId)
            => behaviorId == stateMachineId.BehaviorId;

        public static bool operator !=(BehaviorId behaviorId, EntityId stateMachineId)
            => behaviorId != stateMachineId.BehaviorId;

        public static implicit operator BehaviorId(EntityId stateMachineId)
            => stateMachineId.BehaviorId;

        public static implicit operator EntityId(BehaviorId behaviorId)
            => new EntityId(behaviorId);

        public readonly override bool Equals(object obj)
        {
            return obj is EntityId other && Equals(other);
        }

        public readonly override int GetHashCode()
        {
            return HashCode.Combine(Name, Instance);
        }

        public readonly override string ToString()
            => StateflowsJsonConverter.SerializeObject(this);

        public static implicit operator string(EntityId stateMachineId)
            => StateflowsJsonConverter.SerializeObject(stateMachineId);

        public bool Equals(EntityId other)
            => Name == other.Name && Instance == other.Instance;
    }
}
