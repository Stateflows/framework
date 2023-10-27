using Newtonsoft.Json;
using System.Collections.Generic;
using Stateflows.Activities.Registration;

namespace Stateflows.Activities.Context.Classes
{
    public class ActionValues
    {
        public Dictionary<string, object> InternalValues { get; set; } = new Dictionary<string, object>();

        public Dictionary<string, object> Values { get; set; } = new Dictionary<string, object>();

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
