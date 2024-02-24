using Newtonsoft.Json;
using System.Collections.Generic;

namespace Stateflows.Common
{
    public class CompoundResponse : Response
    {
        [JsonProperty(TypeNameHandling = TypeNameHandling.None)]
        public IEnumerable<RequestResult> Results { get; set; }
    }
}
