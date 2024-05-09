namespace Stateflows.Activities.Context.Interfaces
{
    public interface ITypedActionContext : IActivityNodeContext
    { }

    public interface IActionContext : ITypedActionContext, IInput, IOutput
    { }

    public interface IActionContext<out TToken> : IActionContext//, ITokenContext<TToken>
        // where TToken : Token, new()
    { }
}
