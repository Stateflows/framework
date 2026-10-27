namespace Stateflows.Activities
{
    /// <summary>Runtime model element representing an output node in a structured activity.</summary>
    public sealed class OutputNode : IActivityNode
    {
        /// <summary>Gets the registered model name of this node type.</summary>
        public static string Name => ActivityNode<OutputNode>.Name;
    }
}
