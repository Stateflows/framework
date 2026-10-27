namespace Stateflows.Activities.Context.Interfaces
{
    /// <summary>Context supplied to a control-flow guard, without a strongly typed token.</summary>
    public interface IGuardContext : IActivityFlowContext
    { }

    /// <summary>Context supplied to a guard evaluating a typed object flow.</summary>
    /// <typeparam name="TToken">The token type considered by the guard.</typeparam>
    public interface IGuardContext<out TToken> : IActivityFlowContext<TToken>
    { }
}
