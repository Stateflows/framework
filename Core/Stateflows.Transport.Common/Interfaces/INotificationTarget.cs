using System.Collections.Generic;
using Newtonsoft.Json;
using Stateflows.Common.Transport.Classes;

namespace Stateflows.Common.Transport.Interfaces
{
    public interface INotificationTarget
    {
        [JsonProperty(TypeNameHandling = TypeNameHandling.None)]
        IEnumerable<Watch> Watches { get; }

        BehaviorId Id { get; }

    }
}
