using System.Threading.Tasks;
using System.Collections.Generic;
using Stateflows.Common.Interfaces;

namespace Stateflows.Common.Locator
{
    internal class BehaviorLocator : IBehaviorLocator
    {
        private IEnumerable<IBehaviorProvider> Providers { get; }

        private IDictionary<BehaviorClass, IBehaviorProvider> ProvidersByClasses { get; } = new Dictionary<BehaviorClass, IBehaviorProvider>();

        public BehaviorLocator(IEnumerable<IBehaviorProvider> providers)
        {
            Providers = providers;

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
            
            return
                ProvidersByClasses.TryGetValue(id.BehaviorClass, out var provider) &&
                provider.TryProvideBehavior(id, out behavior);
        }
    }
}
