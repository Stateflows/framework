using System;
using System.Linq;
using System.Collections.Generic;
using Stateflows.Common.Engine;
using Stateflows.Common.Interfaces;

namespace Stateflows.Common.Locator
{
    internal class BehaviorLocator : IBehaviorLocator
    {
        public IEnumerable<IBehaviorProvider> Providers { get; }

        public ClientInterceptor Interceptor { get; }

        public IServiceProvider ServiceProvider { get; }

        // Local providers take precedence over remote ones. Providers are queried on each lookup (instead of caching
        // their classes and subscribing to changes), so a scoped locator holds no references from singleton providers.
        private readonly IBehaviorProvider[] orderedProviders;

        public BehaviorLocator(IEnumerable<IBehaviorProvider> providers, ClientInterceptor interceptor, IServiceProvider serviceProvider)
        {
            Providers = providers;
            Interceptor = interceptor;
            ServiceProvider = serviceProvider;
            orderedProviders = providers.OrderBy(provider => provider.IsLocal ? 0 : 1).ToArray();
        }

        public bool TryLocateBehavior(BehaviorId id, out IBehavior behavior)
        {
            foreach (var provider in orderedProviders)
            {
                if (provider.TryProvideBehavior(id, out var providedBehavior))
                {
                    behavior = new BehaviorProxy(providedBehavior, Interceptor, ServiceProvider);
                    return true;
                }
            }

            behavior = null;
            return false;
        }
    }
}