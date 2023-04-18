using Microsoft.Extensions.DependencyInjection;
using Stateflows.StateMachines;
using Stateflows.Common.Registration.Interfaces;
using Stateflows.Extensions.PlantUml.Classes;

namespace Stateflows
{
    public static class DependencyInjection
    {
        public static IStateflowsBuilder AddPlantUml(this IStateflowsBuilder builder)
        {
            builder.Services.AddSingleton<IStateMachineEventHandler, PlantUmlHandler>();

            return builder;
        }
    }
}
