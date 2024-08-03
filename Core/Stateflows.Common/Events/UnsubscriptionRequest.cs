using Newtonsoft.Json;
using System.Collections.Generic;

namespace Stateflows.Common
{
    [NoImplicitInitialization]
    public sealed class UnsubscriptionRequest : Request<UnsubscriptionResponse>
    {
        public BehaviorId BehaviorId { get; set; }

        [JsonProperty(TypeNameHandling = TypeNameHandling.None)]
        public List<string> NotificationNames { get; set; } = new List<string>();
    }
}
