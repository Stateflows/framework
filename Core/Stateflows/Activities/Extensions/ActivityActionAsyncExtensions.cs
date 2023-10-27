using System.Linq;
using System.Threading.Tasks;
using Stateflows.Common.Models;
using Stateflows.Activities.Models;
using Stateflows.Activities.Context.Interfaces;
using Stateflows.Activities.Context.Classes;

namespace Stateflows.Activities
{
    internal static class ActivityActionAsyncExtensions
    {
        public static Task WhenAll(this Logic<ActivityActionAsync> action, IActionContext context)
            => Task.WhenAll(action.Actions.Select(a => a(context)));

        public static Task WhenAll(this Logic<ActivityEventActionAsync> action, BaseContext context)
            => Task.WhenAll(action.Actions.Select(a => a(context)));
    }
}
