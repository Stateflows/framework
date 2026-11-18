using System.Collections.Generic;
using Stateflows.Common;
using Stateflows.Payloads;

namespace Stateflows.Activities;

/// <summary>A typed output boundary and object-flow target in an Activity scope.</summary>
public sealed class OutputPin<TToken> : IActivityNode
{
    public static string Name => ActivityNode<OutputPin<TToken>>.Name;
}

/// <summary>Name helpers for output pins declared with payload token names.</summary>
/// <seealso href="https://github.com/Stateflows/framework/wiki/4.6.-Payloads" />
public static class OutputPin
{
    /// <summary>Gets the default node name for the given payload token names.</summary>
    /// <param name="tokenNames">Payload names accepted by the node; order does not matter.</param>
    /// <returns><c>OutputPin&lt;Payload&gt;.Name</c> followed by the ordered name set in brackets, for example
    /// <c>OutputPin&lt;Payload&gt;[a|b]</c>.</returns>
    public static string GetName(IReadOnlyList<string> tokenNames)
        => $"{OutputPin<Payload>.Name}[{PayloadNameSet.Key(PayloadNameSet.Validate(tokenNames, nameof(tokenNames)))}]";
}