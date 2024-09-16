using System;
using Newtonsoft.Json;
using Stateflows.Activities;
using Stateflows.Common.Exceptions;
using Stateflows.Common.Utilities;

namespace Stateflows.StateMachines
{
    public struct StateMachineId
    {
        public StateMachineId(string name, string instance)
        {
            Name = name;
            Instance = instance;
        }

        public StateMachineId(BehaviorId id)
        {
            if (id.Type != StateMachineClass.Type)
            {
                throw new StateflowsDefinitionException("BehaviorId doesn't represent State Machine");
            }

            Name = id.Name;
            Instance = id.Instance;
        }

        public string Name { get; set; }

        public string Instance { get; set; }

        [JsonIgnore]
        public readonly StateMachineClass StateMachineClass => new StateMachineClass(Name);

        [JsonIgnore]
        public readonly BehaviorId BehaviorId => new BehaviorId(StateMachineClass.Type, Name, Instance);

        public static bool operator ==(StateMachineId id1, StateMachineId id2)
            => id1.Equals(id2);

        public static bool operator !=(StateMachineId id1, StateMachineId id2)
            => !id1.Equals(id2);

        public static bool operator ==(StateMachineId stateMachineId, BehaviorId behaviorId)
            => stateMachineId.BehaviorId == behaviorId;

        public static bool operator !=(StateMachineId stateMachineId, BehaviorId behaviorId)
            => stateMachineId.BehaviorId != behaviorId;

        public static bool operator ==(BehaviorId behaviorId, StateMachineId stateMachineId)
            => behaviorId == stateMachineId.BehaviorId;

        public static bool operator !=(BehaviorId behaviorId, StateMachineId stateMachineId)
            => behaviorId != stateMachineId.BehaviorId;

        public static implicit operator BehaviorId(StateMachineId stateMachineId)
            => stateMachineId.BehaviorId;

        public static implicit operator StateMachineId(BehaviorId behaviorId)
            => new StateMachineId(behaviorId);

        public readonly override bool Equals(object obj)
            =>
                obj is StateMachineId id &&
                Name == id.Name &&
                Instance == id.Instance;

        public readonly override int GetHashCode()
            => Tuple.Create(Name, Instance).GetHashCode();

        public readonly override string ToString()
            => StateflowsJsonConverter.SerializeObject(this);

        public static implicit operator string(StateMachineId stateMachineId)
            => StateflowsJsonConverter.SerializeObject(stateMachineId);
    }
}
