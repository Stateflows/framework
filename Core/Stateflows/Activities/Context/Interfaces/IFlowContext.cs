namespace Stateflows.Activities.Context.Interfaces
{
    public interface IFlowContext : IActivityActionContext
    {
        ISourceNodeContext SourceNode { get; }

        INodeContext TargetNode { get; }
    }

    public interface IFlowContext<out TToken> : IFlowContext
    {
        TToken Token { get; }
    }
}
