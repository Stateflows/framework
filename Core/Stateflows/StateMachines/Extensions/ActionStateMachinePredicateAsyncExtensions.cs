using System.Linq;
using System.Threading.Tasks;
using Stateflows.Common.Models;
using Stateflows.StateMachines.Interfaces;
using Stateflows.StateMachines.Context.Classes;

namespace Stateflows.StateMachines
{
    internal static class ActionStateMachinePredicateAsyncExtensions
    {
        public static async Task<bool> WhenAll(this Logic<StateMachinePredicateAsync> action, RootContext context)
            => !(await Task.WhenAll(action.Actions.Select(a => a(context)))).Any(result => !result);
    }
}
