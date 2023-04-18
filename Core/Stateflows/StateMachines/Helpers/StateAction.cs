using System.Threading.Tasks;
using Stateflows.StateMachines.Interfaces;

namespace Stateflows.StateMachines
{
    public static class StateAction
    {
        public static StateActionDelegateAsync ToAsync(this StateActionDelegate stateAction)
            => c => Task.Run(() => stateAction(c));
    }
}
