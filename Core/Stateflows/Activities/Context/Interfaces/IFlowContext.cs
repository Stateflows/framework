using Stateflows.Common;

namespace Stateflows.Activities.Context.Interfaces
{
    public interface IFlowContext : IActivityActionContext
    {
        ISourceNodeContext SourceNode { get; }

        INodeContext TargetNode { get; }
    }

    public interface IFlowContext<out TToken> : IFlowContext
        where TToken : Token, new()
    {
        TToken Token { get; }
    }
}
