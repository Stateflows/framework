namespace Stateflows.Common.Initializer
{
    internal class DefaultInstanceInitializationToken
    {
        public BehaviorClass BehaviorClass { get; private set; }

        public DefaultInstanceInitializationRequestFactoryAsync InitializationRequestFactory { get; }

        public DefaultInstanceInitializationToken(BehaviorClass behaviorClass, DefaultInstanceInitializationRequestFactoryAsync initializationRequestFactoryAsync)
        {
            BehaviorClass = behaviorClass;
            InitializationRequestFactory = initializationRequestFactoryAsync;
        }

        public void RefreshEnvironment()
        {
            BehaviorClass = new BehaviorClass(BehaviorClass.Type, BehaviorClass.Name);
        }
    }
}