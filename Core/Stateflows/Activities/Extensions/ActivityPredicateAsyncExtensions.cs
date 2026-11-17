using System.Threading.Tasks;
using Stateflows.Common.Models;
using Stateflows.Activities.Models;
using Stateflows.Activities.Context.Classes;

namespace Stateflows.Activities
{
    internal static class ActivityPredicateAsyncExtensions
    {
        public static async Task<bool> IterateOverAsync(this Logic<ActivityPredicateAsync> action, BaseContext context)
        {
            var hit = true;
            var registrations = context.Context.Executor.Graph.OperationRegistrations[action];
            for (var i = 0; i < action.Actions.Count; i++)
            {
                var handler = action.Actions[i];
                if (!await context.Context.Executor.Inspector.OnPredicateAsync(
                    new PredicateObservationContext(context, registrations[i]), () => handler(context)))
                {
                    hit = false;
                    break;
                }
            }

            return hit;
        }
    }
}
