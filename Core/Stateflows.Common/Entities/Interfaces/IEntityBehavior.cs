using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Stateflows.Common;

namespace Stateflows.Entities
{
    public interface IEntityBehavior : IBehavior
    {
        public async Task<bool> TryMutateAsync<TMutationEvent>(TMutationEvent mutationEvent, IDictionary<string, EventHeader> headers = null)
            => (await SendAsync(mutationEvent, headers)).Status == EventStatus.Consumed;
        
        public async Task<(bool Success, TProjection Projection)> TryGetProjectionAsync<TProjection>(
            IDictionary<string, EventHeader> headers = null)
        {
            var response = await RequestAsync(new ProjectionRequest<TProjection>(), headers);
            return response.Status == EventStatus.Consumed && response.Response != null
                ? (true, response.Response)
                : (false, default);
        }
        
        public Task<IWatcher> WatchProjectionAsync<TProjection>(Action<TProjection> handler)
            => WatchAsync(handler);

        public Task<IWatcher> WatchProjectionAsync<TProjection>(Func<TProjection, Task> asyncHandler)
            => WatchProjectionAsync<TProjection>(handler: n => asyncHandler(n).GetAwaiter().GetResult());

        public async Task<bool> TrySetAsync<T>(string fieldName, T fieldValue, IDictionary<string, EventHeader> headers = null)
        {
            var result = await SendAsync(new FieldState<T> { Name = fieldName, Value = fieldValue }, headers);
            return result.Status == EventStatus.Consumed;
        }

        public async Task<(bool Success, T FieldValue)> TryGetAsync<T>(string fieldName, IDictionary<string, EventHeader> headers = null)
        {
            var response = await RequestAsync(new FieldStateRequest<T> { Name = fieldName }, headers);
            return response.Status == EventStatus.Consumed && response.Response != null
                ? (true, response.Response.Value)
                : (false, default);
        }
    }
}
