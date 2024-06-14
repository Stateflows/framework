namespace Stateflows.Activities.Context.Interfaces
{
    public interface IGuardContext : IActivityFlowContext
    { }

    public interface IGuardContext<out TToken> : IActivityFlowContext<TToken>
    { }
}
