using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Actions;
using Stateflows.Activities;
using Stateflows.Entities;
using Stateflows.Common;
using Stateflows.Common.Cache;
using Stateflows.Common.Classes;
using Stateflows.Common.Lock;
using Stateflows.Common.Tenant;
using Stateflows.Common.Engine;
using Stateflows.Common.Storage;
using Stateflows.Common.Context;
using Stateflows.Common.Engine.Interfaces;
using Stateflows.Common.Scheduler;
using Stateflows.Common.Extensions;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Initializer;
using Stateflows.Common.Subscription;
using Stateflows.Common.Registration.Builders;
using Stateflows.Common.Registration.Interfaces;
using Stateflows.Common.Utilities;
using Stateflows.StateMachines;
using IExecutionContext = Stateflows.Common.IExecutionContext;

namespace Stateflows
{
    /// <summary>Provides dependency-injection registration for Stateflows hosts and clients.</summary>
    /// <remarks>See the <see href="https://github.com/Stateflows/framework/wiki/behaviors">behavior hosting guide</see>.</remarks>
    public static class StateflowsDependencyInjection
    {
        /// <summary>Enables the legacy hosted runtime, including startup and scheduling services.</summary>
        /// <param name="stateflowsBuilder">The Stateflows registration builder.</param>
        /// <returns>The same builder for fluent configuration.</returns>
        public static IStateflowsBuilder AddLegacyRuntime(this IStateflowsBuilder stateflowsBuilder)
        {
            if (!stateflowsBuilder.ServiceCollection.Any(x => x.ServiceType == typeof(StateflowsEngine)))
            {
                stateflowsBuilder
                    .ServiceCollection
                    .AddSingleton<StateflowsEngine>(serviceProvider =>
                        new StateflowsEngine(serviceProvider, (StateflowsBuilder)stateflowsBuilder))
                    .AddSingleton<StateflowsService>()
                    .AddTransient<IBehaviorFactory, BehaviorFactory>()
                    .AddHostedService(provider => provider.GetRequiredService<StateflowsService>())
                    .AddHostedService<Scheduler>()
                    .AddTransient<ScheduleExecutor>()
                    .AddTransient<StartupExecutor>();
            }

            return stateflowsBuilder;
        }
        
        internal static IStateflowsBuilder EnsureStateflowServices(this IStateflowsBuilder stateflowsBuilder)
        {
            if (!stateflowsBuilder.ServiceCollection.Any(x => x.ServiceType == typeof(StateflowsResources)))
            {
                stateflowsBuilder
                    // .AddLegacyRuntime()
                    .ServiceCollection
                    .AddSingleton(_ => new StateflowsResources((StateflowsBuilder)stateflowsBuilder))
                    .AddSingleton<IStateflowsResources>(provider => provider.GetRequiredService<StateflowsResources>())
                    .AddSingleton<INotificationsHub, NotificationsHub>()
                    .AddSingleton<ITenantAccessor, TenantAccessor>()
                    .AddScoped<CommonInterceptor>()
                    .AddScoped<IStateflowsInterceptor, CommonInterceptor>()
                    .AddScoped<IStateflowsTenantExecutor, TenantExecutor>()
                    .AddTransient(provider =>
                        CommonContextHolder.ExecutionContext.Value ??
                        throw new InvalidOperationException($"No service for type '{typeof(IExecutionContext).FullName}' is available in this context.")
                    )
                    .AddTransient(provider =>
                        CommonContextHolder.BehaviorContext.Value ??
                        throw new InvalidOperationException($"No service for type '{typeof(IBehaviorContext).FullName}' is available in this context.")
                    )
                    .AddTransient(provider =>
                        CommonContextHolder.ParentBehaviorContext.Value ??
                        throw new InvalidOperationException($"No service for type '{typeof(IParentBehaviorContext).FullName}' is available in this context.")
                    )
                    .AddTransient(provider =>
                        CommonContextHolder.OwnerBehaviorContext.Value ??
                        throw new InvalidOperationException($"No service for type '{typeof(IOwnerBehaviorContext).FullName}' is available in this context.")
                    )
                    ;
            }

            return stateflowsBuilder;
        }

        /// <summary>Registers Stateflows services, behavior providers, default storage, locks, and configured behavior kinds.</summary>
        /// <param name="services">The host service collection.</param>
        /// <param name="buildAction">Configures behaviors, resources, and shared services before the registrations are finalized.</param>
        /// <returns>The service collection for further dependency-injection configuration.</returns>
        public static IServiceCollection AddStateflows(this IServiceCollection services, System.Action<IStateflowsBuilder> buildAction)
        {
            buildAction.ThrowIfNull(nameof(buildAction));

            var builder = new StateflowsBuilder(services);

            services.AddStateflowsClient(b => { });

            buildAction(builder);

            // A host may register no behaviors itself (e.g. an Orleans silo only routing to behaviors hosted elsewhere).
            builder.EnsureStateflowServices();

            services.AddSingleton(_ => builder.TypeMapper);

            if (!services.IsServiceRegistered<IStateflowsStorage>())
            {
                services.AddSingleton<IStateflowsStorage, InMemoryStorage>();
            }

            if (!services.IsServiceRegistered<IStateflowsNotificationsStorage>())
            {
                services.AddSingleton<IStateflowsNotificationsStorage, InMemoryNotificationsStorage>();
            }

            if (!services.IsServiceRegistered<IStateflowsValueStorage>())
            {
                services.AddSingleton<IStateflowsValueStorage, InMemoryValueStorage>();
            }

            if (!services.IsServiceRegistered<IStateflowsLock>())
            {
                services.AddSingleton<IStateflowsLock, InProcessLock>();
            }

            if (!services.IsServiceRegistered<IStateflowsTenantProvider>())
            {
                services.AddSingleton<IStateflowsTenantProvider, DefaultTenantProvider>();
            }
            
            if (!services.IsServiceRegistered<IStateflowsCache>())
            {
                services.AddTransient<IStateflowsCache, InMemoryCache>();
            }

            if (!services.IsServiceRegistered<IStateflowsEventFilter>())
            {
                services.AddSingleton<IStateflowsEventFilter, NoOpEventFilter>();
            }

            if (!services.IsServiceRegistered<IStateflowsSubscriber>())
            {
                services.AddScoped<IStateflowsSubscriber, StateflowsSubscriber>();
            }

            // if (!services.IsServiceRegistered<IBehaviorFactory>())
            // {
            //     services.AddSingleton(serviceProvider => new StateflowsEngine(serviceProvider, builder));
            //     services.AddSingleton<StateflowsService>();
            //     services.AddTransient<IBehaviorFactory, BehaviorFactory>();
            // }

            ActionsDependencyInjection.Build(builder);
            ActivitiesDependencyInjection.Build(builder);
            EntitiesDependencyInjection.Build(builder);
            StateMachinesDependencyInjection.Build(builder);
            builder.Events.Build();

            return services;
        }

        /// <summary>
        /// Declares that default instance of given behavior class (with instance == string.Empty) should be
        /// initialized automatically on host startup.
        /// </summary>
        /// <param name="stateflowsBuilder">Extended builder</param>
        /// <param name="behaviorClass">Class of default behavior</param>
        /// <param name="initializationRequestFactoryAsync">
        /// Factory that generates custom initialization event for default instance of a behavior.
        /// </param>
        /// <returns>The same builder for fluent configuration.</returns>
        public static IStateflowsBuilder AddDefaultInstance(this IStateflowsBuilder stateflowsBuilder, BehaviorClass behaviorClass, DefaultInstanceInitializationRequestFactoryAsync initializationRequestFactoryAsync = null)
        {
            BehaviorClassesInitializations.Instance.AddDefaultInstanceInitialization(behaviorClass, initializationRequestFactoryAsync);

            return stateflowsBuilder;
        }

        /// <summary>
        /// Registers global interceptor for all hosted behavior instances.
        /// </summary>
        /// <typeparam name="TInterceptor">Interceptor class to be registered</typeparam>
        /// <param name="stateflowsBuilder">Extended builder</param>
        /// <returns>The same builder for fluent configuration.</returns>
        public static IStateflowsBuilder AddInterceptor<TInterceptor>(this IStateflowsBuilder stateflowsBuilder)
            where TInterceptor : class, IBehaviorInterceptor
        {
            stateflowsBuilder.ServiceCollection.AddScoped<IBehaviorInterceptor, TInterceptor>();

            return stateflowsBuilder;
        }

        /// <summary>
        /// Registers global interceptor for all hosted behavior instances.
        /// </summary>
        /// <param name="stateflowsBuilder">Extended builder</param>
        /// <param name="interceptorFactory">Factory method which returns an instance of interceptor to register</param>
        /// <returns>The same builder for fluent configuration.</returns>
        public static IStateflowsBuilder AddInterceptor(this IStateflowsBuilder stateflowsBuilder, BehaviorInterceptorFactory interceptorFactory)
        {
            stateflowsBuilder.ServiceCollection.AddScoped(s => interceptorFactory(s));

            return stateflowsBuilder;
        }

        /// <summary>
        /// Registers client interceptor for all communication with behavior instances (hosted locally or remotely).
        /// </summary>
        /// <typeparam name="TClientInterceptor">Interceptor class to be registered</typeparam>
        /// <param name="stateflowsBuilder">Extended builder</param>
        /// <returns>The same builder for fluent configuration.</returns>
        public static IStateflowsBuilder AddClientInterceptor<TClientInterceptor>(this IStateflowsBuilder stateflowsBuilder)
            where TClientInterceptor : class, IStateflowsClientInterceptor
        {
            stateflowsBuilder.ServiceCollection.AddScoped<IStateflowsClientInterceptor, TClientInterceptor>();

            return stateflowsBuilder;
        }

        /// <summary>
        /// Registers client interceptor for all communication with behavior instances (hosted locally or remotely).
        /// </summary>
        /// <param name="clientInterceptorFactory">Factory method which returns an instance of interceptor to register</param>
        /// <param name="stateflowsBuilder">Extended builder</param>
        /// <returns>The same builder for fluent configuration.</returns>
        public static IStateflowsBuilder AddClientInterceptor(this IStateflowsBuilder stateflowsBuilder, ClientInterceptorFactory clientInterceptorFactory)
        {
            stateflowsBuilder.ServiceCollection.AddScoped(s => clientInterceptorFactory(s));

            return stateflowsBuilder;
        }

        /// <summary>
        /// Registers custom validator for Events that are incoming to hosted behavior instances.
        /// </summary>
        /// <typeparam name="TValidator">Validator class to be registered</typeparam>
        /// <param name="stateflowsBuilder">Extended builder</param>
        /// <returns>The same builder for fluent configuration.</returns>
        public static IStateflowsBuilder AddValidator<TValidator>(this IStateflowsBuilder stateflowsBuilder)
            where TValidator : class, IStateflowsValidator
        {
            (stateflowsBuilder as IStateflowsClientBuilder).AddValidator<TValidator>();
        
            return stateflowsBuilder;
        }
        
        /// <summary>
        /// Registers custom validator for Events that are incoming to hosted behavior instances.
        /// </summary>
        /// <param name="stateflowsBuilder">Extended builder</param>
        /// <param name="validatorFactory">Factory method which returns an instance of validator to register</param>
        /// <returns>The same builder for fluent configuration.</returns>
        public static IStateflowsBuilder AddValidator(this IStateflowsBuilder stateflowsBuilder, ValidatorFactory validatorFactory)
        {
            (stateflowsBuilder as IStateflowsClientBuilder).AddValidator(validatorFactory);
        
            return stateflowsBuilder;
        }
    }
}
