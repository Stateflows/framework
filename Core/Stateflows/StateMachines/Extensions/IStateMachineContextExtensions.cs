using Stateflows.StateMachines.Engine;
using Stateflows.StateMachines.Context.Classes;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines.Extensions
{
    internal static class IStateMachineContextExtensions
    {
        public static Executor GetExecutor(this IStateMachineContext context)
            => (context as StateMachineContext).Context.Executor;
    }
}
