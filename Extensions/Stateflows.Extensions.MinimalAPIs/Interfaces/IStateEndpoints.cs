using Stateflows.Extensions.MinimalAPIs;

namespace Stateflows.StateMachines;

/// <summary>Defines custom HTTP endpoints available while a typed state is active.</summary>
public interface IStateEndpoints : IState
{
    /// <summary>Maps state-scoped custom endpoints during API mapping.</summary>
    /// <param name="endpointsBuilder">Builder for routes filtered by the state's active configuration.</param>
    static abstract void RegisterEndpoints(IEndpointsBuilder endpointsBuilder);
}

/// <summary>Marks a composite state that supplies state-scoped HTTP endpoints.</summary>
public interface ICompositeStateEndpoints : IStateEndpoints;

/// <summary>Marks an orthogonal state that supplies state-scoped HTTP endpoints.</summary>
public interface IOrthogonalStateEndpoints : ICompositeStateEndpoints;