using System;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common.Engine;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Utilities;

namespace Stateflows.Common.Locator
{
    internal class BehaviorProxy : IBehavior, IInjectionScope
    {
        private IBehavior Behavior { get; }

        private ClientInterceptor Interceptor { get; }
        private readonly IBehaviorDispatchObserver dispatchObserver;

        public IServiceProvider ServiceProvider { get; private set; }

        public BehaviorId Id => Behavior.Id;

        public BehaviorProxy(IBehavior behavior, ClientInterceptor interceptor, IServiceProvider serviceProvider)
        {
            Behavior = behavior;
            Interceptor = interceptor;
            ServiceProvider = serviceProvider;
            dispatchObserver = serviceProvider.GetService<IBehaviorDispatchObserver>();
        }

        public async Task<SendResult> SendAsync<TEvent>(TEvent @event, IDictionary<string, EventHeader> headers = null)
        {
            var eventHolder = @event.ToTypedEventHolder(headers);
            return dispatchObserver == null
                ? await DispatchAsync()
                : await dispatchObserver.SendAsync(Id, eventHolder, DispatchAsync);

            async Task<SendResult> DispatchAsync()
            {
                SendResult result = null;
                if (Interceptor.BeforeDispatchEvent(eventHolder))
                {
                    Task<SendResult> dispatch;
                    using (BehaviorDispatchAmbient.Suppress())
                        dispatch = Behavior.SendAsync(eventHolder, eventHolder.Headers);
                    result = await dispatch;

                    Interceptor.AfterDispatchEvent(eventHolder);
                }
                else
                {
                    Trace.WriteLine($"⦗→s⦘ Client interceptor prevented Event dispatch.");
                }

                return result ?? new SendResult(EventStatus.Undelivered, new EventValidation(true));
            }
        }

        public async Task<RequestResult<TResponse>> RequestAsync<TResponse>(IRequest<TResponse> request, IDictionary<string, EventHeader> headers = null)
        {
            var eventHolder = request.ToTypedEventHolder(headers);
            return dispatchObserver == null
                ? await DispatchAsync()
                : await dispatchObserver.RequestAsync(Id, eventHolder, DispatchAsync);

            async Task<RequestResult<TResponse>> DispatchAsync()
            {
                RequestResult<TResponse> result = null;
                if (Interceptor.BeforeDispatchEvent(eventHolder))
                {
                    Task<RequestResult<TResponse>> dispatch;
                    using (BehaviorDispatchAmbient.Suppress())
                        dispatch = Behavior is IEnvelopeRequestBehavior envelopeBehavior
                            ? envelopeBehavior.RequestEnvelopeAsync<TResponse>(eventHolder)
                            : Behavior.RequestAsync(eventHolder.BoxedPayload as IRequest<TResponse>, eventHolder.Headers);
                    result = await dispatch;

                    Interceptor.AfterDispatchEvent(eventHolder);
                }
                else
                {
                    Trace.WriteLine($"⦗→s⦘ Client interceptor prevented Request dispatch.");
                }

                return result ?? new RequestResult<TResponse>(eventHolder, EventStatus.Undelivered);
            }
        }

        [Obsolete("Use WatchStatusAsync instead.")]
        public Task<IEnumerable<TNotification>> GetNotificationsAsync<TNotification>(
            DateTime? lastNotificationsCheck = null)
            => Behavior.GetNotificationsAsync<TNotification>(lastNotificationsCheck);
        
        [Obsolete("Use WatchStatusAsync instead.")]
        public Task<IEnumerable<EventHolder>> GetNotificationsAsync(string[] notificationNames,
            DateTime? lastNotificationsCheck = null)
            => Behavior.GetNotificationsAsync(notificationNames, lastNotificationsCheck);

        public Task<IWatcher> WatchAsync<TNotification>(Func<TNotification, Task> asyncHandler, DateTime? replayNotificationsSince = null)
            => Behavior.WatchAsync(asyncHandler, replayNotificationsSince);

        public Task<IWatcher> WatchAsync(string[] notificationNames, Func<EventHolder, Task> asyncHandler, DateTime? replayNotificationsSince = null)
            => Behavior.WatchAsync(notificationNames, asyncHandler, replayNotificationsSince);

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
