namespace Stateflows.StateMachines
{
    public interface ITransitionContext
    {
        IVertexContext Source { get; }

        IVertexContext Target { get; }
    }
}
