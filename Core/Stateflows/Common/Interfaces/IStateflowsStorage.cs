using System.Collections.Generic;
using System.Threading.Tasks;
using Stateflows.Common.Context;

namespace Stateflows.Common.Interfaces
{
    public interface IStateflowsStorage
    {
        Task<StateflowsContext> Hydrate(BehaviorId id);
        Task Dehydrate(StateflowsContext context);
        Task<IEnumerable<StateflowsContext>> GetContexts(IEnumerable<BehaviorClass> behaviorClasses);
        Task<IEnumerable<StateflowsContext>> GetContextsToTimeTrigger(IEnumerable<BehaviorClass> behaviorClasses);
    }
}
