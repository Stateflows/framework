using System.Linq;
using System.Threading.Tasks;
using Stateflows.Common.Models;
using Stateflows.Activities.Models;
using Stateflows.Activities.Context.Classes;

namespace Stateflows.Activities
{
    internal static class ActivityPredicateAsyncExtensions
    {
        public static async Task<bool> WhenAll(this Logic<ActivityPredicateAsync> action, BaseContext context)
            => !(await Task.WhenAll(action.Actions.Select(a => a(context)))).Any(result => !result);
    }
}
