﻿using System;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.System;
using Stateflows.Activities;
using Stateflows.StateMachines;
using Stateflows.Common;
using Stateflows.Common.Engine;
using Stateflows.Common.Locator;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Extensions;
using Stateflows.Common.System.Classes;
using Stateflows.Common.Activities.Classes;
using Stateflows.Common.StateMachines.Classes;
using Stateflows.Common.Registration.Builders;
using Stateflows.Common.Registration.Interfaces;

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
                .AddTransient<IActivityLocator, ActivityLocator>()
                .AddTransient<ISystem>((IServiceProvider provider) =>
                {
                    if (provider.GetRequiredService<IBehaviorLocator>().TryLocateBehavior(SystemBehavior.Id, out var behavior))
                    {
                        return new SystemWrapper(behavior);
                    }
                    else
                    {
                        throw new StateflowsException("System behavior could not be found");
                    }
                })
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