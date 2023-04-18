using System;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.StateMachines;
using Stateflows.Common.Locator;
using Stateflows.Common.Interfaces;
using Stateflows.Common.StateMachines.Classes;
using Stateflows.Common.Registration.Builders;
using Stateflows.Common.Registration.Interfaces;

namespace Stateflows
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddStateflowsClient(this IServiceCollection services, Action<IStateflowsClientBuilder> builderAction)
        {
            if (builderAction == null) throw new ArgumentNullException(nameof(builderAction));

            var register = new StateflowsBuilder(services);

            builderAction(register);

            services
                .AddTransient<IBehaviorLocator, BehaviorLocator>()
                .AddTransient<IStateMachineLocator, StateMachineLocator>()
                .AddSingleton<IBehaviorClassesProvider, BehaviorClassesProvider>()
                ;

            return services;
        }
    }
}