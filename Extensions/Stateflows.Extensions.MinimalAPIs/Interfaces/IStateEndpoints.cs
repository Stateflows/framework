using Stateflows.Extensions.MinimalAPIs;

namespace Stateflows.StateMachines;

public interface IStateEndpoints : IState
{
    static abstract void RegisterEndpoints(IEndpointsBuilder endpointsBuilder);
}

public interface ICompositeStateEndpoints : IStateEndpoints;

public interface IOrthogonalStateEndpoints : ICompositeStateEndpoints;