namespace Stateflows.Activities
{
    /// <summary>Runtime model element representing a structured activity that iterates over tokens.</summary>
    /// <typeparam name="TToken">The token type used to drive iteration.</typeparam>
    public sealed class IterativeActivityNode<TToken> : IStructuredActivityNode
    {
        /// <summary>Gets the registered model name of this node type.</summary>
        public static string Name => ActivityNode<IterativeActivityNode<TToken>>.Name;
    }
}
