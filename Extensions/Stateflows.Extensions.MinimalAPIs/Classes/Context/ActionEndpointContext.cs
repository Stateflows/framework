using Stateflows.Extensions.MinimalAPIs.Interfaces;

namespace Stateflows.Extensions.MinimalAPIs;

/// <summary>Implements the request-scoped context supplied to custom Action endpoints.</summary>
public class ActionEndpointContext : BehaviorEndpointContext, IActionEndpointContext
{ }