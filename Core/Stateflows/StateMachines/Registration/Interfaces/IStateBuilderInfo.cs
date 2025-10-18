namespace Stateflows.StateMachines.Registration.Interfaces
{
    public interface IStateBuilderInfo
    {
        string Name { get; }
        VertexType Type { get; }
    }
}