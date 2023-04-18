using Stateflows.StateMachines;
using Stateflows.Testing.StateMachines.Sequence;

namespace Stateflows.Testing.StateMachines
{
    public static class IStateMachineInitialBuilderExtensions
    {
        public static IStateMachineInitialBuilder AddExecutionSequenceObserver(this IStateMachineInitialBuilder builder)
            => builder.AddObserver<ExecutionSequenceObserver>();
    }
}
