using Stateflows.Extensions.MinimalAPIs.Interfaces;

namespace Stateflows.Extensions.MinimalAPIs;

public class StateContext : IStateContext
{
    public string Name { get; internal set; }
}
