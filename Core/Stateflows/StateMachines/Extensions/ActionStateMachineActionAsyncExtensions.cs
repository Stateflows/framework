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
            foreach (var task in action.Actions.Select(a => a(context)))
            {
                await task;
            }
        }
    }
}
