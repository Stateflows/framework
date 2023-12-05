namespace Stateflows.Common.Initializer
{
    internal class InitializationToken
    {
        public BehaviorClass BehaviorClass { get; private set; }

        public InitializationRequestFactoryAsync InitializationRequestFactory { get; }

        public InitializationToken(BehaviorClass behaviorClass, InitializationRequestFactoryAsync initializationRequestFactoryAsync)
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