namespace Stateflows.Activities
{
    /// <summary>Runtime model element representing a control-flow decision node.</summary>
    public sealed class ControlDecisionNode : IActivityNode
    {
        /// <summary>Gets the registered model name of this node type.</summary>
        public static string Name => ActivityNode<ControlDecisionNode>.Name;
    }
}
