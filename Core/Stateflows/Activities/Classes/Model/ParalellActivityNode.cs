using System.Collections.Generic;
using Stateflows.Common;
using Stateflows.Payloads;

namespace Stateflows.Activities
{
    /// <summary>Runtime model element representing a parallel structured activity node.</summary>
    /// <typeparam name="TToken">The token type used by the parallel activity.</typeparam>
    public sealed class ParallelActivityNode<TToken> : IStructuredActivityNode
    {
        /// <summary>Gets the registered model name of this node type.</summary>
        public static string Name => ActivityNode<ParallelActivityNode<TToken>>.Name;
    }

    /// <summary>Name helpers for parallel activity nodes declared with payload token names.</summary>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/4.6.-Payloads" />
    public static class ParallelActivityNode
    {
        /// <summary>Gets the default node name for the given payload token names.</summary>
        /// <param name="tokenNames">Payload names accepted by the node; order does not matter.</param>
        /// <returns><c>ParallelActivityNode&lt;Payload&gt;.Name</c> followed by the ordered name set in brackets, for example
        /// <c>ParallelActivityNode&lt;Payload&gt;[a|b]</c>.</returns>
        public static string GetName(IReadOnlyList<string> tokenNames)
            => $"{ParallelActivityNode<Payload>.Name}[{PayloadNameSet.Key(PayloadNameSet.Validate(tokenNames, nameof(tokenNames)))}]";
    }
}
