using System;
using System.Linq;
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
        IServiceProvider ServiceProvider { get; }

        private IBehaviorClassesProvider behaviorClassesProvider = null;
        private IBehaviorClassesProvider BehaviorClassesProvider 
            => behaviorClassesProvider ?? (behaviorClassesProvider = ServiceProvider.GetService<IBehaviorClassesProvider>());

        private IBehaviorClassesDiscoverer Discoverer { get; }

        public BehaviorClassesRepository(
            IServiceProvider serviceProvider,
            IBehaviorClassesDiscoverer discoverer)
        {
            ServiceProvider = serviceProvider;
            Discoverer = discoverer;
        }

        private List<BehaviorClass> behaviorClasses = new List<BehaviorClass>();

        public IEnumerable<BehaviorClass> BehaviorClasses => behaviorClasses;

        protected abstract Task OnBehaviorClassesChanged();

        public Task AddBehaviorClasses(IEnumerable<BehaviorClass> behaviorClasses)
        {
            lock (this.behaviorClasses)
            {
                this.behaviorClasses.AddRange(behaviorClasses);
                this.behaviorClasses = this.behaviorClasses.Distinct().ToList();
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
