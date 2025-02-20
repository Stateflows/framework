namespace Stateflows.Activities
{
    public sealed class DecisionNode<TToken> : IActivityNode
    {
        public static string Name => ActivityNode<DecisionNode<TToken>>.Name;
    }
}
