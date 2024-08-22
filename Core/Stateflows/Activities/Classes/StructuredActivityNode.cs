namespace Stateflows.Activities
{
    public sealed class StructuredActivityNode : ActivityNode, IStructuredActivityNode
    {
        public static string Name => ActivityNode<StructuredActivityNode>.Name;
    }
}
