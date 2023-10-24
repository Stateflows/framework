using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common;
using Stateflows.Common.Classes;
using Stateflows.Common.Engine;
using Stateflows.Common.Extensions;
using Stateflows.Common.Interfaces;
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
                    .AddHostedService(provider => provider.GetService<StateflowsEngine>())
                    .AddSingleton<ITimeService, TimeService>()
                    .AddHostedService(provider => provider.GetService<ITimeService>() as TimeService)
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

            return services;
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
            where TClientInterceptor : class, IBehaviorClientInterceptor
        {
            stateflowsBuilder.ServiceCollection.AddScoped<IBehaviorClientInterceptor, TClientInterceptor>();

            return stateflowsBuilder;
        }

        public static IStateflowsBuilder AddClientInterceptor(this IStateflowsBuilder stateflowsBuilder, BehaviorClientInterceptorFactory clientInterceptorFactory)
        {
            stateflowsBuilder.ServiceCollection.AddScoped(s => clientInterceptorFactory(s));

            return stateflowsBuilder;
        }
    }
}
