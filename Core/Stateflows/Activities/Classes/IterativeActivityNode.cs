namespace Stateflows.Activities
{
    public sealed class IterativeActivityNode<TToken> : IStructuredActivityNode
    {
        public static string Name => ActivityNode<IterativeActivityNode<TToken>>.Name;
    }
}
