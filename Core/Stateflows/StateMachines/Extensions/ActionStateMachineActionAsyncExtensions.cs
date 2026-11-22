using System.Linq;
using System.Threading.Tasks;
using Stateflows.Common.Models;
using Stateflows.StateMachines.Interfaces;
using Stateflows.StateMachines.Context.Classes;

namespace Stateflows.StateMachines
{
    internal static class ActionStateMachineActionAsyncExtensions
    {
        public static async Task IterateOverAsync(this Logic<StateMachineActionAsync> action, RootContext context)
        {
            for (var index = 0; index < action.Actions.Count; index++)
            {
                var handler = action.Actions[index];
                if (action.Registrations.TryGetValue(index, out var registration))
                    await context.Executor.Inspector.OnActionAsync(new ActionObservationContext(context, registration), () => handler(context));
                else await handler(context);
            }
        }
    }
}
