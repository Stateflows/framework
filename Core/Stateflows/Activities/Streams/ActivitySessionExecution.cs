namespace Stateflows.Activities.Streams;

internal sealed class ActivitySessionExecution : Stateflows.Common.EventHeader
{
    public bool Checkpoint { get; set; }
}
