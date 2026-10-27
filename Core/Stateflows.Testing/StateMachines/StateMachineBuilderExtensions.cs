using Microsoft.Extensions.DependencyInjection;
using Stateflows.StateMachines;
using Stateflows.Testing.StateMachines.Sequence;

namespace Stateflows.Testing.StateMachines
{
    /// <summary>Adds test-only extensions for state-machine execution.</summary>
    public static class StateMachineBuilderExtensions
    {
        /// <summary>Registers an observer that records state lifecycle hooks and transition guard/effect execution.</summary>
        /// <param name="builder">The state-machine builder to instrument.</param>
        /// <returns>The state-machine builder returned by the observer registration.</returns>
        public static IStateMachineBuilder AddExecutionSequenceObserver(this IStateMachineBuilder builder)
            => builder.AddObserver((serviceProvider, _) => serviceProvider.GetRequiredService<ExecutionSequenceObserver>());
    }
}
