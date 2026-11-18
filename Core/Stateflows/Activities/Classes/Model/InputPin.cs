using System.Collections.Generic;
using Stateflows.Common;
using Stateflows.Payloads;

namespace Stateflows.Activities;

/// <summary>A typed input boundary in an Activity scope.</summary>
public sealed class InputPin<TToken> : IActivityNode
{
    public static string Name => ActivityNode<InputPin<TToken>>.Name;
}

/// <summary>Name helpers for input pins declared with payload token names.</summary>
/// <seealso href="https://github.com/Stateflows/framework/wiki/4.6.-Payloads" />
public static class InputPin
{
    /// <summary>Gets the default node name for the given payload token names.</summary>
    /// <param name="tokenNames">Payload names accepted by the node; order does not matter.</param>
    /// <returns><c>InputPin&lt;Payload&gt;.Name</c> followed by the ordered name set in brackets, for example
    /// <c>InputPin&lt;Payload&gt;[a|b]</c>.</returns>
    public static string GetName(IReadOnlyList<string> tokenNames)
        => $"{InputPin<Payload>.Name}[{PayloadNameSet.Key(PayloadNameSet.Validate(tokenNames, nameof(tokenNames)))}]";
}