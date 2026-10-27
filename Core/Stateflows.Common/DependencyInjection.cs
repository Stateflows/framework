using System;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Actions;
using Stateflows.Activities;
using Stateflows.StateMachines;
using Stateflows.Entities;
using Stateflows.Common;
using Stateflows.Common.Actions.Classes;
using Stateflows.Common.Engine;
using Stateflows.Common.Locator;
using Stateflows.Common.Validators;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Extensions;
using Stateflows.Common.Exceptions;
using Stateflows.Common.Activities.Classes;
using Stateflows.Common.Entities.Classes;
using Stateflows.Common.StateMachines.Classes;
using Stateflows.Common.Registration.Builders;
using Stateflows.Common.Registration.Interfaces;
using Stateflows.Common.Utilities;

namespace Stateflows
{
    /// <summary>Registers services used by Stateflows clients and configures client-side policies.</summary>
    public static class StateflowsCommonDependencyInjection
    {
        /// <summary>Adds behavior locators, client interceptors, validators, and behavior-class discovery to a service collection.</summary>
        /// <param name="services">The service collection to extend.</param>
        /// <param name="buildAction">Configures client-side interceptors and validators.</param>
        /// <returns>The service collection for further dependency-injection configuration.</returns>
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
                .AddScoped<IActionLocator, ActionLocator>()
                .AddScoped<IEntityLocator, EntityLocator>()
                .AddSingleton<IBehaviorClassesProvider, BehaviorClassesProvider>()
                ;

            builder.AddValidator<AttributeValidator>();

            return services;
        }

        /// <summary>Registers a client interceptor implementation.</summary>
        /// <typeparam name="TClientInterceptor">The interceptor implementation type.</typeparam>
        /// <param name="stateflowsBuilder">The client builder.</param>
        /// <returns>The builder for fluent configuration.</returns>
        public static IStateflowsClientBuilder AddClientInterceptor<TClientInterceptor>(this IStateflowsClientBuilder stateflowsBuilder)
            where TClientInterceptor : class, IStateflowsClientInterceptor
        {
            stateflowsBuilder.ServiceCollection.AddScoped<IStateflowsClientInterceptor, TClientInterceptor>();

            return stateflowsBuilder;
        }

        /// <summary>Registers a client interceptor factory.</summary>
        /// <param name="stateflowsBuilder">The client builder.</param>
        /// <param name="clientInterceptorFactory">Creates an interceptor from the current service provider.</param>
        /// <returns>The builder for fluent configuration.</returns>
        public static IStateflowsClientBuilder AddClientInterceptor(this IStateflowsClientBuilder stateflowsBuilder, ClientInterceptorFactory clientInterceptorFactory)
        {
            stateflowsBuilder.ServiceCollection.AddScoped(s => clientInterceptorFactory(s));

            return stateflowsBuilder;
        }

        /// <summary>Registers a validator implementation for incoming event payloads.</summary>
        /// <typeparam name="TValidator">The validator implementation type.</typeparam>
        /// <param name="stateflowsBuilder">The client builder.</param>
        /// <returns>The builder for fluent configuration.</returns>
        public static IStateflowsClientBuilder AddValidator<TValidator>(this IStateflowsClientBuilder stateflowsBuilder)
            where TValidator : class, IStateflowsValidator
        {
            stateflowsBuilder.ServiceCollection.AddScoped<IStateflowsValidator, TValidator>();
        
            return stateflowsBuilder;
        }
        
        /// <summary>Registers a validator factory for incoming event payloads.</summary>
        /// <param name="stateflowsBuilder">The client builder.</param>
        /// <param name="validatorFactory">Creates a validator from the current service provider.</param>
        /// <returns>The builder for fluent configuration.</returns>
        public static IStateflowsClientBuilder AddValidator(this IStateflowsClientBuilder stateflowsBuilder, ValidatorFactory validatorFactory)
        {
            stateflowsBuilder.ServiceCollection.AddScoped(s => validatorFactory(s));
        
            return stateflowsBuilder;
        }
    }
}