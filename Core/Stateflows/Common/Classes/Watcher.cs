using System;

namespace Stateflows.Common.Classes
{
    public class Watcher<TNotificationEvent> : IWatcher
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

        protected virtual void Dispose(bool disposing)
        {
            unwatcher.UnwatchAsync<TNotificationEvent>().GetAwaiter().GetResult();
        }

        ~Watcher()
            => Dispose(false);
    }
}
