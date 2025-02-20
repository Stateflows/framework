using System.Linq;
using System.Threading.Tasks;
using Stateflows.Common;

namespace Stateflows.StateMachines
{
    public sealed class InState<TState> : ITransitionGuard
        where TState : class, IState
    {
        private readonly IStateMachineContext stateMachineContext;
        public InState(IStateMachineContext stateMachineContext)
        {
            this.stateMachineContext = stateMachineContext;
        }

        public Task<bool> GuardAsync()
            => Task.FromResult(stateMachineContext.CurrentState.GetAllNodes().Any(node => node.Value.Name == State<TState>.Name));
    }
}