using Stateflows.Common.Utilities;
using System.Text.Json.Serialization;

namespace Stateflows.Common.Transport.Classes
{
    public class StateflowsRequest
    {
        public Event Event { get; set; }

        //public string EventString { get; set; }

        //[JsonIgnore]
        //public Event Event
        //{
        //    get => StateflowsJsonConverter.DeserializeObject<Event>(EventString);
        //    set => EventString = StateflowsJsonConverter.SerializePolymorphicObject(value);
        //}

        public BehaviorId BehaviorId { get; set; }

        //public string BehaviorIdString { get; set; }
        
        //[JsonIgnore]
        //public BehaviorId BehaviorId
        //{
        //    get => StateflowsJsonConverter.DeserializeObject<BehaviorId>(BehaviorIdString);
        //    set => BehaviorIdString = StateflowsJsonConverter.SerializePolymorphicObject(value);
        //}
    }
}
