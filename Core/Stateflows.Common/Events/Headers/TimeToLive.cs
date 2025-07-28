using System.Text.Json.Serialization;

namespace Stateflows.Common
{
    public sealed class TimeToLive : EventHeader
    {
        [Newtonsoft.Json.JsonConstructor, JsonConstructor]
        public TimeToLive()
        { }
        
        public TimeToLive(int secondsToLive)
        {
            SecondsToLive = secondsToLive;
        }
        
        public int SecondsToLive { get; set; }
    }
}