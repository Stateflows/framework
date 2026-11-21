using Orleans;
using Orleans.Runtime;

namespace Stateflows;

/// <summary>
/// An Orleans runtime prerequisite failed. The underlying infrastructure error is retained as the inner exception.
/// </summary>
[GenerateSerializer]
public sealed class StateflowsInfrastructureException(string message, Exception innerException)
    : OrleansException(message, innerException)
;
