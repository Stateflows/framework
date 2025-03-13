namespace Stateflows.Common.Context.Interfaces
{
    public interface IBehaviorActionContext : IBehaviorLocator, IExecutionContext
    {
        IBehaviorContext Behavior { get; }
    }
}
