using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Stateflows.Common;

namespace Stateflows.Entities
{
    /// <summary>Provides field, mutation, projection, event, and notification operations for an entity instance.</summary>
    public interface IEntityBehavior : IBehavior
    {
        /// <summary>Sends a mutation event and reports whether an entity mutation consumed it.</summary>
        /// <typeparam name="TMutationEvent">The mutation event type.</typeparam>
        /// <param name="mutationEvent">The mutation event.</param>
        /// <param name="headers">Optional delivery headers.</param>
        /// <returns><see langword="true"/> when the event was consumed; otherwise, <see langword="false"/>.</returns>
        public async Task<bool> TryMutateAsync<TMutationEvent>(TMutationEvent mutationEvent, IDictionary<string, EventHeader> headers = null)
            => (await SendAsync(mutationEvent, headers)).Status == EventStatus.Consumed;
        
        /// <summary>Requests the current value of a projection.</summary>
        /// <typeparam name="TProjection">The projection type.</typeparam>
        /// <param name="headers">Optional delivery headers.</param>
        /// <returns>A success flag and the projection when the request is consumed.</returns>
        public async Task<(bool Success, TProjection Projection)> TryGetProjectionAsync<TProjection>(
            IDictionary<string, EventHeader> headers = null)
        {
            var response = await RequestAsync(new ProjectionRequest<TProjection>(), headers);
            return response.Status == EventStatus.Consumed && response.Response != null
                ? (true, response.Response)
                : (false, default);
        }
        
        /// <summary>Watches for changes to a projection.</summary>
        /// <typeparam name="TProjection">The projection type.</typeparam>
        /// <param name="handler">Callback invoked with each published projection value.</param>
        /// <returns>A watcher that stops the subscription when disposed.</returns>
        public Task<IWatcher> WatchProjectionAsync<TProjection>(Action<TProjection> handler)
            => WatchAsync(handler);

        /// <summary>Asynchronously watches for changes to a projection.</summary>
        /// <typeparam name="TProjection">The projection type.</typeparam>
        /// <param name="asyncHandler">Asynchronous callback invoked with each published projection value.</param>
        /// <returns>A watcher that stops the subscription when disposed.</returns>
        public Task<IWatcher> WatchProjectionAsync<TProjection>(Func<TProjection, Task> asyncHandler)
            => WatchAsync(asyncHandler);

        /// <summary>Sets a registered entity field.</summary>
        /// <typeparam name="T">The field value type.</typeparam>
        /// <param name="fieldName">The registered field name.</param>
        /// <param name="fieldValue">The value to assign.</param>
        /// <param name="headers">Optional delivery headers.</param>
        /// <returns><see langword="true"/> when the field update is accepted; otherwise, <see langword="false"/>.</returns>
        public async Task<bool> TrySetAsync<T>(string fieldName, T fieldValue, IDictionary<string, EventHeader> headers = null)
        {
            var result = await SendAsync(new FieldState<T> { Name = fieldName, Value = fieldValue }, headers);
            return result.Status == EventStatus.Consumed;
        }

        /// <summary>Requests the current value of a registered entity field.</summary>
        /// <typeparam name="T">The expected field value type.</typeparam>
        /// <param name="fieldName">The registered field name.</param>
        /// <param name="headers">Optional delivery headers.</param>
        /// <returns>A success flag and the field value when the read is accepted.</returns>
        public async Task<(bool Success, T FieldValue)> TryGetAsync<T>(string fieldName, IDictionary<string, EventHeader> headers = null)
        {
            var response = await RequestAsync(new FieldStateRequest<T> { Name = fieldName }, headers);
            return response.Status == EventStatus.Consumed && response.Response != null
                ? (true, response.Response.Value)
                : (false, default);
        }
    }
}
