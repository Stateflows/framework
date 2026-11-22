using System.Threading.Tasks;
using Stateflows.Common.Models;
using Stateflows.StateMachines.Interfaces;
using Stateflows.StateMachines.Context.Classes;

namespace Stateflows.StateMachines
{
    internal static class ActionStateMachinePredicateAsyncExtensions
    {
        public static async Task<bool> IterateOverAsync(this Logic<StateMachinePredicateAsync> action, RootContext context)
        {
            var hit = true;
            for (var index = 0; index < action.Actions.Count; index++)
            {
                var handler = action.Actions[index];
                var result = action.Registrations.TryGetValue(index, out var registration)
                    ? await registration.ObserveAsync(context, () => handler(context))
                    : await handler(context);
                if (!result)
                {
                    hit = false;
                    break;
                }
            }

            return hit;
        }
    }
}
