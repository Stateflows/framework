using Stateflows.Common;

namespace Stateflows.Extensions.MinimalAPIs;

/// <summary>Associates custom routes with behavior identity, scope, links, and request-local context slots.</summary>
public class EndpointMetadata
{
    /// <summary>Gets or sets the state/node scope name, or null for a behavior-wide endpoint.</summary>
    public string? ScopeName { get; set; }
    /// <summary>Gets or sets the behavior class addressed by the route.</summary>
    public BehaviorClass BehaviorClass { get; set; }
    /// <summary>Gets or sets the custom route suffix supplied during registration.</summary>
    public string Pattern { get; set; }
    /// <summary>Gets or sets whether the handler declares a Stateflows endpoint context parameter.</summary>
    public bool RequireContext { get; set; }
    /// <summary>Gets or sets operation links and their supported lifecycle statuses by scope.</summary>
    public Dictionary<string, List<(HateoasLink, BehaviorStatus[])>> HateoasLinks { get; set; }
    
    /// <summary>Gets or sets the asynchronous-flow-local general endpoint context slot.</summary>
    public AsyncLocal<BehaviorEndpointContext> BehaviorContext { get; set; } = new();
    /// <summary>Gets or sets the asynchronous-flow-local State Machine context slot.</summary>
    public AsyncLocal<StateMachineEndpointContext> StateMachineContext { get; set; } = new();
    /// <summary>Gets or sets the asynchronous-flow-local state-scoped context slot.</summary>
    public AsyncLocal<StateEndpointContext> StateContext { get; set; } = new();
    /// <summary>Gets or sets the asynchronous-flow-local Activity context slot.</summary>
    public AsyncLocal<ActivityEndpointContext> ActivityContext { get; set; } = new();
    /// <summary>Gets or sets the asynchronous-flow-local node-scoped context slot.</summary>
    public AsyncLocal<ActivityNodeEndpointContext> ActivityNodeContext { get; set; } = new();
    /// <summary>Gets or sets the asynchronous-flow-local Action context slot.</summary>
    public AsyncLocal<ActionEndpointContext> ActionContext { get; set; } = new();
}