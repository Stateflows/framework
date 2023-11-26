using System.Collections.Generic;
using System.Threading.Tasks;
using Stateflows.Common.Classes;
using Stateflows.Common.Context;

namespace Stateflows.Common.Interfaces
{
    public interface IStateflowsStorage
    {
        Task<StateflowsContext> Hydrate(BehaviorId id);
        Task Dehydrate(StateflowsContext context);
        Task<IEnumerable<StateflowsContext>> GetContexts(IEnumerable<BehaviorClass> behaviorClasses);
        Task AddTimeTokens(TimeToken[] timeTokens);
        Task<IEnumerable<TimeToken>> GetTimeTokens(IEnumerable<BehaviorClass> behaviorClasses);
        Task ClearTimeTokens(BehaviorId behaviorId, IEnumerable<string> ids);
    }
}
