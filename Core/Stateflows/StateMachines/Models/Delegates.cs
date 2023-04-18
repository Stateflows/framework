using System.Threading.Tasks;
using Stateflows.StateMachines.Context.Classes;

namespace Stateflows.StateMachines.Interfaces
{
    internal delegate Task StateMachineActionAsync(RootContext context);

    internal delegate Task<bool> StateMachinePredicateAsync(RootContext context);
}