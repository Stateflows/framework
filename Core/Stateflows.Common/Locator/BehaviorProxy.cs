using System;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
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
            SendResult result = null;
            var headersList = headers?.ToList() ?? new List<EventHeader>();

            if (await Interceptor.BeforeDispatchEventAsync(@event, headersList))
            {
                result = await Behavior.SendAsync(@event, headersList.ToArray());

                await Interceptor.AfterDispatchEventAsync(@event);
            }
            else
            {
                Trace.WriteLine($"⦗→s⦘ Client interceptor prevented Event dispatch.");
            }

            result ??= new SendResult(@event, EventStatus.Undelivered);

            return result;
        }

        public async Task<RequestResult<TResponse>> RequestAsync<TResponse>(IRequest<TResponse> request, IEnumerable<EventHeader> headers = null)
        {
            RequestResult<TResponse> result = null;
            var headersList = headers?.ToList() ?? new List<EventHeader>();

            if (await Interceptor.BeforeDispatchEventAsync(@request, headersList))
            {
                result = await Behavior.RequestAsync(@request, headersList.ToArray());

                await Interceptor.AfterDispatchEventAsync(@request);
            }
            else
            {
                Trace.WriteLine($"⦗→s⦘ Client interceptor prevented Request dispatch.");
            }

            result ??= new RequestResult<TResponse>(request, EventStatus.Undelivered);

            return result;
        }

        public Task<IWatcher> WatchAsync<TNotificationEvent>(Action<TNotificationEvent> handler)
            => Behavior.WatchAsync(handler);

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
