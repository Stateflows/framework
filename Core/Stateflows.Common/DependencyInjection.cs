using System;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Activities;
using Stateflows.StateMachines;
using Stateflows.Common;
using Stateflows.Common.Engine;
using Stateflows.Common.Locator;
using Stateflows.Common.Validators;
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

            if (services.IsServiceRegistered<BehaviorLocator>()) throw new StateflowsDefinitionException("Stateflows client already registered");

            var builder = new StateflowsClientBuilder(services);

            buildAction(builder);

            services
                .AddScoped<ClientInterceptor>()
                .AddScoped<IBehaviorLocator, BehaviorLocator>()
                .AddScoped<IStateMachineLocator, StateMachineLocator>()
                .AddScoped<IActivityLocator, ActivityLocator>()
                .AddSingleton<IBehaviorClassesProvider, BehaviorClassesProvider>()
                ;

            builder.AddValidator<AttributeValidator>();

            return services;
        }

        public static IStateflowsClientBuilder AddClientInterceptor<TClientInterceptor>(this IStateflowsClientBuilder stateflowsBuilder)
            where TClientInterceptor : class, IStateflowsClientInterceptor
        {
            stateflowsBuilder.ServiceCollection.AddScoped<IStateflowsClientInterceptor, TClientInterceptor>();

            return stateflowsBuilder;
        }

        public static IStateflowsClientBuilder AddClientInterceptor(this IStateflowsClientBuilder stateflowsBuilder, ClientInterceptorFactory clientInterceptorFactory)
        {
            stateflowsBuilder.ServiceCollection.AddScoped(s => clientInterceptorFactory(s));

            return stateflowsBuilder;
        }

        public static IStateflowsClientBuilder AddValidator<TValidator>(this IStateflowsClientBuilder stateflowsBuilder)
            where TValidator : class, IStateflowsValidator
        {
            stateflowsBuilder.ServiceCollection.AddScoped<IStateflowsValidator, TValidator>();
        
            return stateflowsBuilder;
        }
        
        public static IStateflowsClientBuilder AddValidator(this IStateflowsClientBuilder stateflowsBuilder, ValidatorFactory validatorFactory)
        {
            stateflowsBuilder.ServiceCollection.AddScoped(s => validatorFactory(s));
        
            return stateflowsBuilder;
        }
    }
}