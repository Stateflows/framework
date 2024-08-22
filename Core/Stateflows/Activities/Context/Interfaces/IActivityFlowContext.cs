namespace Stateflows.Activities.Context.Interfaces
{
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
