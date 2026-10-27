namespace Stateflows.Activities
{
    /// <summary>Runtime model element representing a fork node that splits control flow.</summary>
    public sealed class ForkNode : IActivityNode
    {
        /// <summary>Gets the registered model name of this node type.</summary>
        public static string Name => ActivityNode<ForkNode>.Name;
    }
}
