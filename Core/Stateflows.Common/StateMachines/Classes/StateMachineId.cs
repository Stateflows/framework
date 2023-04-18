using System;

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
            if (id.Type != "StateMachine")
            {
                throw new Exception("BehaviorId doesn't represent State Machine");
            }

            Name = id.Name;
            Instance = id.Instance;
        }

        public string Name { get; set; }

        public string Instance { get; set; }

        public BehaviorId BehaviorId => new BehaviorId("StateMachine", Name, Instance);

        public static bool operator ==(StateMachineId id1, StateMachineId id2)
            => id1.Equals(id2);

        public static bool operator !=(StateMachineId id1, StateMachineId id2)
            => !id1.Equals(id2);

        public static bool operator ==(StateMachineId id1, BehaviorId id2)
            => id1.BehaviorId == id2;

        public static bool operator !=(StateMachineId id1, BehaviorId id2)
            => id1.BehaviorId != id2;

        public override bool Equals(object obj)
            =>
                obj is StateMachineId &&
                Name == ((StateMachineId)obj).Name &&
                Instance == ((StateMachineId)obj).Instance;

        public override int GetHashCode()
            => Tuple.Create(Name, Instance).GetHashCode();
    }
}
