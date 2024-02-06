using System;
using Stateflows.Common.Exceptions;

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
            if (id.Type != BehaviorType.Activity)
            {
                throw new StateflowsException("BehaviorId doesn't represent Activity");
            }

            Name = id.Name;
            Instance = id.Instance;
        }

        public string Name { get; set; }

        public string Instance { get; set; }

        public readonly BehaviorId BehaviorId => BehaviorType.Activity.ToClass(Name).ToId(Instance);

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

        public override readonly bool Equals(object obj)
            =>
                obj is ActivityId id &&
                Name == id.Name &&
                Instance == id.Instance;

        public readonly override int GetHashCode()
            => Tuple.Create(Name, Instance).GetHashCode();
    }
}
