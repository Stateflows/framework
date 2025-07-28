using System;

namespace Stateflows.Common
{
    [AttributeUsage(
        AttributeTargets.Class | AttributeTargets.Enum | AttributeTargets.Interface | AttributeTargets.Struct,
        AllowMultiple = false)]
    public sealed class TimeToLiveAttribute : Attribute
    {
        
        public TimeToLiveAttribute(int secondsToLive)
        {
            SecondsToLive = secondsToLive;
        }
        
        public int SecondsToLive { get; private set; }
    }
}
