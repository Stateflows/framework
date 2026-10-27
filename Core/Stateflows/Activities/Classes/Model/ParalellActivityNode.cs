namespace Stateflows.Activities
{
    /// <summary>Runtime model element representing a parallel structured activity node.</summary>
    /// <typeparam name="TToken">The token type used by the parallel activity.</typeparam>
    public sealed class ParallelActivityNode<TToken> : IStructuredActivityNode
    {
        /// <summary>Gets the registered model name of this node type.</summary>
        public static string Name => ActivityNode<ParallelActivityNode<TToken>>.Name;
    }
}
