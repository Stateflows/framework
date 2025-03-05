using Stateflows.StateMachines.Models;

namespace Stateflows.StateMachines.Registration.Interfaces
{
    public interface IVertexBuilder
    {
        string Name { get; }
        VertexType Type { get; }
    }
}