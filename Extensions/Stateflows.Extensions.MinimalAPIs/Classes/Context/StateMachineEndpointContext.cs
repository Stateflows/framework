using Stateflows.Common;
using Stateflows.Extensions.MinimalAPIs.Interfaces;
using IStateContext = Stateflows.Extensions.MinimalAPIs.Interfaces.IStateContext;

namespace Stateflows.Extensions.MinimalAPIs;

/// <summary>Implements the request-scoped State Machine endpoint context populated by the endpoint filter.</summary>
public class StateMachineEndpointContext : BehaviorEndpointContext, IStateMachineEndpointContext
{
    private IReadOnlyTree<IStateContext>? currentStates;
    /// <inheritdoc/>
    public IReadOnlyTree<IStateContext> CurrentStates =>
        currentStates ??= (ElementsTree ?? new Tree<string>())
            .Translate<IStateContext>(name => new StateContext() { Name = name })
            .AsReadOnly();
}