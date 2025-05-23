namespace Stateflows.Activities.Registration.Interfaces
{
    public interface INodeBuilder
    {
        string Name { get; }
        NodeType Type { get; }
    }
}