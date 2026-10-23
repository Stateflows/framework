using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Stateflows.Common;
using Stateflows.Common.Context.Classes;
using Stateflows.Common.Engine;
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
            return InterceptorPipeline.RunAsync(interceptors, next,
                (interceptor, inner) => interceptor.ProcessEventAsync(context, inner));
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
            try
            {
                return await next();
            }
            catch (Exception exception)
            {
                var logger = actionContext.BehaviorContext.ServiceProvider.GetRequiredService<ILogger<EntityInspection>>();
                var handled = await ExecutionExceptionHandling.HandleAsync(
                    actionContext.BehaviorContext.Context, exceptionHandlers,
                    handler => handle(handler, exception), methodName, logger);
                actionContext.BehaviorContext.Context.ExecutionExceptions.Enqueue(exception);

                logger.LogDebug("Entity exception in {Method} for {BehaviorId}; handled: {Handled}. Execution will be rolled back.",
                    methodName, actionContext.Behavior.Id, handled);
                throw;
            }
        }

        /// <remarks>
        /// Observers must call and await the continuation exactly once. Violations raise
        /// <see cref="InvalidOperationException"/>; a continuation failure swallowed by an observer
        /// is rethrown with its original stack trace, and its result is never replaced.
        /// </remarks>
        private Task<TResult> Observe<TResult>(Func<Task<TResult>> next, Func<IEntityObserver, Func<Task<TResult>>, Task> observe)
            => ObserverPipeline.RunAsync(observers, next, observe);
    }
}