using System;
using System.Threading.Tasks;

namespace Stateflows.Common
{
    public interface IWatcher : IDisposable, IAsyncDisposable
    {
        public Task UnwatchAsync()
        {
            Dispose();

            return Task.CompletedTask;
        }
    }
}
