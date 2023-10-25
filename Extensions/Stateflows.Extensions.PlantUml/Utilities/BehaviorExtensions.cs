using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.Common.Interfaces;
using Stateflows.Extensions.PlantUml.Events;

namespace Stateflows
{
    public static class BehaviorExtensions
    {
        public static Task<RequestResult<PlantUmlResponse>> GetPlantUmlAsync(this IBehavior behavior)
            => behavior.RequestAsync(new PlantUmlRequest());

        public static Task<RequestResult<PlantUmlResponse>> GetPlantUmlUrlAsync(this IBehavior behavior)
            => behavior.RequestAsync(new PlantUmlRequest());
    }
}
