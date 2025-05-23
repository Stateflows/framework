namespace Stateflows.Common
{
    public interface IBehaviorActionContext : IBehaviorLocator, IExecutionContext
    {
        IBehaviorContext Behavior { get; }
    }
}
