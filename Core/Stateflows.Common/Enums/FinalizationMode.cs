namespace Stateflows.Common
{
    /// <summary>Specifies how a behavior finalization event is delivered.</summary>
    public enum FinalizationMode
    {
        /// <summary>Queues finalization through normal behavior event processing.</summary>
        Queued,
        /// <summary>Processes finalization immediately.</summary>
        Immediate
    }
}
