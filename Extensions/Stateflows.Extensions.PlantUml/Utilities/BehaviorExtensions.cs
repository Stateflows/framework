using System;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.Extensions.PlantUml.Events;

namespace Stateflows
{
    /// <summary>Requests or watches PlantUML diagrams for behaviors.</summary>
    public static class BehaviorExtensions
    {
        /// <summary>Requests the current PlantUML diagram for a behavior.</summary>
        /// <param name="behavior">The behavior whose diagram is requested.</param>
        /// <returns>A task containing the request status and, when handled, its PlantUML source and image URLs.</returns>
        public static Task<RequestResult<PlantUmlInfo>> GetPlantUmlAsync(this IBehavior behavior)
            => behavior.RequestAsync(new PlantUmlInfoRequest());

        /// <summary>Watches for diagram updates and optionally requests the current diagram immediately.</summary>
        /// <param name="behavior">The behavior whose diagram is watched.</param>
        /// <param name="handler">Receives each published diagram update.</param>
        /// <param name="immediateRequest">When <see langword="true"/>, also requests the current diagram after creating the watch.</param>
        /// <returns>A watcher that can be disposed to stop observing updates.</returns>
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
        
        /// <summary>Watches for diagram updates using an asynchronous handler.</summary>
        /// <param name="behavior">The behavior whose diagram is watched.</param>
        /// <param name="asyncHandler">Asynchronously processes each published diagram update.</param>
        /// <param name="immediateRequest">When <see langword="true"/>, also requests the current diagram after creating the watch.</param>
        /// <returns>A task containing a watcher that can be disposed to stop observing updates.</returns>
        public static Task<IWatcher> WatchPlantUmlAsync(this IBehavior behavior, Func<PlantUmlInfo, Task> asyncHandler, bool immediateRequest = true)
            => WatchPlantUmlAsync(behavior, handler: n => _ = asyncHandler(n), immediateRequest);
    }
}
