using Stateflows.Common.Context;

namespace Stateflows.Common.Interfaces
{
    public interface IStateflowsContextProvider
    {
        public StateflowsContext Context { get; }
    }
}
