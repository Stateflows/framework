using Newtonsoft.Json;
using System.Collections.Generic;

namespace Stateflows.Common
{
    public class CompoundRequest : Request<CompoundResponse>
    {
        [JsonProperty(TypeNameHandling = TypeNameHandling.None)]
        public IEnumerable<Event> Events { get; set; }
    }
}
