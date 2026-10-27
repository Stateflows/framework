namespace Stateflows.Activities
{
    /// <summary>Runtime model element representing a data-store node in an activity.</summary>
    public sealed class DataStoreNode : IActivityNode
    {
        /// <summary>Gets the registered model name of this node type.</summary>
        public static string Name => ActivityNode<DataStoreNode>.Name;
    }
}
