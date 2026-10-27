using Microsoft.Extensions.DependencyInjection;
using Stateflows.Activities;
using Stateflows.StateMachines;
using Stateflows.Common.Registration.Interfaces;
using Stateflows.Extensions.PlantUml.Classes;

namespace Stateflows
{
    /// <summary>Registers PlantUML generation for Stateflows state machines and activities.</summary>
    public static class DependencyInjection
    {
        /// <summary>Adds PlantUML interceptors and the handler that serves diagram requests.</summary>
        /// <param name="builder">The Stateflows builder to configure.</param>
        /// <returns>The same builder, for further Stateflows configuration.</returns>
        public static IStateflowsBuilder AddPlantUml(this IStateflowsBuilder builder)
        {
            builder
                .AddStateMachines(b => b
                    .AddInterceptor<PlantUmlStateMachineInterceptor>()
                )
                .AddActivities(b => b
                    .AddInterceptor<PlantUmlActivityInterceptor>()
                )
                .ServiceCollection
                .AddSingleton<PlantUmlHandler>()
                .AddSingleton<IActivityEventHandler>(services => services.GetService<PlantUmlHandler>())
                .AddSingleton<IStateMachineEventHandler>(services => services.GetService<PlantUmlHandler>());

            return builder;
        }
    }
}
