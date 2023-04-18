using System.Threading.Tasks;
using Stateflows.StateMachines.Interfaces;

namespace Stateflows.StateMachines
{
    public static class StateMachineAction
    {
        public static StateMachineActionDelegateAsync ToAsync(this StateMachineActionDelegate stateMachineAction)
            => c => Task.Run(() => stateMachineAction(c));
    }
}
