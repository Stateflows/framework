using Stateflows.Common.Exceptions;
using Stateflows.StateMachines;
using System;

namespace Stateflows.Activities
{
    public struct ActivityId
    {
        public ActivityId(string name, string instance)
        {
            Name = name;
            Instance = instance;
        }

        public ActivityId(BehaviorId id)
        {
            if (id.Type != "Activity")
            {
                throw new StateflowsException("BehaviorId doesn't represent Activity");
            }

            Name = id.Name;
            Instance = id.Instance;
        }

        public string Name { get; set; }

        public string Instance { get; set; }

        public BehaviorId BehaviorId => new BehaviorId("Activity", Name, Instance);

        public static bool operator ==(ActivityId id1, ActivityId id2)
            => id1.Equals(id2);

        public static bool operator !=(ActivityId id1, ActivityId id2)
            => !id1.Equals(id2);

        public static bool operator ==(ActivityId id1, BehaviorId id2)
            => id1.BehaviorId == id2;

        public static bool operator !=(ActivityId id1, BehaviorId id2)
            => id1.BehaviorId != id2;

        public static implicit operator BehaviorId(ActivityId activityId)
            => activityId.BehaviorId;

        public static implicit operator ActivityId(BehaviorId behaviorId)
            => new ActivityId(behaviorId);

        public override bool Equals(object obj)
            =>
                obj is ActivityId &&
                Name == ((ActivityId)obj).Name &&
                Instance == ((ActivityId)obj).Instance;

        public override int GetHashCode()
            => Tuple.Create(Name, Instance).GetHashCode();
    }
}
