namespace Stateflows.StateMachines
{
    public interface ITransitionContext
    {
        IStateContext Source { get; }

        IStateContext Target { get; }
    }
}
