namespace Stateflows.Activities.Context.Interfaces
{
    public interface ITypedActionContext : IActivityNodeContext
    { }

    public interface IActionContext : ITypedActionContext, IInput, IActionOutput
    { }

    public interface IActionContext<out TToken> : IActionContext, ITokenContext<TToken>
    { }
}
