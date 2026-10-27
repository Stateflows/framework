using System;

namespace Stateflows.Common
{
    /// <summary>Sets the lifetime, in seconds, for notifications of a type.</summary>
    /// <remarks>The notification subscriber gives this type-level setting precedence over a time-to-live delivery header.</remarks>
    [AttributeUsage(
        AttributeTargets.Class | AttributeTargets.Enum | AttributeTargets.Interface | AttributeTargets.Struct,
        AllowMultiple = false)]
    public sealed class TimeToLiveAttribute : Attribute
    {
        /// <summary>Creates a time-to-live setting for notifications of the decorated type.</summary>
        /// <param name="secondsToLive">The notification lifetime in seconds.</param>
        public TimeToLiveAttribute(int secondsToLive)
        {
            SecondsToLive = secondsToLive;
        }
        
        /// <summary>Gets the notification lifetime in seconds.</summary>
        public int SecondsToLive { get; private set; }
    }
}
