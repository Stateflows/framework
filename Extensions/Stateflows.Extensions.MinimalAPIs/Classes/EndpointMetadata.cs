using Stateflows.Common;

namespace Stateflows.Extensions.MinimalAPIs;

public class EndpointMetadata
{
    public string? ScopeName { get; set; }
    public BehaviorClass BehaviorClass { get; set; }
    public string Pattern { get; set; }
    public bool RequireContext { get; set; }
    public Dictionary<string, List<(HateoasLink, BehaviorStatus[])>> HateoasLinks { get; set; }
    
    public AsyncLocal<BehaviorEndpointContext> BehaviorContext { get; set; } = new();
    public AsyncLocal<StateMachineEndpointContext> StateMachineContext { get; set; } = new();
    public AsyncLocal<StateEndpointContext> StateContext { get; set; } = new();
    public AsyncLocal<ActivityEndpointContext> ActivityContext { get; set; } = new();
    public AsyncLocal<ActivityNodeEndpointContext> ActivityNodeContext { get; set; } = new();
    public AsyncLocal<ActionEndpointContext> ActionContext { get; set; } = new();
}