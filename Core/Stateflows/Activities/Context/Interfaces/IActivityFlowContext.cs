using System;

namespace Stateflows.Activities.Context.Interfaces
{
    public interface IFlowContext
    {
        ISourceNodeContext SourceNode { get; }

        INodeContext TargetNode { get; }

        Type TokenType { get; }

        int Weight { get; }
    }

    public interface IIncomingFlowContext : IFlowContext
    {
        bool Activated { get; }

        int TokenCount { get; }
    }

    public interface IFlowTokenContext<out TToken>
    {
        TToken Token { get; }
    }

    public interface IActivityFlowContext : IActivityActionContext, IFlowContext
    { }

    public interface IActivityFlowContext<out TToken> : IActivityFlowContext, IFlowTokenContext<TToken>
    { }
}
