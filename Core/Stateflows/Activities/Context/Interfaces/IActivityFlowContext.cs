namespace Stateflows.Activities.Context.Interfaces
{
    public interface IIncomingFlowContext : IAfterFlowContext
    { }

    public interface IFlowTokenContext<out TToken>
    {
        TToken Token { get; }
    }

    public interface IActivityFlowContext : IActivityActionContext, IFlowContext
    { }
    
    public interface IActivityBeforeFlowContext : IActivityActionContext, IBeforeFlowContext
    { }
    
    public interface IActivityAfterFlowContext : IActivityActionContext, IAfterFlowContext
    { }

    public interface IActivityFlowContext<out TToken> : IActivityFlowContext, IFlowTokenContext<TToken>
    { }
}
