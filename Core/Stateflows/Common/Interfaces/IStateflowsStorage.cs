using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Stateflows.Common.Context;

namespace Stateflows.Common.Interfaces
{
    public interface IStateflowsStorage : IDisposable
    {
        Task<StateflowsContext> HydrateAsync(BehaviorId behaviorId);
        Task DehydrateAsync(StateflowsContext context);
        Task<IEnumerable<StateflowsContext>> GetAllContextsAsync(IEnumerable<BehaviorClass> behaviorClasses);
        Task<IEnumerable<BehaviorId>> GetAllContextIdsAsync(IEnumerable<BehaviorClass> behaviorClasses);
        Task<IEnumerable<StateflowsContext>> GetTimeTriggeredContextsAsync(IEnumerable<BehaviorClass> behaviorClasses);
        Task<IEnumerable<StateflowsContext>> GetStartupTriggeredContextsAsync(IEnumerable<BehaviorClass> behaviorClasses);
    }
}
