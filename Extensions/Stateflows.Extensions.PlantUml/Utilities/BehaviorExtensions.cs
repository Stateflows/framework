using System;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.Extensions.PlantUml.Events;

namespace Stateflows
{
    public static class BehaviorExtensions
    {
        public static Task<RequestResult<PlantUmlInfo>> GetPlantUmlAsync(this IBehavior behavior)
            => behavior.RequestAsync(new PlantUmlInfoRequest());

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
        
        public static Task<IWatcher> WatchPlantUmlAsync(this IBehavior behavior, Func<PlantUmlInfo, Task> asyncHandler, bool immediateRequest = true)
            => WatchPlantUmlAsync(behavior, handler: n => _ = asyncHandler(n), immediateRequest);
    }
}
