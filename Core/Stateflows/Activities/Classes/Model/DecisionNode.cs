namespace Stateflows.Activities
{
    /// <summary>Runtime model element representing a decision node that evaluates object-token flows.</summary>
    /// <typeparam name="TToken">The token type used by the decision node.</typeparam>
    public sealed class DecisionNode<TToken> : IActivityNode
    {
        /// <summary>Gets the registered model name of this node type.</summary>
        public static string Name => ActivityNode<DecisionNode<TToken>>.Name;
    }
}
