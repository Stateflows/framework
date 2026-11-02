namespace Stateflows.Activities;

/// <summary>A typed output boundary and object-flow target in an Activity scope.</summary>
public sealed class OutputPin<TToken> : IActivityNode
{
    public static string Name => ActivityNode<OutputPin<TToken>>.Name;
}
