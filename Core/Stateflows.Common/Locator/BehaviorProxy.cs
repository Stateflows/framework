using System;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using Stateflows.Common.Engine;
using Stateflows.Common.Interfaces;

namespace Stateflows.Common.Locator
{
    internal class BehaviorProxy : IBehavior, IInjectionScope
    {
        private IBehavior Behavior { get; }

        private ClientInterceptor Interceptor { get; }

        public IServiceProvider ServiceProvider { get; private set; }

        public BehaviorId Id => Behavior.Id;

        public BehaviorProxy(IBehavior behavior, ClientInterceptor interceptor, IServiceProvider serviceProvider)
        {
            Behavior = behavior;
            Interceptor = interceptor;
            ServiceProvider = serviceProvider;
        }

        public async Task<SendResult> SendAsync<TEvent>(TEvent @event, IEnumerable<EventHeader> headers = null)
        {
            SendResult result = null;
            var eventHolder = new EventHolder<TEvent>()
            {
                Payload = @event,
                Headers = headers?.ToList() ?? new List<EventHeader>()
            };

            if (Interceptor.BeforeDispatchEvent(eventHolder))
            {
                result = await Behavior.SendAsync(eventHolder.Payload, eventHolder.Headers);

                Interceptor.AfterDispatchEvent(eventHolder);
            }
            else
            {
                Trace.WriteLine($"⦗→s⦘ Client interceptor prevented Event dispatch.");
            }

            result ??= new SendResult(EventStatus.Undelivered, new EventValidation(true));

            return result;
        }

        public async Task<RequestResult<TResponse>> RequestAsync<TResponse>(IRequest<TResponse> request, IEnumerable<EventHeader> headers = null)
        {
            RequestResult<TResponse> result = null;
            var eventHolder = request.ToTypedEventHolder(headers);

            if (Interceptor.BeforeDispatchEvent(eventHolder))
            {
                result = await Behavior.RequestAsync(eventHolder.BoxedPayload as IRequest<TResponse>, eventHolder.Headers);

                Interceptor.AfterDispatchEvent(eventHolder);
            }
            else
            {
                Trace.WriteLine($"⦗→s⦘ Client interceptor prevented Request dispatch.");
            }

            result ??= new RequestResult<TResponse>(eventHolder, EventStatus.Undelivered);

            return result;
        }

        public Task<IEnumerable<TNotification>> GetNotificationsAsync<TNotification>(
            DateTime? lastNotificationsCheck = null)
            => Behavior.GetNotificationsAsync<TNotification>(lastNotificationsCheck);

        public Task<IEnumerable<EventHolder>> GetNotificationsAsync(string[] notificationNames,
            DateTime? lastNotificationsCheck = null)
            => Behavior.GetNotificationsAsync(notificationNames, lastNotificationsCheck);

        public Task<IWatcher> WatchAsync<TNotification>(Action<TNotification> handler)
            => Behavior.WatchAsync(handler);

        public Task<IWatcher> WatchAsync(string[] notificationNames, Action<EventHolder> handler)
            => Behavior.WatchAsync(notificationNames, handler);

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
