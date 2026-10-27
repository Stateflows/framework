using System.Collections.Generic;
using System.Threading.Tasks;

namespace Stateflows.Common.Interfaces
{
    /// <summary>Defines per-behavior named stream persistence.</summary>
    public interface IStateflowsStreamStorage
    {
        /// <summary>Reads a named stream for a behavior.</summary>
        /// <typeparam name="T">The stream item type.</typeparam>
        /// <param name="behaviorId">The behavior that owns the stream.</param>
        /// <param name="streamName">The stream name.</param>
        /// <returns>An asynchronous sequence of stored stream items.</returns>
        Task<IAsyncEnumerable<T>> GetStreamAsync<T>(BehaviorId behaviorId, string streamName);
        /// <summary>Appends items to a named behavior stream.</summary>
        /// <typeparam name="T">The stream item type.</typeparam>
        /// <param name="behaviorId">The behavior that owns the stream.</param>
        /// <param name="streamName">The stream name.</param>
        /// <param name="items">The items to append.</param>
        Task AppendToStreamAsync<T>(BehaviorId behaviorId, string streamName, IEnumerable<T> items);
        /// <summary>Removes all items from a named behavior stream.</summary>
        /// <typeparam name="T">The stream item type.</typeparam>
        /// <param name="behaviorId">The behavior that owns the stream.</param>
        /// <param name="streamName">The stream name.</param>
        Task ClearStreamAsync<T>(BehaviorId behaviorId, string streamName);
    }
}