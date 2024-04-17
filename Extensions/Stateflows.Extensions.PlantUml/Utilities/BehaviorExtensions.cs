using System;
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

        public static Task WatchPlantUmlAsync(this IBehavior behavior, Action<PlantUmlNotification> handler)
            => behavior.WatchAsync(handler);

        public static Task UnwatchPlantUmlAsync(this IBehavior behavior)
            => behavior.UnwatchAsync<PlantUmlNotification>();
    }
}
