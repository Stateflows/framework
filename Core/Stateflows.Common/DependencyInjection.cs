using System;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Activities;
using Stateflows.StateMachines;
using Stateflows.Common;
using Stateflows.Common.Engine;
using Stateflows.Common.Locator;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Extensions;
using Stateflows.Common.Exceptions;
using Stateflows.Common.Activities.Classes;
using Stateflows.Common.StateMachines.Classes;
using Stateflows.Common.Registration.Builders;
using Stateflows.Common.Registration.Interfaces;

namespace Stateflows
{
    public static class StateflowsCommonDependencyInjection
    {
        public static IServiceCollection AddStateflowsClient(this IServiceCollection services, Action<IStateflowsClientBuilder> buildAction)
        {
            buildAction.ThrowIfNull(nameof(buildAction));

            if (services.IsServiceRegistered<BehaviorLocator>()) throw new StateflowsException("Stateflows client already registered");

            var builder = new StateflowsClientBuilder(services);

            buildAction(builder);

            services
                .AddScoped<ClientInterceptor>()
                .AddScoped<IBehaviorLocator, BehaviorLocator>()
                .AddScoped<IStateMachineLocator, StateMachineLocator>()
                .AddScoped<IActivityLocator, ActivityLocator>()
                .AddSingleton<IBehaviorClassesProvider, BehaviorClassesProvider>()
                ;

            return services;
        }

        public static IStateflowsClientBuilder AddClientInterceptor<TClientInterceptor>(this IStateflowsClientBuilder stateflowsBuilder)
            where TClientInterceptor : class, IStateflowsClientInterceptor
        {
            stateflowsBuilder.ServiceCollection.AddScoped<IStateflowsClientInterceptor, TClientInterceptor>();

            return stateflowsBuilder;
        }

        public static IStateflowsClientBuilder AddClientInterceptor(this IStateflowsClientBuilder stateflowsBuilder, ClientInterceptorFactory envelopeHandlerFactory)
        {
            stateflowsBuilder.ServiceCollection.AddScoped(s => envelopeHandlerFactory(s));

            return stateflowsBuilder;
        }
    }
}