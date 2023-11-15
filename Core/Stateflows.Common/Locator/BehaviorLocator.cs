using System.Threading.Tasks;
using System.Collections.Generic;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Engine;

namespace Stateflows.Common.Locator
{
    internal class BehaviorLocator : IBehaviorLocator
    {
        public IEnumerable<IBehaviorProvider> Providers { get; }

        public ClientInterceptor Interceptor { get; }

        private IDictionary<BehaviorClass, IBehaviorProvider> ProvidersByClasses { get; } = new Dictionary<BehaviorClass, IBehaviorProvider>();

        public BehaviorLocator(IEnumerable<IBehaviorProvider> providers, ClientInterceptor interceptor)
        {
            Providers = providers;
            Interceptor = interceptor;

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
                if (!ProvidersByClasses.ContainsKey(behaviorClass))
                {
                    ProvidersByClasses.Add(behaviorClass, provider);
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
                behavior = new BehaviorProxy(behavior, Interceptor);

                result = true;
            }

            return result;
        }
    }
}
