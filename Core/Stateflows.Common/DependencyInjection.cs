using System;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common;
using Stateflows.Common.Engine;
using Stateflows.Common.Locator;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Extensions;
using Stateflows.Common.StateMachines.Classes;
using Stateflows.Common.Registration.Builders;
using Stateflows.Common.Registration.Interfaces;
using Stateflows.StateMachines;

namespace Stateflows
{
    public static class StateflowsCommonDependencyInjection
    {
        public static IServiceCollection AddStateflowsClient(this IServiceCollection services, Action<IStateflowsClientBuilder> builderAction)
        {
            builderAction.ThrowIfNull(nameof(builderAction));

            if (services.IsServiceRegistered<BehaviorLocator>()) throw new Exception("Stateflows client already registered");

            var builder = new StateflowsClientBuilder(services);

            builderAction(builder);

            services
                .AddTransient<ClientInterceptor>()
                .AddTransient<IBehaviorLocator, BehaviorLocator>()
                .AddTransient<IStateMachineLocator, StateMachineLocator>()
                .AddSingleton<IBehaviorClassesProvider, BehaviorClassesProvider>()
                ;

            return services;
        }

        public static IStateflowsClientBuilder AddClientInterceptor<TClientInterceptor>(this IStateflowsClientBuilder stateflowsBuilder)
            where TClientInterceptor : class, IBehaviorClientInterceptor
        {
            stateflowsBuilder.ServiceCollection.AddScoped<IBehaviorClientInterceptor, TClientInterceptor>();

            return stateflowsBuilder;
        }

        public static IStateflowsClientBuilder AddClientInterceptor(this IStateflowsClientBuilder stateflowsBuilder, BehaviorClientInterceptorFactory envelopeHandlerFactory)
        {
            stateflowsBuilder.ServiceCollection.AddScoped(s => envelopeHandlerFactory(s));

            return stateflowsBuilder;
        }
    }
}