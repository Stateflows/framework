using Newtonsoft.Json;
using System.Collections.Generic;
using Stateflows.StateMachines.Registration;

namespace Stateflows.StateMachines.Context.Classes
{
    public class StateValues
    {
        public Dictionary<string, object> InternalValues { get; set; } = new Dictionary<string, object>();

        public Dictionary<string, string> Values { get; set; } = new Dictionary<string, string>();

        private List<string> timeEventIds = null;

        [JsonIgnore]
        public List<string> TimeEventIds
        {
            get
            {
                if (timeEventIds == null)
                {
                    if (
                        InternalValues.TryGetValue(Constants.TimeEventIds, out var timeEventsIdsObj) &&
                        timeEventsIdsObj != null &&
                        timeEventsIdsObj is List<string>
                    )
                    {
                        timeEventIds = timeEventsIdsObj as List<string>;
                    }
                    else
                    {
                        timeEventIds = new List<string>();
                        InternalValues[Constants.TimeEventIds] = timeEventIds;
                    }
                }

                return timeEventIds;
            }
        }
    }
}
