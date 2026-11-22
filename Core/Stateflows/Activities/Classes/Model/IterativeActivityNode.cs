using System.Collections.Generic;
using Stateflows.Common;
using Stateflows.Payloads;

namespace Stateflows.Activities
{
    /// <summary>Runtime model element representing a structured activity that iterates over tokens.</summary>
    /// <typeparam name="TToken">The token type used to drive iteration.</typeparam>
    public sealed class IterativeActivityNode<TToken> : IStructuredActivityNode
    {
        /// <summary>Gets the registered model name of this node type.</summary>
        public static string Name => ActivityNode<IterativeActivityNode<TToken>>.Name;
    }

    /// <summary>Name helpers for iterative activity nodes declared with payload token names.</summary>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/4.6.-Payloads" />
    public static class IterativeActivityNode
    {
        /// <summary>Gets the default node name for the given payload token names.</summary>
        /// <param name="tokenNames">Payload names accepted by the node; order does not matter.</param>
        /// <returns><c>IterativeActivityNode&lt;Payload&gt;.Name</c> followed by the ordered name set in brackets, for example
        /// <c>IterativeActivityNode&lt;Payload&gt;[a|b]</c>.</returns>
        public static string GetName(IReadOnlyList<string> tokenNames)
            => $"{IterativeActivityNode<Payload>.Name}[{PayloadNameSet.Key(PayloadNameSet.Validate(tokenNames, nameof(tokenNames)))}]";
    }
}
