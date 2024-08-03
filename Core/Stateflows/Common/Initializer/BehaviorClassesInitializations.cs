using System.Threading.Tasks;
using System.Collections.Generic;

namespace Stateflows.Common.Initializer
{
    internal class BehaviorClassesInitializations
    {
        public static readonly BehaviorClassesInitializations Instance = new BehaviorClassesInitializations();

        public readonly List<DefaultInstanceInitializationToken> DefaultInstanceInitializationTokens = new List<DefaultInstanceInitializationToken>();

        private static readonly DefaultInstanceInitializationRequestFactoryAsync DefaultDefaultInstanceFactory = (serviceProvider, behaviorClass) => Task.FromResult(new Initialize() as Event);

        public void AddDefaultInstanceInitialization(BehaviorClass behaviorClass, DefaultInstanceInitializationRequestFactoryAsync initializationRequestFactory = null)
            => DefaultInstanceInitializationTokens.Add(new DefaultInstanceInitializationToken(behaviorClass, initializationRequestFactory ?? DefaultDefaultInstanceFactory));
    }
}
