using System;

namespace Stateflows.Common
{
    /// <summary>Legacy startup event raised for registered consumers.</summary>
    /// <remarks>This event is deprecated; use a time event for scheduled behavior instead.</remarks>
    [Obsolete("This event is deprecated and will be removed in future versions. Please use time events instead.")]
    public sealed class Startup : SystemEvent
    {
        /// <summary>Gets or sets the signature identifying the startup consumer.</summary>
        public string ConsumerSignature { get; set; }
    }
}
