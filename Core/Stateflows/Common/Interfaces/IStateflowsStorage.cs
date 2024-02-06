using System.Collections.Generic;
using System.Threading.Tasks;
using Stateflows.Common.Context;
using Stateflows.Common.Trace.Models;

namespace Stateflows.Common.Interfaces
{
    public interface IStateflowsStorage
    {
        Task<StateflowsContext> HydrateAsync(BehaviorId behaviorId);
        Task DehydrateAsync(StateflowsContext context);
        Task<IEnumerable<StateflowsContext>> GetContextsAsync(IEnumerable<BehaviorClass> behaviorClasses);
        Task<IEnumerable<StateflowsContext>> GetContextsToTimeTriggerAsync(IEnumerable<BehaviorClass> behaviorClasses);
        Task SaveTraceAsync(BehaviorTrace behaviorTrace);
        Task<IEnumerable<BehaviorTrace>> GetTracesAsync(BehaviorId behaviorId);
    }
}
