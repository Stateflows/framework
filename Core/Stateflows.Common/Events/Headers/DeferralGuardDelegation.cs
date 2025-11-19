namespace Stateflows.Common
{
    public sealed class DeferralGuardDelegation : EventHeader
    {
        public string VertexIdentifier { get; set; }
        public string EventName { get; set; }
    }
}