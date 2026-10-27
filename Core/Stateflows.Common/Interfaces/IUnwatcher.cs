using System.Threading.Tasks;

namespace Stateflows.Common
{
    /// <summary>Stops a previously created notification watcher.</summary>
    public interface IUnwatcher
    {
        /// <summary>Ends the specified watcher subscription.</summary>
        /// <param name="watcher">The watcher to stop.</param>
        /// <returns>A task that completes when the subscription has ended.</returns>
        Task UnwatchAsync(IWatcher watcher);
    }
}
