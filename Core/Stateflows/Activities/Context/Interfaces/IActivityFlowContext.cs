namespace Stateflows.Activities.Context.Interfaces
{
    /// <summary>Context for an incoming flow after its transfer has been processed.</summary>
    public interface IIncomingFlowContext : IAfterFlowContext
    { }

    /// <summary>Exposes the token currently being evaluated by a flow operation.</summary>
    /// <typeparam name="TToken">The flow token type.</typeparam>
    public interface IFlowTokenContext<out TToken>
    {
        /// <summary>The current token carried by the flow.</summary>
        TToken Token { get; }
    }

    /// <summary>Activity action context enriched with the flow being processed.</summary>
    public interface IActivityFlowContext : IActivityActionContext, IFlowContext
    { }
    
    /// <summary>Context available before a flow transfers tokens.</summary>
    public interface IActivityBeforeFlowContext : IActivityActionContext, IBeforeFlowContext
    { }
    
    /// <summary>Context available after a flow transfer has been attempted.</summary>
    public interface IActivityAfterFlowContext : IActivityActionContext, IAfterFlowContext
    { }

    /// <summary>Flow context that exposes the current token using a concrete token type.</summary>
    /// <typeparam name="TToken">The token type carried by the flow.</typeparam>
    public interface IActivityFlowContext<out TToken> : IActivityFlowContext, IFlowTokenContext<TToken>
    { }
}
