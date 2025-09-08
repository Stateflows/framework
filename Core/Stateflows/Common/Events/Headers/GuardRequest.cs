using Stateflows.StateMachines.Models;

namespace Stateflows.Common
{
    internal sealed class GuardRequest : EventHeader
    {
        public string GuardIdentifier { get; set; }
        public EdgeType EdgeType { get; set; }
        public string SourceName { get; set; }
        public string TargetName { get; set; }
    }
    
    internal sealed class GuardResponse : EventHeader
    {
        public string GuardIdentifier { get; set; }
    }
}