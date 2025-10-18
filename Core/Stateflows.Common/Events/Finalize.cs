namespace Stateflows.Common
{
    [NoForwarding, NoBubbling, NoImplicitInitialization]
    public sealed class Finalize
    {
        public FinalizationMode Mode { get; set; } = FinalizationMode.Immediate;
    }
}
