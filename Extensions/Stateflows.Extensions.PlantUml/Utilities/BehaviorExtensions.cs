using System.Threading.Tasks;
using Stateflows.Common.Interfaces;
using Stateflows.Extensions.PlantUml.Events;

namespace Stateflows
{
    public static class BehaviorExtensions
    {
        public static async Task<string> GetPlantUmlAsync(this IBehavior behavior)
            => (await behavior.RequestAsync(new PlantUmlRequest())).Response?.PlantUml ?? string.Empty;

        public static async Task<string> GetPlantUmlUrlAsync(this IBehavior behavior)
            => (await behavior.RequestAsync(new PlantUmlRequest())).Response?.PlantUmlUrl ?? string.Empty;
    }
}
