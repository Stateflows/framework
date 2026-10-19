using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading;
using Volatile = System.Threading.Volatile;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Stateflows.Common;
using Stateflows.Common.Context.Classes;
using Stateflows.Entities.Context.Classes;
using Stateflows.Entities.Enums;
using Stateflows.Entities.Registration;

namespace Stateflows.Entities.Engine
{
    /// <summary>
    /// Per-execution pipeline of entity interceptors and observers.
    /// Global elements are outermost, followed by entity elements; within each group,
    /// the first registered element is outermost.
    /// </summary>
    internal sealed class EntityInspection
    {
        private readonly EntityActionContext actionContext;

        private readonly IEntityObserver[] observers;

        private readonly IEntityInterceptor[] interceptors;

        private readonly IEntityExceptionHandler[] exceptionHandlers;

        private EntityInspection(EntityActionContext actionContext, IEntityObserver[] observers, IEntityInterceptor[] interceptors, IEntityExceptionHandler[] exceptionHandlers)
        {
            this.actionContext = actionContext;
            this.observers = observers;
            this.interceptors = interceptors;
            this.exceptionHandlers = exceptionHandlers;
        }

        public bool HasObservers => observers.Length != 0;

        public bool HasOperationContexts => HasObservers || exceptionHandlers.Length != 0;

        public bool HasInterceptors => interceptors.Length != 0;

        /// <summary>
        /// Resolves all registered observer and interceptor factories once for a single execution:
        /// global factories first, then entity factories, each group in registration order.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when a factory returns <c>null</c>.</exception>
        public static async Task<EntityInspection> CreateAsync(EntitiesRegister register, EntityRegistration registration, BehaviorContext behaviorContext)
        {
            var serviceProvider = behaviorContext.ServiceProvider;
            var context = new EntityActionContext(behaviorContext);

            var interceptors = new List<IEntityInterceptor>();
            await ResolveAsync(register.GlobalInterceptorFactories, "global", "interceptor", interceptors,
                factory => factory(serviceProvider, context), behaviorContext);
            await ResolveAsync(registration.InterceptorFactories, "entity", "interceptor", interceptors,
                factory => factory(serviceProvider, context), behaviorContext);

            var observers = new List<IEntityObserver>();
            await ResolveAsync(register.GlobalObserverFactories, "global", "observer", observers,
                factory => factory(serviceProvider, context), behaviorContext);
            await ResolveAsync(registration.ObserverFactories, "entity", "observer", observers,
                factory => factory(serviceProvider, context), behaviorContext);

            var exceptionHandlers = new List<IEntityExceptionHandler>();
            await ResolveAsync(register.GlobalExceptionHandlerFactories, "global", "exception handler", exceptionHandlers,
                factory => factory(serviceProvider, context), behaviorContext);
            await ResolveAsync(registration.ExceptionHandlerFactories, "entity", "exception handler", exceptionHandlers,
                factory => factory(serviceProvider, context), behaviorContext);

            return new EntityInspection(context, observers.ToArray(), interceptors.ToArray(), exceptionHandlers.ToArray());
        }

        private static async Task ResolveAsync<TFactory, TElement>(
            IReadOnlyList<TFactory> factories,
            string scope,
            string kind,
            List<TElement> elements,
            Func<TFactory, Task<TElement>> invoke,
            BehaviorContext behaviorContext)
            where TElement : class
        {
            for (var i = 0; i < factories.Count; i++)
            {
                var element = await invoke(factories[i])
                    ?? throw new InvalidOperationException(
                        $"Entity {kind} factory #{i} ({scope}) returned null for behavior '{behaviorContext.Context.Id}'."
                    );
                elements.Add(element);
            }
        }

        /// <summary>
        /// Runs event processing through registered interceptors; the first registered is outermost.
        /// </summary>
        public Task<EventStatus> ProcessEventAsync<TEvent>(EventHolder<TEvent> eventHolder, Func<Task<EventStatus>> next)
        {
            if (interceptors.Length == 0)
            {
                return next();
            }

            var context = new EntityEventContext<TEvent>(actionContext, eventHolder);
            var pipeline = next;
            for (var i = interceptors.Length - 1; i >= 0; i--)
            {
                var interceptor = interceptors[i];
                var inner = pipeline;
                pipeline = () => InterceptAsync(interceptor, context, inner);
            }

            return pipeline();
        }

        /// <summary>Runs a mutation through registered observers (exactly-once continuation enforced).</summary>
        public Task MutationAsync<TTemplate, TMutationEvent>(TTemplate entity, TMutationEvent mutation, Func<Task> next)
        {
            if (!HasOperationContexts)
            {
                return next();
            }

            var context = new MutationObservationContext<TTemplate, TMutationEvent>(actionContext, entity, mutation);

            return Observe<bool>(
                async () =>
                {
                    await ExecuteAsync(
                        async () => { await next(); return true; },
                        (handler, exception) => handler.OnMutationExceptionAsync(context, exception),
                        nameof(IEntityExceptionHandler.OnMutationExceptionAsync));
                    return true;
                },
                (observer, inner) => observer.OnMutationAsync(context, () => inner())
            );
        }

        /// <summary>Runs a computed field evaluation through registered observers; the evaluated value is returned unchanged.</summary>
        public Task<TField> ComputedFieldAsync<TTemplate, TField>(TTemplate entity, string fieldName, Func<Task<TField>> next)
        {
            if (!HasOperationContexts)
            {
                return next();
            }

            var context = new ComputedFieldObservationContext<TTemplate>(actionContext, entity, fieldName);

            return Observe(
                () => ExecuteAsync(next,
                    (handler, exception) => handler.OnComputedFieldExceptionAsync<TTemplate, TField>(context, exception),
                    nameof(IEntityExceptionHandler.OnComputedFieldExceptionAsync)),
                (observer, inner) => observer.OnComputedFieldAsync(context, inner));
        }

        /// <summary>Runs a projection evaluation through registered observers; the evaluated value is returned unchanged.</summary>
        public Task<TProjection> ProjectionAsync<TTemplate, TProjection>(TTemplate entity, PublishScope publishScope, Func<Task<TProjection>> next)
        {
            if (!HasOperationContexts)
            {
                return next();
            }

            var context = new ProjectionObservationContext<TTemplate>(actionContext, entity, publishScope);

            return Observe(
                () => ExecuteAsync(next,
                    (handler, exception) => handler.OnProjectionExceptionAsync<TTemplate, TProjection>(context, exception),
                    nameof(IEntityExceptionHandler.OnProjectionExceptionAsync)),
                (observer, inner) => observer.OnProjectionAsync(context, inner));
        }

        public async Task InitializeAsync(bool implicitInitialization, Func<Task> next)
        {
            var context = new EntityInitializationExceptionContext(actionContext, implicitInitialization);
            await ExecuteAsync(
                async () => { await next(); return true; },
                (handler, exception) => handler.OnEntityInitializationExceptionAsync(context, exception),
                nameof(IEntityExceptionHandler.OnEntityInitializationExceptionAsync));
        }

        private async Task<TResult> ExecuteAsync<TResult>(
            Func<Task<TResult>> next,
            Func<IEntityExceptionHandler, Exception, Task<bool>> handle,
            string methodName)
        {
            if (exceptionHandlers.Length == 0)
            {
                return await next();
            }

            try
            {
                return await next();
            }
            catch (Exception exception)
            {
                var handled = false;
                var logger = actionContext.BehaviorContext.ServiceProvider.GetRequiredService<ILogger<EntityInspection>>();
                foreach (var handler in exceptionHandlers)
                {
                    try
                    {
                        handled |= await handle(handler, exception);
                    }
                    catch (Exception handlerException)
                    {
                        actionContext.BehaviorContext.Context.ExecutionExceptions.Enqueue(handlerException);
                        logger.LogError(handlerException, "Entity exception handler {HandlerType}.{Method} failed for {BehaviorId}.",
                            handler.GetType().FullName, methodName, actionContext.Behavior.Id);
                    }
                }

                logger.LogDebug("Entity exception in {Method} for {BehaviorId}; handled: {Handled}. Execution will be rolled back.",
                    methodName, actionContext.Behavior.Id, handled);
                throw;
            }
        }

        /// <remarks>
        /// Interceptors may call the continuation at most once and must await it before returning.
        /// Skipping it (short-circuit) is allowed; exceptions handled by the interceptor stay handled.
        /// </remarks>
        private static async Task<EventStatus> InterceptAsync<TEvent>(
            IEntityInterceptor interceptor,
            EntityEventContext<TEvent> context,
            Func<Task<EventStatus>> next)
        {
            var continuation = new Continuation<EventStatus>(next, interceptor, "Interceptor");

            EventStatus status;
            try
            {
                status = await interceptor.ProcessEventAsync(context, continuation.InvokeAsync);
            }
            catch
            {
                await continuation.DrainAsync();
                throw;
            }

            await continuation.VerifyAsync(required: false);

            return status;
        }

        /// <remarks>
        /// Observers must call and await the continuation exactly once. Violations raise
        /// <see cref="InvalidOperationException"/>; a continuation failure swallowed by an observer
        /// is rethrown with its original stack trace, and its result is never replaced.
        /// </remarks>
        private Task<TResult> Observe<TResult>(Func<Task<TResult>> next, Func<IEntityObserver, Func<Task<TResult>>, Task> observe)
        {
            var pipeline = next;
            for (var i = observers.Length - 1; i >= 0; i--)
            {
                var observer = observers[i];
                var inner = pipeline;
                pipeline = async () =>
                {
                    var continuation = new Continuation<TResult>(inner, observer, "Observer");

                    try
                    {
                        await observe(observer, continuation.InvokeAsync);
                    }
                    catch
                    {
                        await continuation.DrainAsync();
                        throw;
                    }

                    await continuation.VerifyAsync(required: true);
                    continuation.RethrowSwallowedFailure();

                    return continuation.Result;
                };
            }

            return pipeline();
        }

        private sealed class Continuation<TResult>(Func<Task<TResult>> next, object owner, string ownerKind)
        {
            private int invocationCount;

            private Task<TResult>? inFlight;

            private bool invokedMoreThanOnce => Volatile.Read(ref invocationCount) > 1;

            private ExceptionDispatchInfo? failure;

            public TResult Result { get; private set; } = default!;

            private string OwnerDescription => $"{ownerKind} '{owner.GetType().FullName}'";

            public Task<TResult> InvokeAsync()
            {
                // Counted before execution so re-entrant or concurrent calls are rejected deterministically.
                if (Interlocked.Increment(ref invocationCount) != 1)
                {
                    throw new InvalidOperationException($"{OwnerDescription} invoked its continuation more than once.");
                }

                var task = RunAsync();
                Volatile.Write(ref inFlight, task);

                return task;
            }

            private async Task<TResult> RunAsync()
            {
                try
                {
                    return Result = await next();
                }
                catch (Exception exception)
                {
                    failure = ExceptionDispatchInfo.Capture(exception);
                    throw;
                }
            }

            public async Task DrainAsync()
            {
                var task = Volatile.Read(ref inFlight);
                if (task != null && !task.IsCompleted)
                {
                    try
                    {
                        await task;
                    }
                    catch
                    {
                        // Failure is either already propagating through the hook or captured in 'failure'.
                    }
                }
            }

            public async Task VerifyAsync(bool required)
            {
                if (invokedMoreThanOnce)
                {
                    await DrainAsync();
                    throw new InvalidOperationException($"{OwnerDescription} invoked its continuation more than once.");
                }

                if (Volatile.Read(ref invocationCount) == 0)
                {
                    if (required)
                    {
                        throw new InvalidOperationException($"{OwnerDescription} completed without invoking its continuation.");
                    }

                    return;
                }

                var task = Volatile.Read(ref inFlight);
                if (task == null || !task.IsCompleted)
                {
                    Exception? innerException = null;
                    try
                    {
                        if (task != null)
                        {
                            await task;
                        }
                    }
                    catch (Exception exception)
                    {
                        innerException = exception;
                    }

                    throw new InvalidOperationException(
                        $"{OwnerDescription} completed before its continuation finished; the continuation must be awaited.",
                        innerException
                    );
                }
            }

            public void RethrowSwallowedFailure()
                => failure?.Throw();
        }
    }
}