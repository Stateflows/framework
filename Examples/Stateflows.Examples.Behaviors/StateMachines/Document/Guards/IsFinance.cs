using Stateflows.Examples.Common.Headers;
using Stateflows.StateMachines;

namespace Stateflows.Examples.Behaviors.StateMachines.Document.Guards;

public class IsFinance(IExecutionContext executionContext) : ITransitionGuard
{
    public Task<bool> GuardAsync()
        => Task.FromResult(executionContext.Headers.Any(h => h is Finance));
}