using Stateflows.StateMachines;
using Stateflows.Testing.StateMachines.Sequence;

namespace Stateflows.Testing.StateMachines
{
    public static class IStateMachineInitialBuilderExtensions
    {
        public static IStateMachineBuilder AddExecutionSequenceObserver(this IStateMachineBuilder builder)
            => builder.AddObserver<ExecutionSequenceObserver>();
    }
}
