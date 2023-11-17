using Stateflows.StateMachines;
using Stateflows.Testing.StateMachines.Sequence;

namespace Stateflows.Testing.StateMachines
{
    public static class IStateMachineBuilderExtensions
    {
        public static IStateMachineBuilder AddExecutionSequenceObserver(this IStateMachineBuilder builder)
            => builder.AddObserver<ExecutionSequenceObserver>();
    }
}
