using Stateflows.Common;

namespace Stateflows.Activities.Context.Interfaces
{
    public interface IGuardContext : IFlowContext
    { }

    public interface IGuardContext<out TToken> : IFlowContext<TToken>
        // where TToken : Token, new()
    { }
}
