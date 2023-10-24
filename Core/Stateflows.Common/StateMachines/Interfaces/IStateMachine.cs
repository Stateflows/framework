using System.Threading.Tasks;
using Stateflows.Common.Interfaces;
using Stateflows.StateMachines.Events;

namespace Stateflows.StateMachines
{
    public interface IStateMachine : IBehavior
    {
        Task<CurrentStateResponse> GetCurrentStateAsync();
    }
}
