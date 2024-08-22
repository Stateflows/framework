using Stateflows.Common.Interfaces;

namespace Stateflows.StateMachines
{
    public interface IStateContext
    {
        string Name { get; }

        IContextValues Values { get; }

        bool TryGetParentState(out IStateContext parentStateContext);
    }
}
