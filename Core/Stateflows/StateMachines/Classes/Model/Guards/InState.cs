using System.Linq;
using System.Threading.Tasks;
using Stateflows.Common;

namespace Stateflows.StateMachines
{
    /// <summary>Accepts a transition only while the specified typed state is present anywhere in the active configuration.</summary>
    /// <typeparam name="TState">The state type whose registered name is tested.</typeparam>
    public sealed class InState<TState> : ITransitionGuard
        where TState : class, IState
    {
        private readonly IStateMachineContext stateMachineContext;
        /// <summary>Creates the guard against a machine instance's active-state tree.</summary>
        /// <param name="stateMachineContext">The machine whose active configuration is inspected.</param>
        public InState(IStateMachineContext stateMachineContext)
        {
            this.stateMachineContext = stateMachineContext;
        }

        /// <summary>Tests whether the typed state is active, including as an enclosing or nested active state.</summary>
        /// <returns>A completed task containing whether the state is present in the active-state tree.</returns>
        public Task<bool> GuardAsync()
            => Task.FromResult(stateMachineContext.CurrentStates.GetAllNodes().Any(node => node.Value.Name == State<TState>.Name));
    }
}