namespace Stateflows.Activities.Context.Interfaces
{
    /// <summary>Activity-node action context with activity-specific token input and output.</summary>
    public interface ITypedActionContext : IActivityNodeContext;

    /// <summary>Context supplied to an activity action, including its node, input tokens, and output operations.</summary>
    public interface IActionContext : ITypedActionContext, IInput, IOutput;

    /// <summary>Action context whose incoming token values are exposed as <typeparamref name="TToken"/>.</summary>
    /// <typeparam name="TToken">The token type available to the action.</typeparam>
    public interface IActionContext<out TToken> : IActionContext, ITokenContext<TToken>;
}
