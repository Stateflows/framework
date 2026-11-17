using System.Threading.Tasks;
using Stateflows.Common.Models;
using Stateflows.Activities.Models;
using Stateflows.Activities.Context.Classes;

namespace Stateflows.Activities
{
    internal static class ActivityActionAsyncExtensions
    {
        public static async Task IterateOverAsync(this Logic<ActivityActionAsync> action, Context.Interfaces.IActionContext context)
        {
            var baseContext = (BaseContext)context;
            var registrations = baseContext.Context.Executor.Graph.OperationRegistrations[action];
            for (var i = 0; i < action.Actions.Count; i++)
            {
                var handler = action.Actions[i];
                if (registrations[i].ObservedInBody)
                    await handler(context);
                else
                    await baseContext.Context.Executor.Inspector.OnActionAsync(
                        new ActionObservationContext(baseContext, registrations[i]), () => handler(context));
            }
        }

        public static async Task IterateOverAsync(this Logic<ActivityEventActionAsync> action, BaseContext context)
        {
            var registrations = context.Context.Executor.Graph.OperationRegistrations[action];
            for (var i = 0; i < action.Actions.Count; i++)
            {
                var handler = action.Actions[i];
                await context.Context.Executor.Inspector.OnActionAsync(
                    new ActionObservationContext(context, registrations[i]), () => handler(context));
            }
        }
    }
}
