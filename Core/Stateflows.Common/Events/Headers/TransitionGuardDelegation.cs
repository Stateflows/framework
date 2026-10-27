namespace Stateflows.Common
{
    /// <summary>Delegates a transition guard decision to another transition.</summary>
    public sealed class TransitionGuardDelegation : EventHeader
    {
        /// <summary>Gets or sets the identifier of the transition whose guard is delegated.</summary>
        public string EdgeIdentifier { get; set; }
    }
}