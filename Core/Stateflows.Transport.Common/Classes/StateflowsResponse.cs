using Stateflows.Common.Utilities;
using System.Text.Json.Serialization;

namespace Stateflows.Common.Transport.Classes
{
    public class StateflowsResponse
    {
        public EventStatus EventStatus { get; set; }

        public EventValidation Validation { get; set; }

        //public string ValidationString { get; set; }

        //[JsonIgnore]
        //public EventValidation Validation
        //{
        //    get => StateflowsJsonConverter.DeserializeObject<EventValidation>(ValidationString);
        //    set => ValidationString = StateflowsJsonConverter.SerializePolymorphicObject(value);
        //}

        public Response Response { get; set; }

        //public string ResponseString { get; set; }

        //[JsonIgnore]
        //public Response Response
        //{
        //    get => StateflowsJsonConverter.DeserializeObject<Response>(ResponseString);
        //    set => ResponseString = StateflowsJsonConverter.SerializePolymorphicObject(value);
        //}
    }
}
