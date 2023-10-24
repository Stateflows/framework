using Microsoft.Extensions.DependencyInjection;
//using Stateflows.Activities;
using Stateflows.StateMachines;
using Stateflows.Common.Registration.Interfaces;
using Stateflows.Extensions.PlantUml.Classes;

namespace Stateflows
{
    public static class DependencyInjection
    {
        public static IStateflowsBuilder AddPlantUml(this IStateflowsBuilder builder)
        {
            builder.ServiceCollection.AddSingleton<PlantUmlHandler>();
            builder.ServiceCollection.AddSingleton<IStateMachineEventHandler>(services => services.GetService<PlantUmlHandler>());
            //builder.ServiceCollection.AddSingleton<IActivityEventHandler>(services => services.GetService<PlantUmlHandler>());

            return builder;
        }
    }
}
