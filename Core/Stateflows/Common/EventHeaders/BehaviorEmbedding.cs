namespace Stateflows.Common;

public class BehaviorEmbedding : EventHeader
{
    public BehaviorId OwnerId { get; set; }
    public BehaviorId ParentId { get; set; }
}