using System.Linq;
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
            foreach (var task in action.Actions.Select(a => a(context)))
            {
                await task;
            }
        }

        public static async Task IterateOverAsync(this Logic<ActivityEventActionAsync> action, BaseContext context)
        {
            foreach (var task in action.Actions.Select(a => a(context)))
            {
                await task;
            }
        }
    }
}
