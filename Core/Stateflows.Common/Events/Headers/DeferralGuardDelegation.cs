namespace Stateflows.Common
{
    /// <summary>Delegates a deferred-event guard decision to a containing state vertex.</summary>
    public sealed class DeferralGuardDelegation : EventHeader
    {
        /// <summary>Gets or sets the identifier of the vertex that owns the deferred event.</summary>
        public string VertexIdentifier { get; set; }
        /// <summary>Gets or sets the name of the deferred event.</summary>
        public string EventName { get; set; }
    }
}