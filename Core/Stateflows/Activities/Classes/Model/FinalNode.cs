namespace Stateflows.Activities
{
    /// <summary>Runtime model element representing a final node in an activity.</summary>
    public sealed class FinalNode : IActivityNode
    {
        /// <summary>Gets the registered model name of this node type.</summary>
        public static string Name => ActivityNode<FinalNode>.Name;
    }
}
