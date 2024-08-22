namespace Stateflows.StateMachines
{
    public interface ITransitionContext
    {
        IStateContext SourceState { get; }

        IStateContext TargetState { get; }
    }
}
