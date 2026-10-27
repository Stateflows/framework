namespace Stateflows.Common
{
    /// <summary>Requests finalization of the receiving behavior instance.</summary>
    [NoForwarding, NoBubbling, NoImplicitInitialization]
    public sealed class Finalize
    {
        /// <summary>Gets or sets whether finalization is immediate or deferred.</summary>
        public FinalizationMode Mode { get; set; } = FinalizationMode.Immediate;
    }
}
