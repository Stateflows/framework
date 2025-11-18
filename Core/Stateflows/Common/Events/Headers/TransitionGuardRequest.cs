using Stateflows.StateMachines.Models;

namespace Stateflows.Common
{
    internal sealed class TransitionGuardRequest : EventHeader
    {
        public string GuardIdentifier { get; set; }
        public EdgeType EdgeType { get; set; }
        public string SourceName { get; set; }
        public string TargetName { get; set; }
    }
    
    internal sealed class TransitionGuardResponse : EventHeader
    {
        public string GuardIdentifier { get; set; }
    }
}