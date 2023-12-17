using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common.Interfaces;
using Stateflows.Transport.Common.Interfaces;

namespace Stateflows.Transport.Common.Classes
{
    public abstract class BehaviorClassesRepository : IHostedService
    {
        private readonly IServiceProvider ServiceProvider;

        private IBehaviorClassesProvider behaviorClassesProvider = null;
        private IBehaviorClassesProvider BehaviorClassesProvider 
            => behaviorClassesProvider ??= ServiceProvider.GetService<IBehaviorClassesProvider>();

        private readonly IBehaviorClassesDiscoverer Discoverer;

        private readonly List<BehaviorClass> behaviorClasses = new List<BehaviorClass>();

        public IEnumerable<BehaviorClass> BehaviorClasses => behaviorClasses;

        protected BehaviorClassesRepository(
            IServiceProvider serviceProvider,
            IBehaviorClassesDiscoverer discoverer)
        {
            ServiceProvider = serviceProvider;
            Discoverer = discoverer;
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
                await Discoverer.DiscoverBehaviorClassesAsync(BehaviorClassesProvider.LocalBehaviorClasses);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
