using System;
using System.Threading.Tasks;

namespace Stateflows.Common
{
    public interface IWatcher : IDisposable
    {
        public Task UnwatchAsync()
        {
            Dispose();

            return Task.CompletedTask;
        }
    }
}
