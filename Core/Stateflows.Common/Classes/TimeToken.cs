using System;

namespace Stateflows.Common.Classes
{
    public class TimeToken : EventToken
    {
        public string Id { get; set; }

        public DateTime CreatedAt { get; set; }

        public new TimeEvent Event { get; set; }
    }
}
