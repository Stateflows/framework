using System;

namespace Stateflows.Common
{
    [Obsolete("This event is deprecated and will be removed in future versions. Please use time events instead.")]
    public sealed class Startup : SystemEvent
    {
        public string ConsumerSignature { get; set; }
    }
}
