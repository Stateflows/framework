using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<BehaviorProxy> logger;

        public IServiceProvider ServiceProvider { get; private set; }

        public BehaviorId Id => Behavior.Id;

        public BehaviorProxy(IBehavior behavior, ClientInterceptor interceptor, IServiceProvider serviceProvider)
        {
            Behavior = behavior;
            Interceptor = interceptor;
            ServiceProvider = serviceProvider;
            dispatchObserver = serviceProvider.GetService<IBehaviorDispatchObserver>();
            logger = serviceProvider.GetRequiredService<ILogger<BehaviorProxy>>();
        }

        public async Task<SendResult> SendAsync<TEvent>(TEvent @event, IDictionary<string, EventHeader> headers = null)
        {
            var eventHolder = @event.ToTypedEventHolder(headers);
            return dispatchObserver == null
                ? await DispatchAsync()
                : await dispatchObserver.SendAsync(Id, eventHolder, DispatchAsync);

            async Task<SendResult> DispatchAsync()
            {
                Task<SendResult>? dispatch = null;
                IDisposable? dispatchScope = null;
                await Interceptor.DispatchEventAsync(Id, eventHolder, () =>
                {
                    dispatchScope = BehaviorDispatchAmbient.Suppress();
                    try
                    {
                        dispatch = Behavior.SendAsync(eventHolder, eventHolder.Headers);
                        _ = ObserveDispatchCompletionAsync(dispatch, dispatchScope);
                    }
                    catch
                    {
                        dispatchScope.Dispose();
                        throw;
                    }
                    return Task.CompletedTask;
                });

                return dispatch == null
                    ? new SendResult(EventStatus.Undelivered, new EventValidation(true))
                    : await dispatch;
            }
        }

        public async Task<SendResult> DispatchAsync<TEvent>(TEvent @event, IDictionary<string, EventHeader> headers = null)
        {
            var eventHolder = @event.ToTypedEventHolder(headers);
            return dispatchObserver == null
                ? await DispatchAsync()
                : await dispatchObserver.SendAsync(Id, eventHolder, DispatchAsync);

            async Task<SendResult> DispatchAsync()
            {
                SendResult? result = null;
                await Interceptor.DispatchEventAsync(Id, eventHolder, async () =>
                {
                    using (BehaviorDispatchAmbient.Suppress())
                    {
                        result = await Behavior.DispatchAsync(eventHolder, eventHolder.Headers);
                    }
                });

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
                Task<RequestResult<TResponse>>? dispatch = null;
                IDisposable? dispatchScope = null;
                await Interceptor.DispatchEventAsync(Id, eventHolder, () =>
                {
                    dispatchScope = BehaviorDispatchAmbient.Suppress();
                    try
                    {
                        var requestDispatch = Behavior is IEnvelopeRequestBehavior envelopeBehavior
                            ? envelopeBehavior.RequestEnvelopeAsync<TResponse>(eventHolder)
                            : Behavior.RequestAsync(eventHolder.BoxedPayload as IRequest<TResponse>, eventHolder.Headers);
                        dispatch = requestDispatch;
                        _ = ObserveDispatchCompletionAsync(dispatch, dispatchScope);
                    }
                    catch
                    {
                        dispatchScope.Dispose();
                        throw;
                    }
                    return Task.CompletedTask;
                });

                return dispatch == null
                    ? new RequestResult<TResponse>(eventHolder, EventStatus.Undelivered)
                    : await dispatch;
            }
        }

        private async Task ObserveDispatchCompletionAsync(Task dispatch, IDisposable dispatchScope)
        {
            try
            {
                await dispatch;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Dispatch processing failed for {BehaviorId}.", Id);
            }
            finally
            {
                dispatchScope.Dispose();
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
