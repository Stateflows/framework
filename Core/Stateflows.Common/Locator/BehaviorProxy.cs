using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Stateflows.Common.Engine;

namespace Stateflows.Common.Locator
{
    internal class BehaviorProxy : IBehavior
    {
        private IBehavior Behavior { get; }

        private ClientInterceptor Interceptor { get; }

        public BehaviorId Id => Behavior.Id;

        public BehaviorProxy(IBehavior behavior, ClientInterceptor interceptor)
        {
            Behavior = behavior;
            Interceptor = interceptor;
        }

        public async Task<SendResult> SendAsync<TEvent>(TEvent @event, IEnumerable<EventHeader> headers = null)
        {
            var headersList = headers?.ToList() ?? new List<EventHeader>();

            await Interceptor.BeforeDispatchEventAsync(@event, headersList);

            var result = await Behavior.SendAsync(@event, headersList);

            await Interceptor.AfterDispatchEventAsync(@event);

            return result;
        }

        public async Task<RequestResult<TResponse>> RequestAsync<TResponse>(Request<TResponse> request, IEnumerable<EventHeader> headers = null)
        {
            var headersList = headers?.ToList() ?? new List<EventHeader>();

            await Interceptor.BeforeDispatchEventAsync(@request, headersList);

            var result = await Behavior.RequestAsync(@request, headersList);

            await Interceptor.AfterDispatchEventAsync(@request);

            return result;
        }

        public Task WatchAsync<TNotification>(Action<TNotification> handler)
            => Behavior.WatchAsync<TNotification>(handler);

        public Task UnwatchAsync<TNotification>()
            => Behavior.UnwatchAsync<TNotification>();

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
            => Behavior.Dispose();

        ~BehaviorProxy()
            => Dispose(false);
    }
}
