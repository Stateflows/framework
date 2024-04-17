namespace Stateflows.Common.Initializer
{
    internal class AutoInitializationToken
    {
        public BehaviorClass BehaviorClass { get; private set; }

        public AutoInitializationRequestFactoryAsync InitializationRequestFactory { get; }

        public AutoInitializationToken(BehaviorClass behaviorClass, AutoInitializationRequestFactoryAsync initializationRequestFactoryAsync)
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