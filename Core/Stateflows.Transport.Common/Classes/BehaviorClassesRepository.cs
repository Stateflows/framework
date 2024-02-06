using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common;
using Stateflows.Common.Interfaces;
using Stateflows.Transport.Common.Interfaces;

namespace Stateflows.Transport.Common.Classes
{
    public abstract class BehaviorClassesRepository : IHostedService
    {
        private readonly IServiceProvider ServiceProvider;

        private readonly ILogger<BehaviorClassesRepository> Logger;

        private IBehaviorClassesProvider behaviorClassesProvider = null;
        private IBehaviorClassesProvider BehaviorClassesProvider 
            => behaviorClassesProvider ??= ServiceProvider.GetService<IBehaviorClassesProvider>();

        private readonly IBehaviorClassesDiscoverer Discoverer;

        private readonly List<BehaviorClass> behaviorClasses = new List<BehaviorClass>();

        public readonly EventWaitHandle BehaviorClassesAvailable = new EventWaitHandle(false, EventResetMode.AutoReset);

        public IEnumerable<BehaviorClass> BehaviorClasses => behaviorClasses;

        protected BehaviorClassesRepository(
            IServiceProvider serviceProvider,
            IBehaviorClassesDiscoverer discoverer,
            ILogger<BehaviorClassesRepository> logger
        )
        {
            ServiceProvider = serviceProvider;
            Discoverer = discoverer;
            Logger = logger;
        }

        protected abstract Task OnBehaviorClassesChanged();

        public Task AddBehaviorClasses(IEnumerable<BehaviorClass> behaviorClasses)
        {
            lock (this.behaviorClasses)
            {
                foreach (var behaviorClass in behaviorClasses)
                {
                    if (!this.behaviorClasses.Contains(behaviorClass))
                    {
                        this.behaviorClasses.AddRange(behaviorClasses);
                    }
                }

                BehaviorClassesAvailable.Set();
            }

            lock (this.behaviorClasses)
            {
                return OnBehaviorClassesChanged();
            }
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                await Task.Delay(10 * 1000);
                await Discoverer.DiscoverBehaviorClassesAsync(BehaviorClassesProvider.LocalBehaviorClasses);

                BehaviorClassesAvailable.WaitOne(1000 * 10);
            }
            catch (Exception e)
            {
                Logger.LogError(LogTemplates.ExceptionLogTemplate, typeof(BehaviorClassesRepository).FullName, nameof(StartAsync), e.GetType().Name, e.Message);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
