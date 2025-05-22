using Stateflows.Activities;
using Stateflows.Common;
using Stateflows.Common.Exceptions;
using Stateflows.Extensions.MinimalAPIs.Interfaces;
using Stateflows.StateMachines;

namespace Stateflows.Extensions.MinimalAPIs;

public class BehaviorEndpointContext : IBehaviorEndpointContext
{
    public bool TryLocateBehavior(BehaviorId id, out IBehavior behavior)
        => Locator.TryLocateBehavior(id, out behavior);
    
    internal IBehaviorLocator Locator { get; set; }
    internal IEnumerable<string> ExpectedEventNames { get; set; }
    internal BehaviorStatus Status { get; set; }
    internal Dictionary<string, List<HateoasLink>> CustomHateoasLinks { get; set; }
    internal IReadOnlyTree<string> ElementsTree { get; set; }

    public IBehaviorContext Behavior { get; internal set; }

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

    public EndpointResponse Response()
    {
        var behaviorInfo = GetBehaviorInfo();

        return new EndpointResponse(behaviorInfo.ToHateoasLinks(CustomHateoasLinks), behaviorInfo.ToMetadata());
    }

    public EndpointResponse<T> Response<T>(T result)
    {
        var behaviorInfo = GetBehaviorInfo();

        return new EndpointResponse<T>(result, behaviorInfo.ToHateoasLinks(CustomHateoasLinks), behaviorInfo.ToMetadata());
    }
}