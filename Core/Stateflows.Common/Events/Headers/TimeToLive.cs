using System.Text.Json.Serialization;

namespace Stateflows.Common
{
    /// <summary>Sets the maximum lifetime of an event delivery in seconds.</summary>
    public sealed class TimeToLive : EventHeader
    {
        /// <summary>Creates a time-to-live header whose seconds value can be assigned later.</summary>
        [Newtonsoft.Json.JsonConstructor, JsonConstructor]
        public TimeToLive()
        { }
        
        /// <summary>Creates a time-to-live header.</summary>
        /// <param name="secondsToLive">Maximum delivery lifetime in seconds.</param>
        public TimeToLive(int secondsToLive)
        {
            SecondsToLive = secondsToLive;
        }
        
        /// <summary>Gets or sets the maximum delivery lifetime in seconds.</summary>
        public int SecondsToLive { get; set; }
    }
}