namespace Stateflows.Activities
{
    public sealed class ParallelActivityNode<TToken> : IBaseStructuredActivityNode
    {
        public static string Name => ActivityNode<ParallelActivityNode<TToken>>.Name;
    }
}
