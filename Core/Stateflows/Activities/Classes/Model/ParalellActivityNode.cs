namespace Stateflows.Activities
{
    public sealed class ParallelActivityNode<TToken> : IStructuredActivityNode
    {
        public static string Name => ActivityNode<ParallelActivityNode<TToken>>.Name;
    }
}
