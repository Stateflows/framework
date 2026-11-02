namespace Stateflows.Activities.Streams;

internal sealed class StreamedActivityExecution : Stateflows.Common.EventHeader
{
    /// <summary>
    /// Set when the live stream pump only delivers the event (for example a due live time event) while its source is
    /// transactional; the invocation then runs as a regular transaction instead of a streamed one.
    /// </summary>
    public bool TransactionalSource { get; set; }
}