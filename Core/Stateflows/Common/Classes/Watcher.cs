using System;

namespace Stateflows.Common.Classes
{
    public sealed class Watcher : IWatcher
    {
        private readonly IUnwatcher unwatcher;

        public Watcher(IUnwatcher unwatcher)
        {
            this.unwatcher = unwatcher;
        }
        
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            unwatcher.UnwatchAsync(this).GetAwaiter().GetResult();
        }

        ~Watcher()
            => Dispose(false);
    }
}
