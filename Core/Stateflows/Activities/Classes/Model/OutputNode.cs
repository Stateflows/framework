namespace Stateflows.Activities
{
    /// <summary>Runtime model element representing an output node in a structured activity.</summary>
    [System.Obsolete("Use OutputPin<TToken> as a typed output flow target.")]
    public sealed class OutputNode : IActivityNode
    {
        /// <summary>Gets the registered model name of this node type.</summary>
        public static string Name => ActivityNode<OutputNode>.Name;
    }
}
