namespace Stateflows.Activities
{
    public sealed class IterativeActivityNode<TToken> : IBaseStructuredActivityNode
    {
        public static string Name => ActivityNode<IterativeActivityNode<TToken>>.Name;
    }
}
