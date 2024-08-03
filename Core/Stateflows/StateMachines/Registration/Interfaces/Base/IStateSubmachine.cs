namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    public interface IStateSubmachine<out TReturn>
    {
        TReturn AddSubmachine(string submachineName, EmbeddedBehaviorBuildAction buildAction, StateActionInitializationBuilderAsync initializationBuilder = null);
    }
}
