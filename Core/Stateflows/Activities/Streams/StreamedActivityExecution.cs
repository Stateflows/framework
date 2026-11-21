namespace Stateflows.Activities.Streams;

internal sealed class StreamedActivityExecution : Stateflows.Common.EventHeader
{
    /// <summary>
    /// Set when the live stream pump delivers an event whose invocation uses transactional execution provenance.
    /// </summary>
    public bool TransactionalSource { get; set; }
}