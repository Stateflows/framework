using System.Collections.Generic;
using Stateflows.Common;
using Stateflows.Payloads;

namespace Stateflows.Activities
{
    /// <summary>Runtime model element representing a decision node that evaluates object-token flows.</summary>
    /// <typeparam name="TToken">The token type used by the decision node.</typeparam>
    public sealed class DecisionNode<TToken> : IActivityNode
    {
        /// <summary>Gets the registered model name of this node type.</summary>
        public static string Name => ActivityNode<DecisionNode<TToken>>.Name;
    }

    /// <summary>Name helpers for decision nodes declared with payload token names.</summary>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/4.6.-Payloads" />
    public static class DecisionNode
    {
        /// <summary>Gets the default node name for the given payload token names.</summary>
        /// <param name="tokenNames">Payload names accepted by the node; order does not matter.</param>
        /// <returns><c>DecisionNode&lt;Payload&gt;.Name</c> followed by the ordered name set in brackets, for example
        /// <c>DecisionNode&lt;Payload&gt;[a|b]</c>.</returns>
        public static string GetName(IReadOnlyList<string> tokenNames)
            => $"{DecisionNode<Payload>.Name}[{PayloadNameSet.Key(PayloadNameSet.Validate(tokenNames, nameof(tokenNames)))}]";
    }
}
