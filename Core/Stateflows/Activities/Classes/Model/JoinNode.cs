namespace Stateflows.Activities
{
    /// <summary>Runtime model element representing a join node that synchronizes control flows.</summary>
    public sealed class JoinNode : IActivityNode
    {
        /// <summary>Gets the registered model name of this node type.</summary>
        public static string Name => ActivityNode<JoinNode>.Name;
    }
}
