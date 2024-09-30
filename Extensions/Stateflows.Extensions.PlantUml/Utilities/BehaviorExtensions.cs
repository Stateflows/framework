using System;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.Extensions.PlantUml.Events;

namespace Stateflows
{
    public static class BehaviorExtensions
    {
        public static Task<RequestResult<PlantUmlInfo>> GetPlantUmlAsync(this IBehavior behavior)
            => behavior.RequestAsync(new PlantUmlRequest());

        public static async Task<IWatcher> WatchPlantUmlAsync(this IBehavior behavior, Action<PlantUmlInfo> handler, bool immediateRequest = true)
        {
            var watcher = await behavior.WatchAsync(handler);

            if (immediateRequest)
            {
                var result = await behavior.GetPlantUmlAsync();
                if (result.Status == EventStatus.Consumed)
                {
                    _ = Task.Run(() => handler(result.Response));
                }
            }

            return watcher;
        }
    }
}
