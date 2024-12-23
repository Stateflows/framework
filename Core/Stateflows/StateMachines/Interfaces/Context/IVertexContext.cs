using Stateflows.Common.Interfaces;

namespace Stateflows.StateMachines
{
    public interface IVertexContext
    {
        string Name { get; }

        IContextValues Values { get; }

        bool TryGetParent(out IVertexContext parentVertexContext);
    }
}
