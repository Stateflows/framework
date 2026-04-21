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
            foreach (var handler in action.Actions)
            {
                if (!await handler(context))
                {
                    hit = false;
                    break;
                }
            }

            return hit;
        }
    }
}
