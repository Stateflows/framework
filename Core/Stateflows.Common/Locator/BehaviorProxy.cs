using System;
using System.Diagnostics;
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

        public async Task<SendResult> SendAsync<TEvent>(TEvent @event)
            where TEvent : Event, new()
        {
            SendResult result = null;

            if (await Interceptor.BeforeDispatchEventAsync(@event))
            {
                result = await Behavior.SendAsync(@event);

                await Interceptor.AfterDispatchEventAsync(@event);
            }
            else
            {
                Trace.WriteLine($"⦗→s⦘ Client interceptor prevented Event dispatch.");
            }

            result ??= new SendResult(@event, EventStatus.Undelivered);

            return result;
        }

        public async Task<RequestResult<TResponse>> RequestAsync<TResponse>(Request<TResponse> request)
            where TResponse : Response, new()
        {
            RequestResult<TResponse> result = null;

            if (await Interceptor.BeforeDispatchEventAsync(@request))
            {
                result = await Behavior.RequestAsync(@request);

                await Interceptor.AfterDispatchEventAsync(@request);
            }
            else
            {
                Trace.WriteLine($"⦗→s⦘ Client interceptor prevented Request dispatch.");
            }

            result ??= new RequestResult<TResponse>(request, EventStatus.Undelivered);

            return result;
        }

        public Task WatchAsync<TNotification>(Action<TNotification> handler)
            where TNotification : Notification, new()
            => Behavior.WatchAsync<TNotification>(handler);

        public Task UnwatchAsync<TNotification>()
            where TNotification : Notification, new()
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
