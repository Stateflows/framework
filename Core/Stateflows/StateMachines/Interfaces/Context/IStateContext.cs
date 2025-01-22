using Stateflows.Common.Interfaces;

namespace Stateflows.StateMachines
{
    public interface IStateContext
    {
        string Name { get; }

        IContextValues Values { get; }

        bool TryGetParent(out IStateContext parentStateContext);
    }
}
