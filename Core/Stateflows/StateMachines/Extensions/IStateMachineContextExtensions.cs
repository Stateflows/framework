using Stateflows.Common.Context.Interfaces;
using Stateflows.StateMachines.Engine;
using Stateflows.StateMachines.Context.Classes;

namespace Stateflows.StateMachines.Extensions
{
    internal static class IStateMachineContextExtensions
    {
        public static Executor GetExecutor(this IBehaviorContext context)
            => ((StateMachineContext)context).Context.Executor;
    }
}
