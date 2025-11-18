namespace Stateflows.Common
{
    internal sealed class DeferralGuardRequest : EventHeader
    {
        public string GuardIdentifier { get; set; }
        public string StateName { get; set; }
    }
    
    internal sealed class DeferralGuardResponse : EventHeader
    {
        public string GuardIdentifier { get; set; }
    }
}