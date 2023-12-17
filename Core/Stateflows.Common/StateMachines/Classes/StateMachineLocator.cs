using Stateflows.Common.Interfaces;
using Stateflows.StateMachines;
using System.Threading.Tasks;

namespace Stateflows.Common.StateMachines.Classes
{
    internal class StateMachineLocator : IStateMachineLocator
    {
        private IBehaviorLocator Locator { get; }

        public StateMachineLocator(IBehaviorLocator locator)
        {
            Locator = locator;
        }

        public bool TryLocateStateMachine(StateMachineId id, out IStateMachine stateMachine)
            => Locator.TryLocateStateMachine(id, out stateMachine);
    }
}
