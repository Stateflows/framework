namespace Stateflows.Activities
{
    /// <summary>Runtime model element representing a structured activity node.</summary>
    public sealed class StructuredActivityNode : IStructuredActivityNode
    {
        /// <summary>Gets the registered model name of this node type.</summary>
        public static string Name => ActivityNode<StructuredActivityNode>.Name;
    }
}
