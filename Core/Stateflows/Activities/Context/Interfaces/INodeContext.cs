namespace Stateflows.Activities.Context.Interfaces
{
    public interface INodeContext
    {
        string NodeName { get; }

        NodeType NodeType { get; }

        bool TryGetParentNode(out INodeContext parentNodeContext);
    }
}
