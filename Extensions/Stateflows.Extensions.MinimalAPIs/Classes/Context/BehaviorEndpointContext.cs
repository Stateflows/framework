using Stateflows.Activities;
using Stateflows.Common;
using Stateflows.Common.Exceptions;
using Stateflows.Extensions.MinimalAPIs.Interfaces;
using Stateflows.StateMachines;
using IBehaviorContext = Stateflows.Extensions.MinimalAPIs.Interfaces.IBehaviorContext;

namespace Stateflows.Extensions.MinimalAPIs;

/// <summary>Builds custom endpoint responses using the behavior configuration supplied by the endpoint filter.</summary>
public class BehaviorEndpointContext : IBehaviorEndpointContext
{
    /// <inheritdoc/>
    public bool TryLocateBehavior(BehaviorId id, out IBehavior behavior)
        => Locator.TryLocateBehavior(id, out behavior);
    
    internal IBehaviorLocator Locator { get; set; }
    internal IEnumerable<string> ExpectedEventNames { get; set; }
    internal BehaviorStatus Status { get; set; }
    internal Dictionary<string, List<(HateoasLink, BehaviorStatus[])>> HateoasLinks { get; set; }
    internal IReadOnlyTree<string> ElementsTree { get; set; }
    IBehaviorContext IBehaviorEndpointContext.Behavior => Behavior;
    internal BehaviorContext Behavior { get; set; }

    /// <summary>Builds a status snapshot from the endpoint filter's behavior metadata.</summary>
    /// <returns>A State Machine, Activity, or general Action status snapshot.</returns>
    /// <exception cref="StateflowsRuntimeException">The behavior family is not supported by this context.</exception>
    protected BehaviorInfo GetBehaviorInfo()
    {
        return Behavior.Id.Type switch
        {
            BehaviorType.StateMachine => new StateMachineInfo()
            {
                Id = Behavior.Id,
                CurrentStates = ElementsTree,
                ExpectedEvents = ExpectedEventNames,
                BehaviorStatus = Status
            },
            BehaviorType.Activity => new ActivityInfo()
            {
                Id = Behavior.Id,
                ActiveNodes = ElementsTree,
                ExpectedEvents = ExpectedEventNames,
                BehaviorStatus = Status
            },
            BehaviorType.Action => new BehaviorInfo()
            {
                Id = Behavior.Id,
                ExpectedEvents = ExpectedEventNames,
                BehaviorStatus = Status
            },
            _ => throw new StateflowsRuntimeException("Unknown behavior type")
        };
    }

    /// <inheritdoc/>
    public EndpointResponse Response()
    {
        var behaviorInfo = GetBehaviorInfo();
        var metadata = behaviorInfo.ToMetadata();
        
        return new EndpointResponse(behaviorInfo.ToHateoasLinks("", "", HateoasLinks), metadata);
    }

    /// <inheritdoc/>
    public EndpointResponse<T> Response<T>(T result)
    {
        var behaviorInfo = GetBehaviorInfo();
        var metadata = behaviorInfo.ToMetadata();
        
        return new EndpointResponse<T>(result, behaviorInfo.ToHateoasLinks("", "", HateoasLinks), metadata);
    }
}