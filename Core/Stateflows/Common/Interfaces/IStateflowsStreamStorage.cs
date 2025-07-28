using System.Collections.Generic;
using System.Threading.Tasks;

namespace Stateflows.Common.Interfaces
{
    public interface IStateflowsStreamStorage
    {
        Task<IAsyncEnumerable<T>> GetStreamAsync<T>(BehaviorId behaviorId, string streamName);
        Task AppendToStreamAsync<T>(BehaviorId behaviorId, string streamName, IEnumerable<T> items);
        Task ClearStreamAsync<T>(BehaviorId behaviorId, string streamName);
    }
}