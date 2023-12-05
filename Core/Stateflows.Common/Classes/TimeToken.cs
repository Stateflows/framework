using System;
using Newtonsoft.Json;

namespace Stateflows.Common.Classes
{
    public class TimeToken : EventToken
    {
        public string Id { get; set; }

        public DateTime CreatedAt { get; set; }

        public new TimeEvent Event { get; set; }

        public string EdgeIdentifier { get; set; }
    }
}
