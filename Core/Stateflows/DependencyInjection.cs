using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
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
using Stateflows.StateMachines.Engine;
using IExecutionContext = Stateflows.Common.IExecutionContext;

namespace Stateflows
{
    public static class StateflowsDependencyInjection
    {
        internal static IStateflowsBuilder EnsureStateflowServices(this IStateflowsBuilder stateflowsBuilder)
        {
            if (!stateflowsBuilder.ServiceCollection.Any(x => x.ServiceType == typeof(StateflowsEngine)))
            {
                stateflowsBuilder
                    .ServiceCollection
                    .AddSingleton<StateflowsEngine>()
                    .AddSingleton<StateflowsService>()
                    .AddHostedService(provider => provider.GetRequiredService<StateflowsService>())
                    .AddSingleton<IStateflowsTelemetry>(provider => provider.GetRequiredService<StateflowsService>())
                    .AddSingleton<INotificationsHub, NotificationsHub>()
                    .AddHostedService<Scheduler>()
                    .AddTransient<ScheduleExecutor>()
                    .AddTransient<StartupExecutor>()
                    .AddSingleton<ITenantAccessor, TenantAccessor>()
                    .AddScoped<CommonInterceptor>()
                    .AddScoped<IBehaviorContextProvider, BehaviorContextProvider>()
                    .AddScoped<IStateflowsTenantExecutor, TenantExecutor>()
                    .AddTransient(provider =>
                        CommonContextHolder.ExecutionContext.Value ??
                        throw new InvalidOperationException($"No service for type '{typeof(IExecutionContext).FullName}' is available in this context.")
                    )
                    .AddTransient(provider =>
                        CommonContextHolder.BehaviorContext.Value ??
                        throw new InvalidOperationException($"No service for type '{typeof(IBehaviorContext).FullName}' is available in this context.")
                    )
                    ;
            }

            return stateflowsBuilder;
        }

        public static IServiceCollection AddStateflows(this IServiceCollection services, Action<IStateflowsBuilder> buildAction)
        {
            buildAction.ThrowIfNull(nameof(buildAction));

            var builder = new StateflowsBuilder(services);

            services.AddStateflowsClient(b => { });

            buildAction(builder);

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

            StateMachinesDependencyInjection.Build(builder);

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
        public static IStateflowsBuilder AddValidator(this IStateflowsBuilder stateflowsBuilder, ValidatorFactory validatorFactory)
        {
            (stateflowsBuilder as IStateflowsClientBuilder).AddValidator(validatorFactory);
        
            return stateflowsBuilder;
        }
    }
}
