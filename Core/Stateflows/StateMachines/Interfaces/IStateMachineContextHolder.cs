using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Stateflows.Common;

namespace Stateflows.StateMachines
{
    public interface IStateMachineContextHolder : IAsyncDisposable
    {
        StateMachineId StateMachineId { get; }
        BehaviorStatus BehaviorStatus { get; }
        // IEnumerable<string> ExpectedEventNames { get; }
        Task<IEnumerable<string>> GetExpectedEventNamesAsync();
        IReadOnlyTree<string> CurrentStates { get; }
        IStateMachineContext GetStateMachineContext();
        IStateContext GetStateContext(string stateName);
    }
}