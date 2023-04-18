using System.Threading.Tasks;
using Stateflows.Extensions.PlantUml.Events;

namespace Stateflows.StateMachines
{
    public static class StateMachineExtensions
    {
        public static async Task<string> GetPlantUmlAsync(this IStateMachine stateMachine)
            => (await stateMachine.RequestAsync(new PlantUmlRequest()))?.PlantUml ?? "";

        public static async Task<string> GetPlantUmlUrlAsync(this IStateMachine stateMachine)
            => (await stateMachine.RequestAsync(new PlantUmlRequest()))?.PlantUmlUrl ?? "";
    }
}
