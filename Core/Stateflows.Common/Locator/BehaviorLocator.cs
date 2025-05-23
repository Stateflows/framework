﻿using System;
using System.Threading.Tasks;
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

        private IDictionary<BehaviorClass, IBehaviorProvider> ProvidersByClasses { get; } = new Dictionary<BehaviorClass, IBehaviorProvider>();

        public BehaviorLocator(IEnumerable<IBehaviorProvider> providers, ClientInterceptor interceptor, IServiceProvider serviceProvider)
        {
            Providers = providers;
            Interceptor = interceptor;
            ServiceProvider = serviceProvider;

            foreach (var provider in Providers)
            {
                provider.BehaviorClassesChanged += changedProvider => RegisterProviderClasses(changedProvider);

                RegisterProviderClasses(provider);
            }
        }

        private Task RegisterProviderClasses(IBehaviorProvider provider)
        {
            foreach (var behaviorClass in provider.BehaviorClasses)
            {
                if (!ProvidersByClasses.ContainsKey(behaviorClass) || provider.IsLocal)
                {
                    ProvidersByClasses[behaviorClass] = provider;
                }
            }

            return Task.CompletedTask;
        }

        public bool TryLocateBehavior(BehaviorId id, out IBehavior behavior)
        {
            behavior = null;

            var result = false;

            if (ProvidersByClasses.TryGetValue(id.BehaviorClass, out var provider) && provider.TryProvideBehavior(id, out behavior))
            {
                behavior = new BehaviorProxy(behavior, Interceptor, ServiceProvider);

                result = true;
            }

            return result;
        }
    }
}
