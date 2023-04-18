using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common.Classes;
using Stateflows.Common.Extensions;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Registration.Builders;
using Stateflows.Common.Registration.Interfaces;
using Stateflows.StateMachines.Engine;

namespace Stateflows
{
    public static class StateflowsDependencyInjection
    {
        internal static IStateflowsBuilder EnsureStateflowServices(this IStateflowsBuilder stateflowsBuilder)
        {
            if (!stateflowsBuilder.Services.Any(x => x.ServiceType == typeof(StateflowsEngine)))
            {
                stateflowsBuilder
                    .Services
                    .AddSingleton<StateflowsEngine>()
                    .AddHostedService(provider => provider.GetService<StateflowsEngine>())
                    .AddSingleton<ITimeService, TimeService>()
                    .AddHostedService(provider => provider.GetService<ITimeService>() as TimeService)
                    ;
            }

            return stateflowsBuilder;
        }
        public static IServiceCollection AddStateflows(this IServiceCollection services, Action<IStateflowsBuilder> builderAction)
        {
            if (builderAction == null) throw new ArgumentNullException(nameof(builderAction));

            var register = new StateflowsBuilder(services);

            builderAction(register);

            if (!services.IsServiceRegistered<IStateflowsStorage>())
            {
                services.AddSingleton<IStateflowsStorage, InMemoryStorage>();
            }

            if (!services.IsServiceRegistered<IStateflowsLock>())
            {
                services.AddSingleton<IStateflowsLock, InProcessLock>();
            }

            services.AddStateflowsClient(b => { });

            return services;
        }
    }
}
