namespace Stateflows.Activities
{
    /// <summary>Runtime model element representing a merge node for alternative control flows.</summary>
    public sealed class MergeNode : IActivityNode
    {
        /// <summary>Gets the registered model name of this node type.</summary>
        public static string Name => ActivityNode<MergeNode>.Name;
    }
}
