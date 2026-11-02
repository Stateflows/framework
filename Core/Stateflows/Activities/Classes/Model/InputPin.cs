namespace Stateflows.Activities;

/// <summary>A typed input boundary in an Activity scope.</summary>
public sealed class InputPin<TToken> : IActivityNode
{
    public static string Name => ActivityNode<InputPin<TToken>>.Name;
}
