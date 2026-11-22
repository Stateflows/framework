namespace Stateflows.Activities
{
    /// <summary>The structure of a registered flow guard expression, preserving short-circuit order.</summary>
    public enum ActivityGuardKind
    {
        Leaf,
        And,
        Or
    }
}
