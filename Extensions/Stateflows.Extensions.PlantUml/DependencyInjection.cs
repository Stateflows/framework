using Microsoft.Extensions.DependencyInjection;
using Stateflows.Activities;
using Stateflows.StateMachines;
using Stateflows.Common.Registration.Interfaces;
using Stateflows.Extensions.PlantUml.Classes;

namespace Stateflows
{
    public static class DependencyInjection
    {
        public static IStateflowsBuilder AddPlantUml(this IStateflowsBuilder builder)
        {
            builder
                .AddStateMachines(b => b
                    .AddInterceptor<PlantUmlStateMachineInterceptor>()
                )
                .ServiceCollection
                .AddSingleton<PlantUmlHandler>()
                .AddSingleton<IActivityEventHandler>(services => services.GetService<PlantUmlHandler>())
                .AddSingleton<IStateMachineEventHandler>(services => services.GetService<PlantUmlHandler>());

            return builder;
        }
    }
}
