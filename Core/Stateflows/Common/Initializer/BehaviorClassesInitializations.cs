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

        //    public readonly List<AutoInitializationToken> AutoInitializationTokens = new List<AutoInitializationToken>();

        //    private static readonly AutoInitializationRequestFactoryAsync DefaultAutoFactory = (serviceProvider, behaviorClass) => Task.FromResult(new InitializationRequest());

        //    public void AddAutoInitialization(BehaviorClass behaviorClass, AutoInitializationRequestFactoryAsync initializationRequestFactory = null)
        //        => AutoInitializationTokens.Add(new AutoInitializationToken(behaviorClass, initializationRequestFactory ?? DefaultAutoFactory));

        //    public void RefreshEnvironment()
        //    {
        //        foreach (var token in DefaultInstanceInitializationTokens)
        //        {
        //            token.RefreshEnvironment();
        //        }

        //        foreach (var token in AutoInitializationTokens)
        //        {
        //            token.RefreshEnvironment();
        //        }
        //    }
    }
}
