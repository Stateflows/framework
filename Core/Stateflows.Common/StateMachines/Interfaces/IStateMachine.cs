using System.Threading.Tasks;
using System.Collections.Generic;
using Stateflows.Common.Interfaces;
using Stateflows.StateMachines.Events;

namespace Stateflows.StateMachines
{
    public interface IStateMachine : IBehavior
    {
        Task<StateDescriptor> GetCurrentStateAsync();

        Task<IEnumerable<string>> GetExpectedEventsAsync();
    }
}
