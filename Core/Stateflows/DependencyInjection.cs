using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common;
using Stateflows.Common.Lock;
using Stateflows.Common.Tenant;
using Stateflows.Common.Engine;
using Stateflows.Common.Storage;
using Stateflows.Common.Scheduler;
using Stateflows.Common.Extensions;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Initializer;
using Stateflows.Common.Registration.Builders;
using Stateflows.Common.Registration.Interfaces;
using Stateflows.System;

namespace Stateflows
{
    public static class StateflowsDependencyInjection
    {
        internal static IStateflowsBuilder EnsureStateflowServices(this IStateflowsBuilder stateflowsBuilder)
        {
            if (!stateflowsBuilder.ServiceCollection.Any(x => x.ServiceType == typeof(StateflowsEngine)))
            {
                stateflowsBuilder
                    .EnsureSystemServices()
                    .ServiceCollection
                    .AddSingleton<StateflowsEngine>()
                    .AddHostedService(provider => provider.GetService<StateflowsEngine>())
                    .AddHostedService<ThreadScheduler>()
                    .AddHostedService<ThreadInitializer>()
                    .AddScoped<CommonInterceptor>()
                    ;
            }

            return stateflowsBuilder;
        }

        public static IServiceCollection AddStateflows(this IServiceCollection services, Action<IStateflowsBuilder> builderAction)
        {
            builderAction.ThrowIfNull(nameof(builderAction));

            var builder = new StateflowsBuilder(services);

            services.AddStateflowsClient(b => { });

            builderAction(builder);

            if (!services.IsServiceRegistered<IStateflowsStorage>())
            {
                services.AddSingleton<IStateflowsStorage, InMemoryStorage>();
            }

            if (!services.IsServiceRegistered<IStateflowsLock>())
            {
                services.AddSingleton<IStateflowsLock, InProcessLock>();
            }

            if (!services.IsServiceRegistered<IStateflowsTenantsManager>())
            {
                services.AddSingleton<IStateflowsTenantsManager, SingleTenantManager>();
            }

            return services;
        }

        public static IStateflowsBuilder AddDefaultInstance(this IStateflowsBuilder stateflowsBuilder, BehaviorClass behaviorClass, InitializationRequestFactoryAsync initializationRequestFactoryAsync = null)
        {
            BehaviorClassesInitializations.Instance.Initialize(behaviorClass, initializationRequestFactoryAsync);

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
    }
}
