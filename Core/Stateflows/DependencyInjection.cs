using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common;
using Stateflows.Common.Classes;
using Stateflows.Common.Lock;
using Stateflows.Common.Tenant;
using Stateflows.Common.Engine;
using Stateflows.Common.Storage;
using Stateflows.Common.Context;
using Stateflows.Common.Scheduler;
using Stateflows.Common.Extensions;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Initializer;
using Stateflows.Common.Subscription;
using Stateflows.Common.Registration.Builders;
using Stateflows.Common.Registration.Interfaces;

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
                    .AddHostedService(provider => provider.GetService<StateflowsService>())
                    .AddSingleton<NotificationsHub>()
                    .AddHostedService(provider => provider.GetService<NotificationsHub>())
                    .AddSingleton<INotificationsHub>(provider => provider.GetService<NotificationsHub>())
                    .AddHostedService<Scheduler>()
                    .AddTransient<ScheduleExecutor>()
                    .AddTransient<StartupExecutor>()
                    .AddSingleton<ITenantAccessor, TenantAccessor>()
                    .AddScoped<CommonInterceptor>()
                    .AddScoped<IStateflowsTenantExecutor, TenantExecutor>()
                    .AddTransient(provider =>
                        CommonContextHolder.ExecutionContext.Value ??
                        throw new InvalidOperationException($"No service for type '{typeof(IExecutionContext).FullName}' is available in this context.")
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

            if (!services.IsServiceRegistered<IStateflowsLock>())
            {
                services.AddSingleton<IStateflowsLock, InProcessLock>();
            }

            if (!services.IsServiceRegistered<IStateflowsTenantProvider>())
            {
                services.AddSingleton<IStateflowsTenantProvider, DefaultTenantProvider>();
            }

            return services;
        }

        public static IStateflowsBuilder AddDefaultInstance(this IStateflowsBuilder stateflowsBuilder, BehaviorClass behaviorClass, DefaultInstanceInitializationRequestFactoryAsync initializationRequestFactoryAsync = null)
        {
            BehaviorClassesInitializations.Instance.AddDefaultInstanceInitialization(behaviorClass, initializationRequestFactoryAsync);

            return stateflowsBuilder;
        }

        public static IStateflowsBuilder AddInterceptor<TInterceptor>(this IStateflowsBuilder stateflowsBuilder)
            where TInterceptor : class, IBehaviorInterceptor
        {
            stateflowsBuilder.ServiceCollection.AddScoped<IBehaviorInterceptor, TInterceptor>();

            return stateflowsBuilder;
        }

        public static IStateflowsBuilder AddInterceptor(this IStateflowsBuilder stateflowsBuilder, BehaviorInterceptorFactory interceptorFactory)
        {
            stateflowsBuilder.ServiceCollection.AddScoped(s => interceptorFactory(s));

            return stateflowsBuilder;
        }

        public static IStateflowsBuilder AddClientInterceptor<TClientInterceptor>(this IStateflowsBuilder stateflowsBuilder)
            where TClientInterceptor : class, IStateflowsClientInterceptor
        {
            stateflowsBuilder.ServiceCollection.AddScoped<IStateflowsClientInterceptor, TClientInterceptor>();

            return stateflowsBuilder;
        }

        public static IStateflowsBuilder AddClientInterceptor(this IStateflowsBuilder stateflowsBuilder, ClientInterceptorFactory clientInterceptorFactory)
        {
            stateflowsBuilder.ServiceCollection.AddScoped(s => clientInterceptorFactory(s));

            return stateflowsBuilder;
        }

        public static IStateflowsBuilder AddValidator<TValidator>(this IStateflowsBuilder stateflowsBuilder)
            where TValidator : class, IStateflowsValidator
        {
            (stateflowsBuilder as IStateflowsClientBuilder).AddValidator<TValidator>();
        
            return stateflowsBuilder;
        }
        
        public static IStateflowsBuilder AddValidator(this IStateflowsBuilder stateflowsBuilder, ValidatorFactory validatorFactory)
        {
            (stateflowsBuilder as IStateflowsClientBuilder).AddValidator(validatorFactory);
        
            return stateflowsBuilder;
        }
    }
}
