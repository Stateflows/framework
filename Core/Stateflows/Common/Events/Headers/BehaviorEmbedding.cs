namespace Stateflows.Common;

internal class BehaviorEmbedding : EventHeader
{
    public BehaviorId OwnerId { get; set; }
    public BehaviorId ParentId { get; set; }
}