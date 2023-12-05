using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Stateflows.Common.Initializer
{
    internal class BehaviorClassesInitializations
    {
        public static readonly BehaviorClassesInitializations Instance = new BehaviorClassesInitializations();

        public readonly List<InitializationToken> InitializationTokens = new List<InitializationToken>();

        private static readonly InitializationRequestFactoryAsync DefaultFactory = (serviceProvider, behaviorClass) => Task.FromResult(new InitializationRequest());

        public void Initialize(BehaviorClass behaviorClass, InitializationRequestFactoryAsync initializationRequestFactory = null)
            => InitializationTokens.Add(new InitializationToken(behaviorClass, initializationRequestFactory ?? DefaultFactory));
    }
}
