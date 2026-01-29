using Stateflows.Examples.Common.Headers;
using Stateflows.StateMachines;

namespace Stateflows.Examples.Behaviors.StateMachines.Document.Guards;

public class IsManager(IExecutionContext executionContext) : ITransitionGuard
{
    public Task<bool> GuardAsync()
        => Task.FromResult(executionContext.Headers.Values.Any(h => h is Manager));
}