using System;
using Newtonsoft.Json;
using Stateflows.Common.Exceptions;
using Stateflows.Common.Utilities;

namespace Stateflows
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
                throw new StateflowsDefinitionException("BehaviorId doesn't represent Activity");
            }

            Name = id.Name;
            Instance = id.Instance;
        }

        public string Name { get; set; }

        public string Instance { get; set; }

        [JsonIgnore]
        public readonly ActivityClass ActivityClass => new ActivityClass(Name);

        [JsonIgnore]
        public readonly BehaviorId BehaviorId => new BehaviorId(BehaviorType.Activity, Name, Instance);

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

        public readonly override bool Equals(object obj)
            =>
                obj is ActivityId id &&
                Name == id.Name &&
                Instance == id.Instance;

        public readonly override int GetHashCode()
            => Tuple.Create(Name, Instance).GetHashCode();

        public readonly override string ToString()
            => StateflowsJsonConverter.SerializeObject(this);

        public static implicit operator string(ActivityId activityId)
            => StateflowsJsonConverter.SerializeObject(activityId);
    }
}
